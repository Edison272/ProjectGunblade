using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AttackSystem
{
    /// Status effects carried by an attack (data only)
    public readonly struct StatusSpec
    {
        public readonly float Duration;
        public readonly float SlowMultiplier; // <1 slows, >1 hastes
        public readonly int DotDamage;        // total over Duration
        public readonly int DotTicks;

        public StatusSpec(float duration, float slowMultiplier, int dotDamage, int dotTicks)
        {
            Duration = duration;
            SlowMultiplier = Mathf.Max(0.01f, slowMultiplier); // 0 would freeze the target
            DotDamage = dotDamage;
            DotTicks = Mathf.Max(1, dotTicks);
        }

        public bool HasSlow => Duration > 0f && !Mathf.Approximately(SlowMultiplier, 1f);
        public bool HasDot => Duration > 0f && DotDamage != 0;
    }

    /// Owns active status effects on one character.
    /// Movement should multiply its speed by SpeedMultiplier.
    public class StatusEffectHandler : MonoBehaviour
    {
        IHealth health;
        readonly List<(float multiplier, float expires)> slows = new List<(float, float)>();

        public float SpeedMultiplier { get; private set; } = 1f;

        void Awake()
        {
            TryGetComponent(out health);
        }

        public void Apply(in StatusSpec spec)
        {
            if (spec.HasSlow)
            {
                slows.Add((spec.SlowMultiplier, Time.time + spec.Duration));
                RecalculateSpeed();
            }
            if (spec.HasDot && health != null)
            {
                StartCoroutine(DotRoutine(spec));
            }
        }

        void Update()
        {
            if (slows.Count == 0) return;

            bool changed = false;
            for (int i = slows.Count - 1; i >= 0; i--)
            {
                if (slows[i].expires <= Time.time)
                {
                    slows.RemoveAt(i);
                    changed = true;
                }
            }
            if (changed) RecalculateSpeed();
        }

        // strongest effect wins (lowest multiplier), so slows don't stack multiplicatively
        void RecalculateSpeed()
        {
            float result = 1f;
            for (int i = 0; i < slows.Count; i++)
            {
                if (i == 0 || slows[i].multiplier < result) result = slows[i].multiplier;
            }
            SpeedMultiplier = result;
        }

        IEnumerator DotRoutine(StatusSpec spec)
        {
            var wait = new WaitForSeconds(spec.Duration / spec.DotTicks);
            int dealt = 0;
            for (int tick = 1; tick <= spec.DotTicks; tick++)
            {
                yield return wait;
                // integer split: cumulative total is exact, sub-1 damage per tick just skips ticks
                int cumulative = spec.DotDamage * tick / spec.DotTicks;
                int amount = cumulative - dealt;
                dealt = cumulative;
                if (amount != 0) health.ChangeHealth(amount);
            }
        }
    }
}
using UnityEngine;

namespace AttackSystem
{
    /// Anything an attack can hit implements this. One lookup per hit.
    public interface IAttackable
    {
        void ReceiveHit(in HitInfo hit);
    }

    /// Immutable snapshot of one hit
    public readonly struct HitInfo
    {
        public readonly Vector2 SourcePos;
        public readonly int Damage;
        public readonly float KnockbackForce;
        public readonly StatusSpec Status;

        public HitInfo(Vector2 sourcePos, int damage, float knockbackForce, StatusSpec status)
        {
            SourcePos = sourcePos;
            Damage = damage;
            KnockbackForce = knockbackForce;
            Status = status;
        }
    }

    /// Put on anything hittable (characters, destructible walls).
    /// Caches sibling components once in Awake and routes hits to them.
    public class HitReceiver : MonoBehaviour, IAttackable
    {
        IHealth health;
        IMovement movement;
        StatusEffectHandler status;

        void Awake()
        {
            TryGetComponent(out health);
            TryGetComponent(out movement);
            TryGetComponent(out status);
        }

        public void ReceiveHit(in HitInfo hit)
        {
            health?.ChangeHealth(hit.Damage);

            if (hit.KnockbackForce > 0f && movement != null)
            {
                Vector3 dir = (transform.position - (Vector3)hit.SourcePos).normalized;
                movement.ForceMove(dir, hit.KnockbackForce);
            }

            if (status) status.Apply(hit.Status);
        }
    }
}
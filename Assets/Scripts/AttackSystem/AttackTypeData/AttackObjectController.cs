    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Anything that uses attack objects uses THIS. It contains an array of attack objects. Useful for more attacks with a lot of features
    /// AttackObjects can be safetly accessed using 
    /// </summary>

    [Serializable]
    public class AttackObjectController
    {
        public float hu = 0;
        [SerializeReference] public AttackObject[] AttackObjects = new AttackObject[] {};
        // connects interaction events to attack objects and other attack objects via mapping
        [SerializeField] private AttackObjectMap[] _attackObjectMap = new AttackObjectMap[] {};
        private readonly Dictionary<AttackObject, int> _attackObjIndexes = new Dictionary<AttackObject, int>();
        private List<AttackBehaviorBase> _attackInstanceBuffer = new List<AttackBehaviorBase>();
        public Action GetOnHitEffect;
        public Action GetOnDestroyEffect;

        public void UpdateSerialization()
        {
            for(int i = 0; i < AttackObjects.Length; i++)
            {
                AttackObject attackType = AttackObjects[i];
                if (attackType == null) return;
                if (attackType.UpdateSerialization(out var updated))
                    AttackObjects[i] = updated;
                
                _attackObjIndexes[AttackObjects[i]] = i;
            }
            // for(int i = 0; i < _attackObjectMap.Length; i++)
            // {
            //     AttackObjectMap attackType = _attackObjectMap[i];
            //     if (attackType == null) return;
            //     if (attackType.UpdateSerialization(out var updated))
            //         AttackObjects[i] = updated;
                
            //     _attackObjIndexes[AttackObjects[i]] = i;
            // }
        }
        public void StartAttack(int attackIndex, TargetData targData)
        {
            AttackObject atk_obj = GetAttack(attackIndex);
            atk_obj?.Attack(targData, _attackInstanceBuffer);
            foreach(AttackBehaviorBase atk_instance in _attackInstanceBuffer)
            {
                if (atk_instance)
                    _attackObjectMap[attackIndex].ApplyInteractions(this, atk_instance);
            }

            _attackInstanceBuffer.Clear();
        }
        public AttackObject GetAttack(int mainAttackIndex)
        {
            if (mainAttackIndex >= 0 && mainAttackIndex < AttackObjects.Length)
                return AttackObjects[mainAttackIndex];
            else
                return null;
        }
        public int GetIndex(AttackObject atkObj)
        {
            if (_attackObjIndexes.TryGetValue(atkObj, out int index))
            {
                return index;
            }
            return -1;
        }
    }

    /// <summary>
    /// A helper class to map an AttackObject to the other AttackObjects it will trigger/create through events
    /// </summary>
    [Serializable]
    public class AttackObjectMap
    {
        [Header("Main Object Index")]
        public int attackObjectIndex;
        [Header("Effect Indexes")]
        public int OnHitIndex = -1;
        public int OnDestroyIndex = -1;

        public void ApplyInteractions(AttackObjectController controller, AttackBehaviorBase instance)
        {
            if (OnHitIndex != -1)
            {
                instance.OnHitEffects.Add(controller.AttackObjects[OnHitIndex]);
            }
            if (OnDestroyIndex != -1)
            {
                instance.OnDestroyEffects.Add(controller.AttackObjects[OnDestroyIndex]);
            }
        }
    }
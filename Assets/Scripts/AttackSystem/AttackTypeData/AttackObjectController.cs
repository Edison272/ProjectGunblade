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
            for(int i = 0; i < _attackObjectMap.Length; i++)
            {
                AttackObjectMap objMap = _attackObjectMap[i];
                if (objMap == null) return;
                objMap.AssignInteractions(this);
                _attackObjectMap[i] = objMap;
            }
        }
        public void StartAttack(int attackIndex, TargetData targData)
        {
            AttackObject atk_obj = GetAttack(attackIndex);
            atk_obj?.Attack(targData, _attackInstanceBuffer);

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
        public int[] OnHitIndexes = new int[] {};
        public int[] OnDestroyIndex = new int[] {};

        public void AssignInteractions(AttackObjectController controller)
        {
            AttackObject atkObj = controller.AttackObjects[attackObjectIndex];
            if (atkObj == null) return;

            atkObj.OnHitEffects = AssignAttackArray(OnHitIndexes, controller);
            atkObj.OnDestroyEffects = AssignAttackArray(OnDestroyIndex, controller);
        }
        private AttackObject[] AssignAttackArray(int[] indexArray, AttackObjectController controller)
        {
            if (indexArray != null)
            {
                List<AttackObject> atkObjList = new List<AttackObject>();
                foreach(int index in indexArray)
                {
                    if (index == -1)
                        continue;

                    atkObjList.Add(controller.AttackObjects[index]);
                }
                return atkObjList.ToArray();
            }
            else
            {
                return new AttackObject[0];
            }
        }
    }
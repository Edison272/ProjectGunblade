using UnityEngine;

namespace AttackSystem
{
    /// Anything an attack can hit implements this. One lookup per hit.
    public interface IAttackable
    {
        void ApplyAttack(in TargetData targetData, in AttackStats attack);
    }
}
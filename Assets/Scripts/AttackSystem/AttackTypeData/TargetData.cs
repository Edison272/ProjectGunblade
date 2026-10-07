using UnityEngine;

[System.Serializable]
public enum TargetFaction { Enemies, Allies, Both }

/// Immutable description of where an attack goes and who it's from.
/// Use the With...() methods to derive modified copies.
public readonly struct TargetData
{
    // True position data
    public readonly Vector2 sourcePos;
    public readonly Vector2 targetPos;

    // VFX data (both absolute world positions)
    public readonly Vector2 vfxSourcePos;
    public readonly Vector2 vfxTargetPos; // renamed from vfxTargetOffset; swap in your own name if different
    public float vfxTargetOffset => vfxSourcePos.y - targetPos.y;

    // Entity data
    public readonly LayerMask targetMask;
    public readonly Character owner;
    public readonly Transform objectTarget;

    public TargetData(Vector2 sourcePos, Vector2 targetPos, Vector2 vfxSourcePos, Vector2 vfxTargetPos,
                      LayerMask targetMask = default, Character owner = null, Transform objectTarget = null)
    {
        this.sourcePos = sourcePos;
        this.targetPos = targetPos;
        this.vfxSourcePos = vfxSourcePos;
        this.vfxTargetPos = vfxTargetPos;
        this.targetMask = targetMask;
        this.owner = owner;
        this.objectTarget = objectTarget;
    }
    public TargetData CopyToNewPosition(Vector2 newSourcePos) =>
        new TargetData(newSourcePos, GetDir(), vfxTargetPos, vfxTargetPos + GetDir(), targetMask, owner, objectTarget);

    #region Derivation
    // dervie a version of the target data with an owner
    public TargetData WithSourcePos(Vector2 newSourcePos) =>
        new TargetData(newSourcePos, targetPos, vfxSourcePos, vfxTargetPos, targetMask, owner, objectTarget);
    public TargetData WithTargetPos(Vector2 newTargetPos) =>
        new TargetData(sourcePos, newTargetPos, vfxSourcePos, vfxTargetPos, targetMask, owner, objectTarget);

    public TargetData WithVFXPos(Vector2 vfxSourcePos, Vector2 vfxTargetPos) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetPos, targetMask, owner, objectTarget);

    public TargetData WithOwner(Character newOwner) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetPos, targetMask, newOwner, objectTarget);

    /// Targets a transform: sets objectTarget and snaps targetPos / vfxTargetPos to it
    public TargetData WithObjectTarget(Transform newObjectTarget, Transform objectTargetVFX = null) =>
        new TargetData(sourcePos, newObjectTarget.position, vfxSourcePos,
                       objectTargetVFX ? objectTargetVFX.position : newObjectTarget.position,
                       targetMask, owner, newObjectTarget);
    #endregion

    #region Helpers
    public Vector2 GetDir() => targetPos - sourcePos;
    public Vector2 GetVFXDir() => vfxTargetPos - vfxSourcePos;
    #endregion
}

// used to request target data, with specific requests for the type of targeting
public struct TargetDataRequest
{
    public Vector2 TargetPos;
    public TargetFaction TargetFaction;
    public float HomingRadius;
}
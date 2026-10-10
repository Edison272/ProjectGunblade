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

    // VFX data (both local positions)
    public readonly Vector2 vfxSourcePos;
    public readonly Vector2 vfxTargetOffset;

    // Entity data
    public readonly LayerMask targetMask;
    public readonly Character owner;
    public readonly Transform objectTarget;
    public readonly int RecursionLimit; // if -1, this is a fresh instance

    public TargetData(Vector2 sourcePos, Vector2 targetPos, Vector2 vfxSourcePos, Vector2 vfxTargetOffset,
                      LayerMask targetMask = default, Character owner = null, Transform objectTarget = null, int recursionLimit = -1)
    {
        this.sourcePos = sourcePos;
        this.targetPos = targetPos;
        this.vfxSourcePos = vfxSourcePos;
        this.vfxTargetOffset = vfxTargetOffset;
        this.targetMask = targetMask;
        this.owner = owner;
        this.objectTarget = objectTarget;
        this.RecursionLimit = recursionLimit;
    }
    public TargetData CopyToNewPositions(Vector2 sourcePos, Vector2 targetPos, Vector2 vfxSourcePos, Vector2 vfxTargetOffset) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget);
    public TargetData CopyToNewSourcePos(Vector2 newSourcePos) {
        return new TargetData(newSourcePos, newSourcePos + GetDir(),
                            newSourcePos + vfxTargetOffset, vfxTargetOffset,
                            targetMask, owner, objectTarget);
    }
    public TargetData CopyToNewPositions(Vector2 sourcePos, Vector2 targetPos) {
        return new TargetData(sourcePos, targetPos,
                            sourcePos + vfxTargetOffset, vfxTargetOffset,
                            targetMask, owner, objectTarget);
    }

    #region Derivation
    public TargetData WtihRecursionLimit(int newRecursionLimit) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget, newRecursionLimit);
    // dervie a version of the target data with an owner
    public TargetData WithSourcePos(Vector2 newSourcePos) =>
        new TargetData(newSourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget, RecursionLimit);
    public TargetData WithTargetPos(Vector2 newTargetPos) =>
        new TargetData(sourcePos, newTargetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget, RecursionLimit);

    // Control VFX
    public TargetData WithVFXOffset(Vector2 vfxSourcePos, Vector2 vfxTargetOffset) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget, RecursionLimit);
    public TargetData WithVFXSourcePos(Vector2 vfxSourcePos) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget, RecursionLimit);
    public TargetData WithVFXTargetOffset(Vector2 vfxTargetOffset) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, owner, objectTarget, RecursionLimit);
    // Homing Capabilities
    public TargetData WithOwner(Character newOwner) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, targetMask, newOwner, objectTarget, RecursionLimit);

    /// Targets a transform: sets objectTarget and snaps targetPos / vfxTargetOffset to it
    public TargetData WithObjectTarget(Transform newObjectTarget, Transform objectTargetVFX = null) =>
        new TargetData(sourcePos, newObjectTarget.position, vfxSourcePos,
                       objectTargetVFX ? objectTargetVFX.position : newObjectTarget.position,
                       targetMask, owner, newObjectTarget, RecursionLimit);
    public TargetData WithTargetMask(LayerMask newMask) =>
        new TargetData(sourcePos, targetPos, vfxSourcePos, vfxTargetOffset, newMask, owner, objectTarget, RecursionLimit);
    #endregion

    #region Helpers
    public Vector2 GetDir() => targetPos - sourcePos;
    public Vector2 GetVFXDir() => vfxTargetOffset - vfxSourcePos;
    #endregion
}

// used to request target data, with specific requests for the type of targeting
public struct TargetDataRequest
{
    public Vector2 TargetPos;
    public TargetFaction TargetFaction;
    public float HomingRadius;
}
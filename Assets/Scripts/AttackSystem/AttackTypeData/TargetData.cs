
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[System.Serializable]
public enum TargetFaction {Enemies, Allies, Both}

/// used to determine where an attack goes, and who it's from
[System.Serializable]
public struct TargetData
{
    // True position data
    public Vector2 sourcePos;
    public Vector2 targetPos;

    // VFX Data
    public Vector2 vfxSourcePos;
    public Vector2 vfxTargetOffset;
    
    // Entity Data
    public LayerMask targetMask;
    public Character owner;
    public Transform objectTarget;

    public TargetData(Vector2 src_pos, Vector2 targ_pos, Vector2 vfxSrcPos, Vector2 vfxTargPos, int targMask = 0)
    {
        sourcePos = src_pos;
        targetPos = targ_pos;
        vfxSourcePos = vfxSrcPos;
        vfxTargetOffset = vfxTargPos;
        targetMask = targMask;
        owner = null;
        objectTarget = null;
    }

    public TargetData SetOwner(Character owner)
    {
        this.owner = owner;
        return this;
    }
    public TargetData SetObjectTarget(Transform objectTarget, Transform objectTargetVFX = null)
    {
        this.objectTarget = objectTarget;
        targetPos = objectTarget.transform.position;
        vfxTargetOffset = objectTargetVFX ? objectTargetVFX.position : objectTarget.position;
        return this;
    }

    #region Helpers
    public Vector2 GetDir()
    {
        return targetPos - sourcePos;
    }
    public Vector2 GetVFXDir()
    {
        return vfxTargetOffset - vfxSourcePos;
    }
    #endregion
}


// used to request target data, with specific reqests for the type of targetting
public struct TargetDataRequest
{
    public Vector2 TargetPos;
    public TargetFaction TargetFaction;
    public float HomingRadius; 
}
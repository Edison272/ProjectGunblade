
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// used to determine where an attack goes, and who it's from
[System.Serializable]
public struct AttackTarget
{
    public Vector2 source_pos;
    public Vector2 target_pos;
    public Vector2 output_pos;
    public Vector2 vfx_target_offset;
    public Character sender;

    public AttackTarget(Vector2 src_pos, Vector2 targ_pos, Vector2 out_pos, Vector2 vfx_targ_offset, Character send = null)
    {
        source_pos = src_pos;
        target_pos = targ_pos;
        output_pos = out_pos;
        vfx_target_offset = vfx_targ_offset;
        sender = send;
    }
}
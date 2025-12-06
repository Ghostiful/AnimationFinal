using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyStateBlend : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public delegate float[] a3realOp(float[] v_out, params object[] args);
public delegate a3_BlendOp a3blendOpExecute(a3_BlendOp blendOp);

public enum BlendOpLimits
{
    a3blendOpLimitControl = 16,
    a3blendOpLimitInput = 8
}

// Main blend operation structure
public class a3_BlendOp
{
    // Execution function
    public a3blendOpExecute exec;

    // Blend operation function
    public a3realOp op;

    // Output value
    public float[] v_out;

    // Control value set (max 16)
    public float[][] v_ctrl;

    // Input value set (max 8)
    public float[][] u;

    // Control and input counts
    public ushort vCount;
    public ushort uCount;

    // Constructor
    public a3_BlendOp()
    {
        v_ctrl = new float[(int)BlendOpLimits.a3blendOpLimitControl][];
        u = new float[(int)BlendOpLimits.a3blendOpLimitInput][];
        vCount = 0;
        uCount = 0;
    }
}

public struct a3_BlendOpSet
{
    a3blendOpExecute exec;
    a3realOp op_transformMat;
    a3realOp op_transformDQ;
    a3realOp op_rotate;
    a3realOp op_scale;
    a3realOp op_translate;
    a3realOp op_user;
}

public class a3_SpatialPoseBlendNode
{
    public a3_BlendOpSet blendOpSet;
    public a3_SpatialPose pose_out;
    public a3_SpatialPose[] pose_ctrl;
    public float[][] u;
    public ushort vCount;
    public ushort uCount;

    public a3_SpatialPoseBlendNode()
    {
        pose_ctrl = new a3_SpatialPose[(int)BlendOpLimits.a3blendOpLimitControl];
        u = new float[(int)BlendOpLimits.a3blendOpLimitInput][];
        vCount = 0;
        uCount = 0;
    }

    // Helper methods
    public void SetControlPose(int index, a3_SpatialPose pose)
    {
        if (index >= 0 && index < (int)BlendOpLimits.a3blendOpLimitControl)
        {
            pose_ctrl[index] = pose;
            if (index >= vCount)
                vCount = (ushort)(index + 1);
        }
    }

    public void SetInput(int index, float[] input)
    {
        if (index >= 0 && index < (int)BlendOpLimits.a3blendOpLimitInput)
        {
            u[index] = input;
            if (index >= uCount)
                uCount = (ushort)(index + 1);
        }
    }

    public a3_SpatialPose GetControlPose(int index)
    {
        if (index >= 0 && index < vCount)
            return pose_ctrl[index];
        return default;
    }
}

public class a3_SpatialPoseBlendTree
{
    public a3_Hierarchy blendTreeDescriptor;
    public a3_SpatialPoseBlendNode[] nodes;

    public a3_SpatialPoseBlendTree(int nodeCount)
    {
        nodes = new a3_SpatialPoseBlendNode[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            nodes[i] = new a3_SpatialPoseBlendNode();
        }
    }
}
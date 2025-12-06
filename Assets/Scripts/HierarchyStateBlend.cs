using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public static class HierarchyStateBlend
{
    public static int a3spatialPoseBlendTreeConfigureNode(a3_SpatialPoseBlendTree blendTree, int nodeIndex, a3_SpatialPose outPose, a3_SpatialPose pose1, a3_SpatialPose pose2, a3_BlendOpSet blendOp)
    {
        blendTree.nodes[nodeIndex].pose_out = outPose;
        blendTree.nodes[nodeIndex].pose_ctrl[0] = pose1;
        blendTree.nodes[nodeIndex].pose_ctrl[1] = pose2;
        blendTree.nodes[nodeIndex].vCount = 2;
        blendTree.nodes[nodeIndex].blendOpSet = blendOp;

        return 0;
    }

    public static int a3spatialPoseBlendTreeExecute(a3_SpatialPoseBlendTree blendTree)
    {
        for (uint i = 0; i < blendTree.blendTreeDescriptor.numNodes; i++)
        {
            a3_BlendOp op = new a3_BlendOp();
            float u = 0.5f;

            // Set input
            blendTree.nodes[i].SetInput(0, new float[] { u });
            blendTree.nodes[i].uCount = 1;

            /// ROTATION BLEND
            op.op = blendTree.nodes[i].blendOpSet.op_rotate;
            op.v_out = GetVector4AsArray(ref blendTree.nodes[i].pose_out.rotate);

            // Set controls
            for (uint j = 0; j < blendTree.nodes[i].vCount; j++)
            {
                op.v_ctrl[j] = GetVector4AsArray(ref blendTree.nodes[i].pose_ctrl[j].rotate);
            }
            op.vCount = blendTree.nodes[i].vCount;

            // Set inputs
            for (uint j = 0; j < blendTree.nodes[i].uCount; j++)
            {
                op.u[j] = blendTree.nodes[i].GetInput((int)j);
            }
            op.uCount = blendTree.nodes[i].uCount;

            // Execute blend operation
            op.exec = blendTree.nodes[i].blendOpSet.exec;
            blendTree.nodes[i].blendOpSet.exec(op);

            // Write back result
            SetVector4FromArray(ref blendTree.nodes[i].pose_out.rotate, op.v_out);

            /// TRANSLATION BLEND
            op.op = blendTree.nodes[i].blendOpSet.op_translate;
            op.v_out = GetVector4AsArray(ref blendTree.nodes[i].pose_out.translate);

            // Set controls
            for (int j = 0; j < blendTree.nodes[i].vCount; j++)
            {
                op.v_ctrl[j] = GetVector4AsArray(ref blendTree.nodes[i].pose_ctrl[j].translate);
            }
            op.vCount = blendTree.nodes[i].vCount;

            // Set inputs
            for (int j = 0; j < blendTree.nodes[i].uCount; j++)
            {
                op.u[j] = blendTree.nodes[i].GetInput(j);
            }
            op.uCount = blendTree.nodes[i].uCount;

            // Execute blend operation
            op.exec = blendTree.nodes[i].blendOpSet.exec;
            blendTree.nodes[i].blendOpSet.exec(op);

            // Write back result
            SetVector4FromArray(ref blendTree.nodes[i].pose_out.translate, op.v_out);

            /// SCALE BLEND
            op.op = blendTree.nodes[i].blendOpSet.op_scale;
            op.v_out = GetVector4AsArray(ref blendTree.nodes[i].pose_out.scale);

            // Set controls
            for (int j = 0; j < blendTree.nodes[i].vCount; j++)
            {
                op.v_ctrl[j] = GetVector4AsArray(ref blendTree.nodes[i].pose_ctrl[j].scale);
            }
            op.vCount = blendTree.nodes[i].vCount;

            // Set inputs
            for (int j = 0; j < blendTree.nodes[i].uCount; j++)
            {
                op.u[j] = blendTree.nodes[i].GetInput(j);
            }
            op.uCount = blendTree.nodes[i].uCount;

            // Execute blend operation
            op.exec = blendTree.nodes[i].blendOpSet.exec;
            blendTree.nodes[i].blendOpSet.exec(op);

            // Write back result
            SetVector4FromArray(ref blendTree.nodes[i].pose_out.scale, op.v_out);
        }

        return 1;

    }

    // Helper: Convert Vector4 to float array
    private static float[] GetVector4AsArray(ref Vector4 vec)
    {
        return new float[] { vec.x, vec.y, vec.z, vec.w };
    }

    // Helper: Set Vector4 from float array
    private static void SetVector4FromArray(ref Vector4 vec, float[] array)
    {
        if (array != null && array.Length >= 4)
        {
            vec.x = array[0];
            vec.y = array[1];
            vec.z = array[2];
            vec.w = array[3];
        }
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
    public a3blendOpExecute exec;
    public a3realOp op_transformMat;
    public a3realOp op_transformDQ;
    public a3realOp op_rotate;
    public a3realOp op_scale;
    public a3realOp op_translate;
    public a3realOp op_user;
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

    public float[] GetInput(int index)
    {
        if (index >= 0 && index < uCount)
            return u[index];
        return null;
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
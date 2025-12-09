using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.UIElements.VisualElement;

[System.Serializable]
public class a3_HierarchyPose
{
    // pointer to first spatial pose in owner pool
    public a3_SpatialPose[] poses;

    // index of first spatial pose in owner pool
    public int hpose_index;

    public a3_HierarchyPose(int nodeCount)
    {
        poses = new a3_SpatialPose[nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            poses[i] = new a3_SpatialPose();
        }
        hpose_index = 0;
    }
}

[System.Serializable]
public class a3_HierarchyPoseGroup
{
    // pointer to hierarchy
    public a3_Hierarchy hierarchy;

    // hierarchical spatial poses
    public a3_HierarchyPose[] hpose;

    // complete spatial pose pool
    public a3_SpatialPose[] pose;

    // channels
    public a3_SpatialPoseChannel[] channel;

    // preferred Euler orders
    public a3_SpatialPoseEulerOrder[] order;

    // number of hierarchical poses
    public int hposeCount;

    // total number of spatial poses
    public int poseCount;
}

[System.Serializable]
public class a3_HierarchyState
{
    // pointer to hierarchy
    public a3_Hierarchy hierarchy;

    // collection of poses
    public a3_HierarchyPose[] hpose;

    // active animation pose
    public a3_HierarchyPose animPose => hpose[0];

    // local-space pose (node relative to parent's space)
    public a3_HierarchyPose localSpace => hpose[1];
    
    // object-space pose (node relative to root-parent's space)
    public a3_HierarchyPose objectSpace => hpose[2];
    
    // local-space inverse pose (parent relative to node's space)
    public a3_HierarchyPose localSpaceInv => hpose[3];
    
    // object-space inverse pose (root-parent relative to node's space)
    public a3_HierarchyPose objectSpaceInv => hpose[4];
    
    // object-space bind-to-current pose
    public a3_HierarchyPose objectSpaceBindToCurrent => hpose[5];

    public a3_HierarchyState()
    {
        hpose = new a3_HierarchyPose[6];
    }
}

public static class a3_HierarchyStateFunctions
{
    public static int a3hierarchyPoseGroupCreate(ref a3_HierarchyPoseGroup poseGroup_out, ref a3_Hierarchy hierarchy, int poseCount)
    {
        // validate params and initialization states
        if (poseGroup_out != null && hierarchy != null && poseGroup_out.hierarchy == null && hierarchy.nodes != null)
        {
            // determine memory requirements
            int nodeCount = hierarchy.numNodes;
            int hposeCount = poseCount;
            int sposeCount = hposeCount * nodeCount;

            // allocate everything
            poseGroup_out.hpose = new a3_HierarchyPose[hposeCount];
            poseGroup_out.pose = new a3_SpatialPose[sposeCount];
            poseGroup_out.channel = new a3_SpatialPoseChannel[nodeCount];
            poseGroup_out.order = new a3_SpatialPoseEulerOrder[nodeCount];

            // Initialize spatial poses
            for (int i = 0; i < sposeCount; i++)
                poseGroup_out.pose[i] = new a3_SpatialPose();

            // set pointers
            for (int i = 0; i < hposeCount; ++i)
            {
                poseGroup_out.hpose[i] = new a3_HierarchyPose(nodeCount);
                poseGroup_out.hpose[i].hpose_index = i * nodeCount;

                // Point to correct section of pose array
                for (int j = 0; j < nodeCount; j++)
                {
                    poseGroup_out.hpose[i].poses[j] = poseGroup_out.pose[i * nodeCount + j];
                }
            }

            // reset all data
            //a3_HierarchyStateFunctions.a3hierarchyPoseReset(ref poseGroup_out.hpose[0], nodeCount);

            // Initialize channels and orders with defaults
            for (int i = 0; i < nodeCount; i++)
            {
                poseGroup_out.channel[i] = a3_SpatialPoseChannel.a3poseChannel_none;
                poseGroup_out.order[i] = a3_SpatialPoseEulerOrder.a3poseEulerOrder_xyz;
            }

            poseGroup_out.hierarchy = hierarchy;
            poseGroup_out.hposeCount = hposeCount;
            poseGroup_out.poseCount = sposeCount;

            // done
            return 1;
        }
        return -1;
    }

    public static int a3hierarchyPoseConvert(ref a3_HierarchyPose pose_inout, int nodeCount, a3_SpatialPoseChannel[] channel, a3_SpatialPoseEulerOrder[] order)
    {
        if (pose_inout != null && nodeCount > 0 && channel != null)
        {
            int i;
            for (i = 0; i < nodeCount; i++)
                SpatialPose.a3spatialPoseConvert(ref pose_inout.poses[i], channel[i], order[i]);
            return i;
        }
        return -1;
    }

    public static int a3hierarchyPoseRestore(ref a3_HierarchyPose pose_inout, int nodeCount, a3_SpatialPoseChannel[] channel, a3_SpatialPoseEulerOrder[] order)
    {
        if (pose_inout != null && nodeCount > 0 && channel != null)
        {
            int i;
            for (i = 0; i < nodeCount; i++)
                SpatialPose.a3spatialPoseRestore(ref pose_inout.poses[i], channel[i], order[i]);
            return i;
        }
        return -1;
    }

    public static int a3hierarchyPoseCopy(ref a3_HierarchyPose pose_out, a3_HierarchyPose pose_in, int nodeCount)
    {
        if (pose_out != null && pose_in != null && nodeCount > 0)
        {
            int i;
            for (i = 0; i < nodeCount; i++)
                SpatialPose.a3spatialPoseCopy(ref pose_out.poses[i], pose_in.poses[i]);
            return i;
        }
        return -1;
    }

    public static int a3hierarchyPoseConcat(ref a3_HierarchyPose pose_out, a3_HierarchyPose pose_lhs, a3_HierarchyPose pose_rhs, int nodeCount)
    {
        if (pose_out != null && pose_lhs != null && pose_rhs != null && nodeCount > 0)
        {
            int i;
            for (i = 0; i < nodeCount; ++i)
                SpatialPose.a3spatialPoseConcat(pose_out.poses[i], pose_lhs.poses[i], pose_rhs.poses[i]);
            return i;
        }
        return -1;
    }

    public static int a3hierarchyPoseDeconcat(ref a3_HierarchyPose pose_out, a3_HierarchyPose pose_lhs, a3_HierarchyPose pose_rhs, int nodeCount)
    {
        if (pose_out != null && pose_lhs != null && pose_rhs != null && nodeCount > 0)
        {
            int i;
            for (i = 0; i < nodeCount; ++i)
                SpatialPose.a3spatialPoseDeconcat(pose_out.poses[i], pose_lhs.poses[i], pose_rhs.poses[i]);
            return i;
        }
        return -1;
    }

    public static int a3hierarchyPoseLerp(ref a3_HierarchyPose pose_out, a3_HierarchyPose pose_0, a3_HierarchyPose pose_1, float u, int nodeCount)
    {
        if (pose_out != null && pose_0 != null && pose_1 != null && nodeCount > 0)
        {
            int i;
            for (i = 0; i < nodeCount; ++i)
                SpatialPose.a3spatialPoseLerp(pose_out.poses[i], pose_0.poses[i], pose_1.poses[i], u);
            return i;
        }
        return -1;
    }

    public static int a3hierarchyStateCreate(ref a3_HierarchyState state_out, ref a3_Hierarchy hierarchy)
    {
        // validate params and initialization states
        if (state_out != null && hierarchy != null && state_out.hierarchy == null && hierarchy.nodes != null)
        {
            // determine memory requirements
            int nodeCount = hierarchy.numNodes;
            int hposeCount = 6;
            int sposeCount = hposeCount * nodeCount;

            // allocate everything
            a3_SpatialPose[] allPoses = new a3_SpatialPose[sposeCount];
            for (int i = 0; i < sposeCount; i++)
                allPoses[i] = new a3_SpatialPose();

            // set pointers
            state_out.hierarchy = hierarchy;
            state_out.hpose = new a3_HierarchyPose[hposeCount];

            for (int i = 0; i < hposeCount; ++i)
            {
                state_out.hpose[i] = new a3_HierarchyPose(nodeCount);
                //a3hierarchyPoseGroupLoad(ref state_out., ref state_out.hierarchy);
                state_out.hpose[i].hpose_index = i * nodeCount;

                // Point to correct section of allPoses array
                for (int j = 0; j < nodeCount; j++)
                {
                    state_out.hpose[i].poses[j] = allPoses[i * nodeCount + j];
                }
            }

            // reset all data
            //a3hierarchyPoseReset(ref state_out.hpose[0], nodeCount);

            // done
            return 1;
        }
        return -1;
    }

    public static int a3hierarchyStateUpdateLocalInverse(a3_HierarchyState state)
    {
        if (state != null && state.hierarchy != null)
        {
            int i = 0;
            for (i = 0; i < state.hierarchy.numNodes; ++i)
            {
                state.localSpaceInv.poses[i].transformMat = state.localSpace.poses[i].transformMat.inverse;
            }
            return i;
        }
        return -1;
    }

    public static int a3hierarchyStateUpdateObjectInverse(a3_HierarchyState state)
    {
        if (state != null && state.hierarchy != null)
        {
            int i = 0;
            for (i = 0; i < state.hierarchy.numNodes; ++i)
            {
                state.objectSpaceInv.poses[i].transformMat = state.objectSpace.poses[i].transformMat.inverse;
            }
            return i;
        }
        return -1;
    }

    public static int a3hierarchyStateUpdateObjectBindToCurrent(a3_HierarchyState state, a3_HierarchyState state_bind)
    {
        if (state != null && state.hierarchy != null && state_bind != null && state_bind.hierarchy != null)
        {
            int i = 0;
            for (i = 0; i < state.hierarchy.numNodes; ++i)
            {
                state.objectSpaceBindToCurrent.poses[i].transformMat =
                    state.objectSpace.poses[i].transformMat * state_bind.objectSpaceInv.poses[i].transformMat;
            }
            return i;
        }
        return -1;
    }

    // still needed - function to load bone transforms as an array from the cat prefab
    public static int a3hierarchyPoseGroupLoad(ref a3_HierarchyPoseGroup poseGroup_out, ref a3_Hierarchy hierarchy_out, Bone[] bones)
    {
        /// Hierarchy
        hierarchy_out = a3_Hierarchy.a3hierarchyCreate(bones.Length);

        for (int i = 0; i < bones.Length; i++)
        {
            hierarchy_out.nodes[i].index = i;
            hierarchy_out.nodes[i].name = bones[i].transform.name;
            hierarchy_out.nodes[i].parentIndex = bones[i].parentIndex;

        }

        /// Hierarchy pose group
        a3_HierarchyStateFunctions.a3hierarchyPoseGroupCreate(ref poseGroup_out, ref hierarchy_out, 6);
        // should be happening within the create but isn't for some reason - Izzy
        //poseGroup_out.pose = new a3_SpatialPose[bones.Length];
        //poseGroup_out.hpose[0].poses = new a3_SpatialPose[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            poseGroup_out.hpose[0].poses[i].translate = bones[i].transform.localPosition;
            poseGroup_out.hpose[0].poses[i].translate.w = 1;
            poseGroup_out.hpose[0].poses[i].rotate = bones[i].transform.localRotation.eulerAngles;
            poseGroup_out.hpose[0].poses[i].rotate.w = 1;
            poseGroup_out.hpose[0].poses[i].scale = bones[i].transform.localScale;
            poseGroup_out.pose[i].translate = bones[i].transform.position;
            poseGroup_out.pose[i].rotate = bones[i].transform.rotation.eulerAngles;
            poseGroup_out.pose[i].rotate.w = 1;
            poseGroup_out.pose[i].scale = bones[i].transform.localScale;
        }
        a3hierarchyPoseConvert(ref poseGroup_out.hpose[0], hierarchy_out.numNodes, poseGroup_out.channel, poseGroup_out.order);


        return 1;
    }
}

[System.Serializable]
public struct Bone
{
    public Transform transform;
    public int parentIndex;
}

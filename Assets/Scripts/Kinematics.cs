using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class a3_Kinematics
{
    //-----------------------------------------------------------------------------

    // single FK helpers
    private static void a3kinematicsSolveForwardSingle(a3_HierarchyState hierarchyState, int index, int parentIndex)
    {
        // T[this_object] = T[parent_object] * T[this_local]
        hierarchyState.objectSpace.poses[index].transformMat =
            hierarchyState.objectSpace.poses[parentIndex].transformMat *
            hierarchyState.localSpace.poses[index].transformMat;
    }

    private static void a3kinematicsSolveForwardRoot(a3_HierarchyState hierarchyState, int index)
    {
        // T[root_object] = T[root_local]
        hierarchyState.objectSpace.poses[index].transformMat =
            hierarchyState.localSpace.poses[index].transformMat;
    }

    // partial FK solver
    public static int a3kinematicsSolveForwardPartial(a3_HierarchyState hierarchyState, int firstIndex, int nodeCount)
    {
        if (hierarchyState != null && hierarchyState.hierarchy != null &&
            firstIndex < hierarchyState.hierarchy.numNodes && nodeCount > 0)
        {
            // implement forward kinematics algorithm: 
            //	- for all nodes starting at first index
            //		- if node is not root (has parent node)
            //			- object matrix = parent object matrix * local matrix
            //		- else
            //			- copy local matrix to object matrix

            a3_HierarchyNode[] nodes = hierarchyState.hierarchy.nodes;
            int endIndex = firstIndex + nodeCount;
            int count = 0;

            for (int i = firstIndex; i < endIndex; ++i)
            {
                if (nodes[i].parentIndex >= 0)
                    a3kinematicsSolveForwardSingle(hierarchyState, nodes[i].index, nodes[i].parentIndex);
                else
                    a3kinematicsSolveForwardRoot(hierarchyState, nodes[i].index);
                count++;
            }

            return count;
        }
        return -1;
    }

    //-----------------------------------------------------------------------------

    // single IK helpers
    private static void a3kinematicsSolveInverseSingle(a3_HierarchyState hierarchyState, int index, int parentIndex)
    {
        // T[this_local] = T[parent_object]^-1 * T[this_object]
        hierarchyState.localSpace.poses[index].transformMat =
            hierarchyState.objectSpaceInv.poses[parentIndex].transformMat *
            hierarchyState.objectSpace.poses[index].transformMat;
    }

    private static void a3kinematicsSolveInverseRoot(a3_HierarchyState hierarchyState, int index)
    {
        // T[root_local] = T[root_object]
        hierarchyState.localSpace.poses[index].transformMat =
            hierarchyState.objectSpace.poses[index].transformMat;
    }

    // partial IK solver
    public static int a3kinematicsSolveInversePartial(a3_HierarchyState hierarchyState, int firstIndex, int nodeCount)
    {
        if (hierarchyState != null && hierarchyState.hierarchy != null &&
            firstIndex < hierarchyState.hierarchy.numNodes && nodeCount > 0)
        {
            // implement inverse kinematics algorithm: 
            //	- for all nodes starting at first index
            //		- if node is not root (has parent node)
            //			- local matrix = inverse parent object matrix * object matrix
            //		- else
            //			- copy object matrix to local matrix

            a3_HierarchyNode[] nodes = hierarchyState.hierarchy.nodes;
            int endIndex = firstIndex + nodeCount;
            int count = 0;

            for (int i = firstIndex; i < endIndex; ++i)
            {
                if (nodes[i].parentIndex >= 0)
                    a3kinematicsSolveInverseSingle(hierarchyState, nodes[i].index, nodes[i].parentIndex);
                else
                    a3kinematicsSolveInverseRoot(hierarchyState, nodes[i].index);
                count++;
            }

            return count;
        }
        return -1;
    }

    //-----------------------------------------------------------------------------

    public static void a3kinematicsUpdateHierarchyStateFK(a3_HierarchyState activeHS, a3_HierarchyState baseHS, a3_HierarchyPoseGroup poseGroup)
    {
        if (activeHS.hierarchy == baseHS.hierarchy &&
            activeHS.hierarchy == poseGroup.hierarchy)
        {
            // FK pipeline
            //	-> concatenate base pose
            //	-> convert poses to local-space matrices
            //	-> perform recursive FK

            a3_HierarchyStateFunctions.a3hierarchyPoseConcat(
                activeHS.localSpace,     // local: goal to calculate
                activeHS.animPose,       // holds current sample pose
                baseHS.localSpace,       // holds base pose (animPose is all identity poses)
                activeHS.hierarchy.numNodes);

            a3_HierarchyStateFunctions.a3hierarchyPoseConvert(
                activeHS.localSpace,
                activeHS.hierarchy.numNodes,
                poseGroup.channel,
                poseGroup.order);

            a3kinematicsSolveForwardPartial(activeHS, 0, activeHS.hierarchy.numNodes);
        }
    }

    public static void a3kinematicsUpdateHierarchyStateIK(a3_HierarchyState activeHS, a3_HierarchyState baseHS, a3_HierarchyPoseGroup poseGroup)
    {
        if (activeHS.hierarchy == baseHS.hierarchy &&
            activeHS.hierarchy == poseGroup.hierarchy)
        {
            // IK pipeline
            //	-> perform recursive IK
            //	-> restore local-space matrices to poses
            //	-> deconcatenate base pose

            a3kinematicsSolveInversePartial(activeHS, 0, activeHS.hierarchy.numNodes);

            a3_HierarchyStateFunctions.a3hierarchyPoseRestore(
                activeHS.localSpace,
                activeHS.hierarchy.numNodes,
                poseGroup.channel,
                poseGroup.order);

            a3_HierarchyStateFunctions.a3hierarchyPoseDeconcat(
                activeHS.animPose,       // current sample pose: goal to calculate
                activeHS.localSpace,     // holds local pose
                baseHS.localSpace,       // holds base pose (animPose is all identity poses)
                activeHS.hierarchy.numNodes);
        }
    }

    public static void a3kinematicsUpdateHierarchyStateSkin(a3_HierarchyState activeHS, a3_HierarchyState baseHS)
    {
        if (activeHS.hierarchy == baseHS.hierarchy)
        {
            // FK pipeline extended for skinning and other applications
            //	-> update local-space inverse matrices
            //	-> update object-space inverse matrices
            //	-> update transform from base to current

            a3_HierarchyStateFunctions.a3hierarchyStateUpdateLocalInverse(activeHS);
            a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(activeHS);
            a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectBindToCurrent(activeHS, baseHS);
        }
    }

    //-----------------------------------------------------------------------------

    // helper to resolve single-joint IK after solver
    private static void a3kinematicsResolvePostIK(a3_HierarchyState activeHS,
            a3_HierarchyState baseHS, a3_HierarchyPoseGroup poseGroup,
            int nodeIndex, Matrix4x4 j2obj)
    {
        // post-IK resolution for single affected joint
        //	-> reassign resolved transform to object-space
        //	-> compute object-space inverse matrix
        //	-> compute local-space matrix
        //	-> restore local-space matrix to pose
        //	-> deconcatenate base pose

        // reassign resolved transform to OBJECT-SPACE matrix
        activeHS.objectSpace.poses[nodeIndex].transformMat = j2obj;

        // compute OBJECT-SPACE matrix inverse
        activeHS.objectSpaceInv.poses[nodeIndex].transformMat = j2obj.inverse;

        // solve LOCAL-SPACE matrix
        a3kinematicsSolveInverseSingle(activeHS,
            activeHS.hierarchy.nodes[nodeIndex].index,
            activeHS.hierarchy.nodes[nodeIndex].parentIndex);

        // restore pose
        SpatialPose.a3spatialPoseRestore(activeHS.localSpace.poses[nodeIndex],
            poseGroup.channel[nodeIndex], poseGroup.order[nodeIndex]);

        // deconcat
        SpatialPose.a3spatialPoseDeconcat(activeHS.animPose.poses[nodeIndex],
            activeHS.localSpace.poses[nodeIndex],
            baseHS.localSpace.poses[nodeIndex]);
    }
}

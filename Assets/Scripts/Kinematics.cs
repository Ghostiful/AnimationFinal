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
                ref activeHS.hpose[1],     // local: goal to calculate
                activeHS.animPose,       // holds current sample pose
                baseHS.localSpace,       // holds base pose (animPose is all identity poses)
                activeHS.hierarchy.numNodes);

            a3_HierarchyStateFunctions.a3hierarchyPoseConvert(
                ref activeHS.hpose[1],
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
                ref activeHS.hpose[1],
                activeHS.hierarchy.numNodes,
                poseGroup.channel,
                poseGroup.order);

            a3_HierarchyStateFunctions.a3hierarchyPoseDeconcat(
                ref activeHS.hpose[1],       // current sample pose: goal to calculate
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
    private static void a3kinematicsResolvePostIK(ref a3_HierarchyState activeHS,
            ref a3_HierarchyState baseHS, ref a3_HierarchyPoseGroup poseGroup,
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
        //Debug.Log(nodeIndex);
        SpatialPose.a3spatialPoseRestore(ref activeHS.localSpace.poses[nodeIndex],
            poseGroup.channel[nodeIndex], poseGroup.order[nodeIndex]);

        // deconcat
        SpatialPose.a3spatialPoseDeconcat(activeHS.animPose.poses[nodeIndex],
            activeHS.localSpace.poses[nodeIndex],
            baseHS.localSpace.poses[nodeIndex]);
    }

    public static void a3kinematicsUpdateLookAtIK(a3_HierarchyState sceneGraphState,
            a3_HierarchyState activeHS, a3_HierarchyState baseHS, a3_HierarchyPoseGroup poseGroup,
            int sceneGraphIndex_hierarchyObj, int sceneGraphIndex_effector,
            int hierarchyObjIndex_affected)
    {
        Matrix4x4 m_hierarchyObj_4x4 = Matrix4x4.identity;
        Matrix4x4 m_affected_4x4 = Matrix4x4.identity;

        if ((sceneGraphState == null || activeHS == null || baseHS == null || poseGroup == null) ||
            (activeHS.hierarchy != baseHS.hierarchy) ||
            (activeHS.hierarchy != poseGroup.hierarchy))
            return;

        // need to properly transform joints to their parent frame and vice-versa
        // get the hierarchy root object transform relative to the rig
        Matrix4x4 obj2rig = sceneGraphState.localSpace.poses[sceneGraphIndex_hierarchyObj].transformMat;
        Matrix4x4 rig2obj = sceneGraphState.localSpaceInv.poses[sceneGraphIndex_hierarchyObj].transformMat;

        // affected joint relative to hierarchy
        Matrix4x4 j2obj_affected = activeHS.objectSpace.poses[hierarchyObjIndex_affected].transformMat;

        // SOLVER
        {
            // affected joint relative to rig
            Matrix4x4 j2rig_affected = obj2rig * j2obj_affected;

            // affected joint position in rig
            Vector3 affectedPos_rig = j2rig_affected.GetColumn(3);

            // effector locator position in rig
            Vector3 effectorPos_rig = sceneGraphState.localSpace.poses[sceneGraphIndex_effector].transformMat.GetColumn(3);

            // bases to form
            Vector3 right = m_hierarchyObj_4x4.GetColumn(0);
            Vector3 fwd = m_hierarchyObj_4x4.GetColumn(1);
            Vector3 up = m_hierarchyObj_4x4.GetColumn(2);

            // compute look-at basis
            {
                // compute bases
                fwd = (effectorPos_rig - affectedPos_rig).normalized;
                right = Vector3.Cross(fwd, up).normalized;
                up = Vector3.Cross(right, fwd);

                // convert to matrix
                Matrix4x4 r_affected = Matrix4x4.identity;
                r_affected.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
                r_affected.SetColumn(1, new Vector4(fwd.x, fwd.y, fwd.z, 0));
                r_affected.SetColumn(2, new Vector4(up.x, up.y, up.z, 0));

                // map to joint orientation
                Matrix4x4 m_affected_transpose = m_affected_4x4.transpose;
                r_affected = r_affected * m_affected_transpose;

                // put it back in hierarchy object space
                j2rig_affected.SetColumn(0, r_affected.GetColumn(0));
                j2rig_affected.SetColumn(1, r_affected.GetColumn(1));
                j2rig_affected.SetColumn(2, r_affected.GetColumn(2));
            }

            j2obj_affected = rig2obj * j2rig_affected;
        }

        // RESOLVE IK (single-chain)
        a3kinematicsResolvePostIK(ref activeHS, ref baseHS, ref poseGroup, hierarchyObjIndex_affected, j2obj_affected);
    }

    public static void a3kinematicsUpdateLimbIK(a3_HierarchyState sceneGraphState,
        a3_HierarchyState activeHS, a3_HierarchyState baseHS, a3_HierarchyPoseGroup poseGroup,
        int sceneGraphIndex_hierarchyObj, int sceneGraphIndex_effector_end, int sceneGraphIndex_constraint,
        int hierarchyObjIndex_affected_end, int hierarchyObjIndex_affected_hinge, int hierarchyObjIndex_affected_base)
    {
        Matrix4x4 m_hierarchyObj_4x4 = Matrix4x4.identity;
        Matrix4x4 m_affected_end_4x4 = Matrix4x4.identity;
        Matrix4x4 m_affected_hinge_4x4 = Matrix4x4.identity;
        Matrix4x4 m_affected_base_4x4 = Matrix4x4.identity;

        if ((sceneGraphState == null || activeHS == null || baseHS == null || poseGroup == null) ||
            (activeHS.hierarchy != baseHS.hierarchy) ||
            (activeHS.hierarchy != poseGroup.hierarchy))
            return;

        // need to properly transform joints to their parent frame and vice-versa
        // get the hierarchy root object transform relative to the rig
        Matrix4x4 obj2rig = sceneGraphState.localSpace.poses[sceneGraphIndex_hierarchyObj].transformMat;
        Matrix4x4 rig2obj = sceneGraphState.localSpaceInv.poses[sceneGraphIndex_hierarchyObj].transformMat;

        // affected joints relative to hierarchy
        Matrix4x4 j2obj_affected_end = activeHS.objectSpace.poses[hierarchyObjIndex_affected_end].transformMat;
        Matrix4x4 j2obj_affected_hinge = activeHS.objectSpace.poses[hierarchyObjIndex_affected_hinge].transformMat;
        Matrix4x4 j2obj_affected_base = activeHS.objectSpace.poses[hierarchyObjIndex_affected_base].transformMat;

        // SOLVER
        {
            // affected joints relative to rig
            Matrix4x4 j2rig_affected_end = obj2rig * j2obj_affected_end;
            Matrix4x4 j2rig_affected_hinge = obj2rig * j2obj_affected_hinge;
            Matrix4x4 j2rig_affected_base = obj2rig * j2obj_affected_base;

            // affected joint positions in rig
            Vector3 affectedPos_end_rig = j2rig_affected_end.GetColumn(3);
            Vector3 affectedPos_hinge_rig = j2rig_affected_hinge.GetColumn(3);
            Vector3 affectedPos_base_rig = j2rig_affected_base.GetColumn(3);

            // effector and constraint positions in rig
            Vector3 effectorPos_end_rig = sceneGraphState.localSpace.poses[sceneGraphIndex_effector_end].transformMat.GetColumn(3);
            Vector3 constraintPos_rig = sceneGraphState.localSpace.poses[sceneGraphIndex_constraint].transformMat.GetColumn(3);

            // determine if solution exists
            Vector3 upperDiff = affectedPos_base_rig - affectedPos_hinge_rig;
            Vector3 lowerDiff = affectedPos_hinge_rig - affectedPos_end_rig;
            Vector3 effectorDiff = effectorPos_end_rig - affectedPos_base_rig;
            Vector3 constraintDiff = constraintPos_rig - affectedPos_base_rig;
            Vector3 normal = Vector3.Cross(constraintDiff, effectorDiff).normalized;

            float upperDist = upperDiff.magnitude;
            float lowerDist = lowerDiff.magnitude;
            float effectorDist = effectorDiff.magnitude;
            effectorDiff = effectorDiff.normalized;
            float maxDist = upperDist + lowerDist;

            if (effectorDist >= maxDist)
            {
                // simple solution: end goes to farthest possible point, hinge also easy to solve
                affectedPos_end_rig = affectedPos_base_rig + effectorDiff * maxDist;
                affectedPos_hinge_rig = affectedPos_base_rig + effectorDiff * upperDist;
            }
            else
            {
                // not-so-simple solution: while wrist position is solved, need elbow
                // use properties of triangles to get location
                // area of triangle using Heron's formula
                float s = 0.5f * (effectorDist + maxDist);
                float area = Mathf.Sqrt(s * (s - effectorDist) * (s - upperDist) * (s - lowerDist));
                float height = 2.0f * area / effectorDist;
                float baseLen = Mathf.Sqrt(upperDist * upperDist - height * height);

                Vector3 offset = Vector3.Cross(effectorDiff, normal) * height;
                affectedPos_hinge_rig = affectedPos_base_rig + effectorDiff * baseLen + offset;
                affectedPos_end_rig = effectorPos_end_rig;
            }

            // bases to form
            Vector3 right = m_hierarchyObj_4x4.GetColumn(0);
            Vector3 fwd = m_hierarchyObj_4x4.GetColumn(1);

            // compute base node basis
            {
                fwd = (affectedPos_hinge_rig - affectedPos_base_rig).normalized;
                right = Vector3.Cross(fwd, normal);

                Matrix4x4 r_affected = Matrix4x4.identity;
                r_affected.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
                r_affected.SetColumn(1, new Vector4(fwd.x, fwd.y, fwd.z, 0));
                r_affected.SetColumn(2, new Vector4(normal.x, normal.y, normal.z, 0));

                Matrix4x4 m_affected_base_transpose = m_affected_base_4x4.transpose;
                r_affected = r_affected * m_affected_base_transpose;

                j2rig_affected_base.SetColumn(0, r_affected.GetColumn(0));
                j2rig_affected_base.SetColumn(1, r_affected.GetColumn(1));
                j2rig_affected_base.SetColumn(2, r_affected.GetColumn(2));
            }

            // compute hinge node basis
            {
                fwd = (affectedPos_end_rig - affectedPos_hinge_rig).normalized;
                right = Vector3.Cross(fwd, normal);

                Matrix4x4 r_affected = Matrix4x4.identity;
                r_affected.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
                r_affected.SetColumn(1, new Vector4(fwd.x, fwd.y, fwd.z, 0));
                r_affected.SetColumn(2, new Vector4(normal.x, normal.y, normal.z, 0));

                Matrix4x4 m_affected_hinge_transpose = m_affected_hinge_4x4.transpose;
                r_affected = r_affected * m_affected_hinge_transpose;

                j2rig_affected_hinge.SetColumn(0, r_affected.GetColumn(0));
                j2rig_affected_hinge.SetColumn(1, r_affected.GetColumn(1));
                j2rig_affected_hinge.SetColumn(2, r_affected.GetColumn(2));
                j2rig_affected_hinge.SetColumn(3, new Vector4(affectedPos_hinge_rig.x, affectedPos_hinge_rig.y, affectedPos_hinge_rig.z, 1));
            }

            // update end node basis
            {
                j2rig_affected_end.SetColumn(3, new Vector4(affectedPos_end_rig.x, affectedPos_end_rig.y, affectedPos_end_rig.z, 1));
            }

            j2obj_affected_end = rig2obj * j2rig_affected_end;
            j2obj_affected_hinge = rig2obj * j2rig_affected_hinge;
            j2obj_affected_base = rig2obj * j2rig_affected_base;
        }

        // RESOLVE IK (multi-chain: work from root to leaf to get correct transformations)
        a3kinematicsResolvePostIK(ref activeHS, ref baseHS, ref poseGroup, hierarchyObjIndex_affected_base, j2obj_affected_base);
        a3kinematicsResolvePostIK(ref activeHS, ref baseHS, ref poseGroup, hierarchyObjIndex_affected_hinge, j2obj_affected_hinge);
        a3kinematicsResolvePostIK(ref activeHS, ref baseHS, ref poseGroup, hierarchyObjIndex_affected_end, j2obj_affected_end);
    }
}

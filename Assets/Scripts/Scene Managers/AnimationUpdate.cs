using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationUpdate
{
    public static int ApplyEffectors(ref a3_HierarchyState sceneGraphState, ref a3_HierarchyState activeHS, ref a3_HierarchyState baseHS, ref a3_HierarchyPoseGroup poseGroup)
    {
        if (sceneGraphState == null || activeHS == null || baseHS == null || poseGroup == null)
        {
            Debug.Assert(sceneGraphState != null);
            //Debug.Assert(activeHS != null);
            //Debug.Assert(baseHS != null);
            Debug.Assert(poseGroup != null);
            return -1;
        }

        a3_Basis basis_obj = BasisUtil.a3basisInit(a3_BasisAxis.basis_yp, a3_BasisAxis.basis_zp);
        a3_Basis basis_neck = BasisUtil.a3basisInit(a3_BasisAxis.basis_zp, a3_BasisAxis.basis_yp);


        // Neck look-at
        a3_Kinematics.a3kinematicsUpdateLookAtIK(ref sceneGraphState, ref activeHS, ref baseHS, ref poseGroup, 0, sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_neckLookat_ctrl"), activeHS.hierarchy.a3hierarchyGetNodeIndex("neck"), basis_obj, basis_neck);

        /// Front Limbs
        // Right
        a3_Basis basis_wrist = BasisUtil.a3basisInit(a3_BasisAxis.basis_zp, a3_BasisAxis.basis_yp);
        a3_Basis basis_elbow = BasisUtil.a3basisInit(a3_BasisAxis.basis_yn, a3_BasisAxis.basis_xp);
        a3_Basis basis_shoulder = basis_elbow;
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristEff_r_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristCon_r_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_right_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_right_3"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_right_1"),
            basis_obj, basis_wrist, basis_elbow, basis_shoulder);
        // Left
        basis_elbow = BasisUtil.a3basisInit(a3_BasisAxis.basis_yn, a3_BasisAxis.basis_xp);
        basis_shoulder = basis_elbow;
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristEff_l_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristCon_l_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_left_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_left_3"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_left_1"),
            basis_obj, basis_wrist, basis_elbow, basis_shoulder);

        /// Back Limbs
        // Right
        a3_Basis basis_ankle = BasisUtil.a3basisInit(a3_BasisAxis.basis_zp, a3_BasisAxis.basis_yp);
        a3_Basis basis_knee = BasisUtil.a3basisInit(a3_BasisAxis.basis_yn, a3_BasisAxis.basis_xp);
        a3_Basis basis_hip = basis_knee;
        
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleEff_r_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleCon_r_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_right_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_right_3"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_right_1"),
            basis_obj, basis_ankle, basis_knee, basis_hip);
        // Left
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleEff_l_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleCon_l_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_left_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_left_3"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_left_1"),
            basis_obj, basis_ankle, basis_knee, basis_hip);

        /// Tail
        a3_Basis basis_tailEnd = BasisUtil.a3basisInit(a3_BasisAxis.basis_yp, a3_BasisAxis.basis_zp);
        a3_Basis basis_tailMiddle = BasisUtil.a3basisInit(a3_BasisAxis.basis_yn, a3_BasisAxis.basis_xp);
        a3_Basis basis_tailBase = basis_tailMiddle;
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_tailEff_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_tailCon_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("tail_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("tail_3"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("tail_1"),
            basis_obj, basis_tailEnd, basis_tailMiddle, basis_tailBase);

        return 1;
    }

    public static void UpdateSkeleton(ref a3_HierarchyState sceneGraphState, ref a3_HierarchyState activeHS, ref a3_HierarchyState baseHS, ref a3_HierarchyPoseGroup poseGroup, ref Bone[] bones, bool usePysics)
    {
        if (usePysics)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].transform.localPosition = activeHS.animPose.poses[i].translate;
                bones[i].transform.localRotation = new Quaternion(activeHS.animPose.poses[i].rotate.x, activeHS.animPose.poses[i].rotate.y, activeHS.animPose.poses[i].rotate.z, 1);
            }
        }
        else
        {
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].transform.position = activeHS.animPose.poses[i].translate;
                bones[i].transform.rotation = new Quaternion(activeHS.animPose.poses[i].rotate.x, activeHS.animPose.poses[i].rotate.y, activeHS.animPose.poses[i].rotate.z, 1);
            }
        }
        
    }

    public static void DoAnimationPipeline(ref a3_HierarchyState sceneGraphState, ref a3_HierarchyState activeHS, ref a3_HierarchyState activeHS_fk, ref a3_HierarchyState activeHS_ik, ref a3_HierarchyState baseHS, ref a3_HierarchyPoseGroup poseGroup, ref Bone[] bones)
    {
        // add clip controller update here
        // add blend tree here
        a3_Kinematics.a3kinematicsUpdateHierarchyStateFK(ref activeHS_fk, ref baseHS, ref poseGroup);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref activeHS_ik.hpose[0], activeHS_fk.animPose, activeHS_ik.hierarchy.numNodes);
        a3_Kinematics.a3kinematicsUpdateHierarchyStateFK(ref activeHS_ik, ref baseHS, ref poseGroup);

        a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(activeHS);
        ApplyEffectors(ref sceneGraphState, ref activeHS_ik, ref baseHS, ref poseGroup);
        //a3_Kinematics.a3kinematicsUpdateHierarchyStateIK(ref activeHS_ik, ref baseHS, ref poseGroup);

        a3_HierarchyStateFunctions.a3hierarchyPoseLerp(ref activeHS.hpose[0], activeHS_fk.animPose, activeHS_ik.animPose, 0.5f, activeHS.hierarchy.numNodes);
        // Lerp between fk and ik for final result


        a3_Kinematics.a3kinematicsUpdateHierarchyStateFK(ref activeHS, ref baseHS, ref poseGroup);
        a3_Kinematics.a3kinematicsUpdateHierarchyStateSkin(ref activeHS, ref baseHS);

    }
}

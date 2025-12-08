using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationUpdate
{
    public static int ApplyEffectors(a3_HierarchyState sceneGraphState, a3_HierarchyState activeHS, a3_HierarchyState baseHS, a3_HierarchyPoseGroup poseGroup)
    {
        if (sceneGraphState == null || activeHS == null || baseHS == null || poseGroup == null)
        {
            Debug.Assert(sceneGraphState != null);
            //Debug.Assert(activeHS != null);
            //Debug.Assert(baseHS != null);
            Debug.Assert(poseGroup != null);
            return -1;
        }

        // Neck look-at
        a3_Kinematics.a3kinematicsUpdateLookAtIK(sceneGraphState, activeHS, baseHS, poseGroup, 0, 3, activeHS.hierarchy.a3hierarchyGetNodeIndex("neck"));

        /// Front Limbs
        // Right
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristEff_r_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristCon_r_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_right_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_right_2"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_right_1"));
        // Left
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristEff_l_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_wristCon_l_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_left_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_left_2"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_front_left_1"));

        /// Back Limbs
        // Right
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleEff_r_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleCon_r_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_right_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_right_2"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_right_1"));
        // Left
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleEff_l_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_ankleCon_l_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_left_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_left_2"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("leg_back_left_1"));

        /// Tail
        a3_Kinematics.a3kinematicsUpdateLimbIK(sceneGraphState, activeHS, baseHS, poseGroup, 0,
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_tailEff_ctrl"),
            sceneGraphState.hierarchy.a3hierarchyGetNodeIndex("scene_skeleton_tailCon_ctrl"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("tail_4_end"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("tail_3"),
            activeHS.hierarchy.a3hierarchyGetNodeIndex("tail_1"));

        return 1;
    }

    public static void UpdateAnimation()
    {

    }
}

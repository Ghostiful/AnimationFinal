using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLoad : MonoBehaviour
{
    [Header("Animation Data")]
    public bool useCharacter = true;
    public bool forceDisableStreaming = false;

    [Header("Scene Objects")]
    public SkinnedMeshRenderer characterRenderer;
    public Transform cameraTransform;
    public Transform characterRoot;
    public Bone[] characterBones;

    [Header("IK Effectors")]
    public Transform neckLookAtCtrl;
    public Transform wristEffectorR;
    public Transform wristConstraintR;
    public Transform wristEffectorL;
    public Transform wristConstraintL;
    public Transform ankleEffectorR;
    public Transform ankleConstraintR;
    public Transform ankleEffectorL;
    public Transform ankleConstraintL;
    public Transform tailEffector;
    public Transform tailConstraint;
    public Bone[] effectors;

    [Header("Animation Clips")]
    public string[] clipNames = new string[]
    {
        "xbot_idle_f",
        "xbot_idle_m",
        "xbot_idle_pistol"
    };

    // Core animation systems
    private a3_Hierarchy sceneGraph;
    private a3_HierarchyState sceneGraphState;

    private a3_Hierarchy hierarchy_skel;
    private a3_HierarchyPoseGroup hierarchyPoseGroup_skel;

    private a3_HierarchyState hierarchyState_skel_ik;
    private a3_HierarchyState hierarchyState_skel_fk;
    private a3_HierarchyState hierarchyState_skel_final;
    private a3_HierarchyState hierarchyState_skel_base;

    // Blend tree states
    private a3_Hierarchy blendTree;
    private a3_HierarchyState[] hierarchyState_skel_blend;

    // Clip controllers
    private a3_ClipController clipCtrl_idle_f;
    private a3_ClipController clipCtrl_idle_m;
    private a3_ClipController clipCtrl_idle_p;

    private a3_ClipPool clipPool;

    // Unity bone mapping
    private Transform[] boneTransforms;
    private Dictionary<string, int> boneNameToIndex;

    // Flags
    public bool isLoaded { get; private set; }
    private bool streaming;

    void Start()
    {
        LoadAnimationSystem();
    }

    private void Update()
    {
        AnimationUpdate.DoAnimationPipeline(ref sceneGraphState, ref hierarchyState_skel_final, ref hierarchyState_skel_fk, ref hierarchyState_skel_ik, ref hierarchyState_skel_base, ref hierarchyPoseGroup_skel, ref characterBones);
        AnimationUpdate.UpdateSkeleton(ref sceneGraphState, ref hierarchyState_skel_final, ref hierarchyState_skel_base, ref hierarchyPoseGroup_skel, ref characterBones);
    }

    public void LoadAnimationSystem()
    {
        // Initialize scene graph
        InitializeSceneGraph();

        // Initialize skeletal hierarchy
        InitializeCharacterHierarchy();

        // Initialize animation clips
        InitializeAnimationClips();

        // Initialize hierarchy states
        InitializeHierarchyStates();

        // Initialize blend tree
        InitializeBlendTree();

        isLoaded = true;
        Debug.Log("Animation system loaded");
    }

    void InitializeSceneGraph()
    {
        const int sceneObjectCount = 26;
        sceneGraph = a3_Hierarchy.a3hierarchyCreate(sceneObjectCount + characterBones.Length);

        // Set up scene graph structure
        sceneGraph.a3hierarchySetNode(0, -1, "scene_world_root");

        sceneGraph.a3hierarchySetNode(1, 0, "scene_skeleton_ctrl");
        sceneGraph.a3hierarchySetNode(2, 1, "scene_skeleton_rig");
        sceneGraph.a3hierarchySetNode(3, 2, "scene_skeleton_neckLookat_ctrl");
        sceneGraph.a3hierarchySetNode(4, 2, "scene_skeleton_wristEff_r_ctrl");
        sceneGraph.a3hierarchySetNode(5, 2, "scene_skeleton_wristCon_r_ctrl");
        sceneGraph.a3hierarchySetNode(6, 2, "scene_skeleton_wristEff_l_ctrl");
        sceneGraph.a3hierarchySetNode(7, 2, "scene_skeleton_wristCon_l_ctrl");
        sceneGraph.a3hierarchySetNode(8, 2, "scene_skeleton_ankleEff_r_ctrl");
        sceneGraph.a3hierarchySetNode(9, 2, "scene_skeleton_ankleCon_r_ctrl");
        sceneGraph.a3hierarchySetNode(10, 2, "scene_skeleton_ankleEff_l_ctrl");
        sceneGraph.a3hierarchySetNode(11, 2, "scene_skeleton_ankleCon_l_ctrl");
        sceneGraph.a3hierarchySetNode(12, 2, "scene_skeleton_tailEff_ctrl");
        sceneGraph.a3hierarchySetNode(13, 2, "scene_skeleton_tailCon_ctrl");
        sceneGraph.a3hierarchySetNode(14, 1, "scene_skeleton");
        //for (int i = 0; i < characterBones.Length; i++)
        //{
        //    sceneGraph.a3hierarchySetNode(i + 15, characterBones[i].parentIndex + 15, characterBones[i].transform.name);
        //}
        a3_HierarchyPoseGroup effectorGroup = new a3_HierarchyPoseGroup();
        a3_HierarchyStateFunctions.a3hierarchyPoseGroupCreate(ref effectorGroup, ref sceneGraph, sceneGraph.numNodes);
        //a3_HierarchyStateFunctions.a3hierarchyPoseGroupLoad(ref effectorGroup, ref sceneGraph, effectors);
        for (int i = 0; i < effectors.Length; i++)
        {
            effectorGroup.hpose[0].poses[i + 3].translate = effectors[i].transform.position;
            effectorGroup.hpose[0].poses[i + 3].rotate = effectors[i].transform.rotation.eulerAngles;
            effectorGroup.hpose[0].poses[i + 3].rotate.w = 1;
            effectorGroup.hpose[0].poses[i + 3].scale = effectors[i].transform.localScale;
        }

        // Create scene graph state
        sceneGraphState = new a3_HierarchyState();
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref sceneGraphState, ref sceneGraph);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref sceneGraphState.hpose[0], effectorGroup.hpose[0], sceneGraph.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref sceneGraphState.hpose[1], effectorGroup.hpose[0], sceneGraph.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref sceneGraphState.hpose[2], effectorGroup.hpose[0], sceneGraph.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref sceneGraphState.hpose[0], sceneGraph.numNodes, effectorGroup.channel, effectorGroup.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref sceneGraphState.hpose[1], sceneGraph.numNodes, effectorGroup.channel, effectorGroup.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref sceneGraphState.hpose[2], sceneGraph.numNodes, effectorGroup.channel, effectorGroup.order);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateLocalInverse(sceneGraphState);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(sceneGraphState);



    }

    void InitializeCharacterHierarchy()
    {
        // Izzy put ur hierarchy loading stuff here
        hierarchyPoseGroup_skel = new a3_HierarchyPoseGroup();
        hierarchy_skel = new a3_Hierarchy();
        hierarchy_skel = a3_Hierarchy.a3hierarchyCreate(characterBones.Length);
        a3_HierarchyStateFunctions.a3hierarchyPoseGroupLoad(ref hierarchyPoseGroup_skel, ref hierarchy_skel, characterBones);


    }

    void InitializeAnimationClips()
    {
        // Calculate required storage
        int hierarchySampleCount = hierarchyPoseGroup_skel.hposeCount > 0 ? hierarchyPoseGroup_skel.hposeCount : 1;
        int hierarchyKeyframeCount = hierarchySampleCount - 1;

        // For now, create minimal clip pool for testing
        // TODO: Load actual clip data from files or Unity AnimationClips
        clipPool = new a3_ClipPool();
        a3_KeyframeAnimation.a3clipPoolCreate(clipPool,
            clipNames.Length, // clip count
            hierarchyKeyframeCount, // keyframe count
            hierarchySampleCount); // sample count

        // Initialize placeholder clips
        const int fps = 24;
        double playbackRate = (double)fps;

        for (int i = 0; i < clipNames.Length; i++)
        {
            // Create sample and keyframe for this clip
            if (i < clipPool.sampleCount - 1)
            {
                a3_Sample sample0 = clipPool.sample[i];
                a3_Sample sample1 = clipPool.sample[i + 1];

                sample0.index = i;
                sample1.index = i + 1;

                a3_KeyframeAnimation.a3sampleInit(ref sample0, i * 10, playbackRate);
                a3_KeyframeAnimation.a3sampleInit(ref sample1, (i + 1) * 10, playbackRate);

                clipPool.sample[i] = sample0;
                clipPool.sample[i + 1] = sample1;

                if (i < clipPool.keyframeCount)
                {
                    a3_Keyframe keyframe = clipPool.keyframe[i];
                    keyframe.index = i;
                    a3_KeyframeAnimation.a3keyframeInit(ref keyframe, sample0, sample1, playbackRate);
                    clipPool.keyframe[i] = keyframe;
                }
            }

            if (i < clipPool.clipCount)
            {
                a3_Clip clip = clipPool.clip[i];
                a3_KeyframeAnimation.a3clipInit(clip, clipNames[i],
                    clipPool.keyframe[0], clipPool.keyframe[clipPool.keyframeCount - 1]);
                a3_KeyframeAnimation.a3clipCalculateDuration(clipPool, i, playbackRate);
            }
        }

        // Initialize controllers
        clipCtrl_idle_f = new a3_ClipController();
        clipCtrl_idle_m = new a3_ClipController();
        clipCtrl_idle_p = new a3_ClipController();

        KeyframeAnimationController.a3clipControllerInit(
            clipCtrl_idle_f, "ctrl_idle_f", clipPool, 0, fps, playbackRate);
        KeyframeAnimationController.a3clipControllerInit(
            clipCtrl_idle_m, "ctrl_idle_m", clipPool, 1, fps, playbackRate);
        KeyframeAnimationController.a3clipControllerInit(
            clipCtrl_idle_p, "ctrl_idle_p", clipPool, 2, fps, playbackRate);
    }

    void InitializeHierarchyStates()
    {
        // Base state
        hierarchyState_skel_base = new a3_HierarchyState();
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref hierarchyState_skel_base, ref hierarchy_skel);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_base.hpose[0], hierarchyPoseGroup_skel.hpose[0], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_base.hpose[1], hierarchyPoseGroup_skel.hpose[1], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_base.hpose[2], hierarchyPoseGroup_skel.hpose[2], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_base.hpose[0], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_base.hpose[1], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_base.hpose[2], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_Kinematics.a3kinematicsSolveForwardPartial(ref hierarchyState_skel_base, 0, hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateLocalInverse(hierarchyState_skel_base);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(hierarchyState_skel_base);

        // FK state
        hierarchyState_skel_fk = new a3_HierarchyState();
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref hierarchyState_skel_fk, ref hierarchy_skel);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_fk.hpose[0], hierarchyPoseGroup_skel.hpose[0], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_fk.hpose[1], hierarchyPoseGroup_skel.hpose[1], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_fk.hpose[2], hierarchyPoseGroup_skel.hpose[2], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_fk.hpose[0], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_fk.hpose[1], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_fk.hpose[2], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_Kinematics.a3kinematicsSolveForwardPartial(ref hierarchyState_skel_fk, 0, hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateLocalInverse(hierarchyState_skel_fk);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(hierarchyState_skel_fk);


        // IK state
        hierarchyState_skel_ik = new a3_HierarchyState();
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref hierarchyState_skel_ik, ref hierarchy_skel);
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref hierarchyState_skel_ik, ref hierarchy_skel);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_ik.hpose[0], hierarchyPoseGroup_skel.hpose[0], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_ik.hpose[1], hierarchyPoseGroup_skel.hpose[1], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_ik.hpose[2], hierarchyPoseGroup_skel.hpose[2], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_ik.hpose[0], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_ik.hpose[1], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_ik.hpose[2], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_Kinematics.a3kinematicsSolveForwardPartial(ref hierarchyState_skel_ik, 0, hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateLocalInverse(hierarchyState_skel_ik);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(hierarchyState_skel_ik);

        // Final state
        hierarchyState_skel_final = new a3_HierarchyState();
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref hierarchyState_skel_final, ref hierarchy_skel);
        a3_HierarchyStateFunctions.a3hierarchyStateCreate(ref hierarchyState_skel_final, ref hierarchy_skel);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_final.hpose[0], hierarchyPoseGroup_skel.hpose[0], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_final.hpose[1], hierarchyPoseGroup_skel.hpose[1], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseCopy(ref hierarchyState_skel_final.hpose[2], hierarchyPoseGroup_skel.hpose[2], hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_final.hpose[0], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_final.hpose[1], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_HierarchyStateFunctions.a3hierarchyPoseConvert(ref hierarchyState_skel_final.hpose[2], hierarchy_skel.numNodes, hierarchyPoseGroup_skel.channel, hierarchyPoseGroup_skel.order);
        a3_Kinematics.a3kinematicsSolveForwardPartial(ref hierarchyState_skel_final, 0, hierarchy_skel.numNodes);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateLocalInverse(hierarchyState_skel_final);
        a3_HierarchyStateFunctions.a3hierarchyStateUpdateObjectInverse(hierarchyState_skel_final);


    }

    /// <summary>
    /// Initialize blend tree hierarchy and states
    /// </summary>
    void InitializeBlendTree()
    {
        // Create blend tree hierarchy
        blendTree = a3_Hierarchy.a3hierarchyCreate(5);
        blendTree.a3hierarchySetNode(0, -1, "blendTree_result");
        blendTree.a3hierarchySetNode(1, 0, "blendTree_idle_fm_blend");
        blendTree.a3hierarchySetNode(2, 1, "blendTree_idle_f");
        blendTree.a3hierarchySetNode(3, 1, "blendTree_idle_m");
        blendTree.a3hierarchySetNode(4, 0, "blendTree_idle_p");

        // Create blend tree states
        hierarchyState_skel_blend = new a3_HierarchyState[5];
        for (int i = 0; i < 5; i++)
        {
            hierarchyState_skel_blend[i] = new a3_HierarchyState();
            a3_HierarchyStateFunctions.a3hierarchyStateCreate(
                ref hierarchyState_skel_blend[i], ref hierarchy_skel);
        }
    }

}

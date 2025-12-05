using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Schema;
using UnityEngine;

public class HierarchyState : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int a3hierarchyPoseGroupCreate(a3_HierarchyPoseGroup poseGroup_out, a3_Hierarchy hierarchy, int poseCount)
    {


        return 0;
    }

}

struct a3_HierarchyPose
{
    a3_SpatialPose hpose_base;
    int hpose_index;
}

struct a3_HierarchyPoseGroup
{
    a3_Hierarchy hierarchy;
    Vector<a3_HierarchyPose> hpose;
    Vector<a3_SpatialPose> pose;
    Vector<a3_SpatialPoseChannel> channel;
    int hposeCount;
    int poseCount;
}

struct a3_HierarchyState
{
    a3_Hierarchy hierarchy;

    // active animation pose
    a3_HierarchyPose[] animPose;

    // local-space pose (node relative to parent's space)
    a3_HierarchyPose[] localSpace;

    // object-space pose (node relative to root-parent's space)
    a3_HierarchyPose[] objectSpace;

    // local-space inverse pose (parent relative to node's space)
    a3_HierarchyPose[] localSpaceInv;

    // object-space inverse pose (root-parent relative to node's space)
    a3_HierarchyPose[] objectSpaceInv;

    // object-space bind-to-current pose
    a3_HierarchyPose[] objectSpaceBindToCurrent;
}

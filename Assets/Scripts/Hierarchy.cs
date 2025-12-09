using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.UIElements.VisualElement;

[System.Serializable]
public struct a3_HierarchyNode
{
    public string name;
    public int index;
    public int parentIndex; // -1 if root

    public a3_HierarchyNode(string name, int index, int parentIndex)
    {
        this.name = name;
        this.index = index;
        this.parentIndex = parentIndex;
    }
}

public class a3_Hierarchy
{
    public a3_HierarchyNode[] nodes;
    public int numNodes;

    //-----------------------------------------------------------------------------
    public static a3_Hierarchy a3hierarchyCreate(int numNodes, string[] names = null)
    {
        if (numNodes <= 0)
        {
            Debug.LogError("A3: Invalid node count for hierarchy");
            return null;
        }

        a3_Hierarchy hierarchy = new a3_Hierarchy();
        hierarchy.nodes = new a3_HierarchyNode[numNodes];

        // Initialize nodes with default or provided names
        for (int i = 0; i < numNodes; i++)
        {
            string nodeName = (names != null && i < names.Length && !string.IsNullOrEmpty(names[i]))
                ? names[i]
                : $"a3node_{i}";

            hierarchy.nodes[i] = new a3_HierarchyNode(nodeName, i, -1);
        }
        hierarchy.numNodes = numNodes;

        return hierarchy;
    }

    public int a3hierarchySetNode(int index, int parentIndex, string name)
    {
        if (nodes == null || index < 0 || index >= nodes.Length)
        {
            // Invalid hierarchy or index
            return -1;
        }

        if (index <= parentIndex)
        {
            // Hierarchy node's index must be greater than its parent's
            return -1;
        }

        nodes[index] = new a3_HierarchyNode(name, index, parentIndex);
        return index;
    }

    public int a3hierarchyGetNodeIndex(string name)
    {
        if (nodes == null || string.IsNullOrEmpty(name))
            return -1;

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].name == name)
                return i;
        }

        return -1;
    }

    public bool a3hierarchyIsParentNode(int parentIndex, int otherIndex)
    {
        if (nodes == null || parentIndex < 0 || otherIndex < 0 ||
            parentIndex >= nodes.Length || otherIndex >= nodes.Length)
            return false;

        return nodes[otherIndex].parentIndex == parentIndex;
    }

    public bool a3hierarchyIsChildNode(int childIndex, int otherIndex)
    {
        return a3hierarchyIsParentNode(otherIndex, childIndex);
    }

    public bool a3hierarchyIsSiblingNode(int siblingIndex, int otherIndex)
    {
        if (nodes == null || siblingIndex < 0 || otherIndex < 0 ||
            siblingIndex >= nodes.Length || otherIndex >= nodes.Length)
            return false;

        return nodes[siblingIndex].parentIndex == nodes[otherIndex].parentIndex;
    }

    public bool a3hierarchyIsAncestorNode(int ancestorIndex, int otherIndex)
    {
        if (nodes == null || ancestorIndex < 0 || otherIndex < 0 ||
            ancestorIndex >= nodes.Length || otherIndex >= nodes.Length)
            return false;

        int currentParent = nodes[otherIndex].parentIndex;
        while (currentParent >= 0)
        {
            if (currentParent == ancestorIndex)
                return true;
            currentParent = nodes[currentParent].parentIndex;
        }

        return false;
    }

    public bool a3hierarchyIsDescendantNode(int descendantIndex, int otherIndex)
    {
        return a3hierarchyIsAncestorNode(otherIndex, descendantIndex);
    }
}
﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using NodeGraph;
using NodeGraph.DataModel;
using BridgeUI;

[CustomNodeView(typeof(PanelNode))]
public class PanelNodeView : NodeView
{
    protected static UIType? uiTypeTemplate;
    protected static NodeType nodeTypeTemplate;

    protected PanelNodeBase panelNode { get { return target as PanelNodeBase; } }

    public override int Style
    {
        get
        {
            return panelNode.style;
        }
    }
    public override string Category
    {
        get
        {
            return "panel";
        }
    }
    public override float CustomNodeHeight
    {
        get
        {
            if (panelNode != null && !string.IsNullOrEmpty(panelNode.Info.discription))
            {
                return EditorGUIUtility.singleLineHeight + 5;
            }
            return 0;
        }
    }
    public override void OnInspectorGUI(NodeGUI gui)
    {
        base.OnInspectorGUI(gui);
        if (target != null){
            gui.Name = (target as PanelNode).assetName;
        }
    }
    public override void OnNodeGUI(Rect position, NodeData data)
    {
        base.OnNodeGUI(position, data);
        if (panelNode != null && !string.IsNullOrEmpty(panelNode.Info.discription))
        {
            var rect = new Rect(position.x + 20, position.y, position.width - 40, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(rect, panelNode.Info.discription);
        }
    }
    public override void OnClickNodeGUI(NodeGUI nodeGUI, Vector2 mousePosition, ConnectionPointData result)
    {
        base.OnClickNodeGUI(nodeGUI, mousePosition, result);
        if (panelNode == null) return;
        var nodeInfo = panelNode.nodeInfo;
        if (nodeInfo.prefab){
            EditorGUIUtility.PingObject(nodeInfo.prefab);
        }
    }
    public override void OnContextMenuGUI(GenericMenu menu, NodeGUI gui)
    {
        base.OnContextMenuGUI(menu, gui);
        menu.AddItem(new GUIContent("Copy UIType"), false, () =>
        {
            var nodeItem = (target as PanelNode);
            uiTypeTemplate = nodeItem.nodeInfo.uiType;
        });
        menu.AddItem(new GUIContent("Paste UIType"), false, () =>
        {
            var nodeItem = (target as PanelNode);
            nodeItem.nodeInfo.uiType = (UIType)uiTypeTemplate;
        });
    }
}

using NodeEditorFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.TextureWang.Scripts.Nodes;

[Node(false, "Util/Comment")]
public class CommentNode : Node
{
    public const string ID = "nodeComment";
    public override string GetID { get { return ID; } }

    public string m_Comment;

    

    public FloatRemap m_Width;
    public FloatRemap m_Height;
    public FloatRemap m_FontSize;

    public override Node Create(Vector2 pos)
    {

        CommentNode node = CreateInstance<CommentNode>();

        node.rect = new Rect(pos.x, pos.y, 300, 100);
        node.name = "Comment";
        node.m_Width = new FloatRemap(300, 0, 4000);
        node.m_Height = new FloatRemap(100, 0, 500);
        node.m_FontSize = new FloatRemap(20, 0, 100);
        return node;
    }

    protected internal override void CopyScriptableObjects(System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {

        TextureNode.ConnectRemapFloats(this, replaceSerializableObject);
    }

    protected internal override void DrawNode()
    {
        // TODO: Node Editor Feature: Custom Windowing System
        // Create a rect that is adjusted to the editor zoom
        Rect nodeRect = rect;
        nodeRect.position += NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
        contentOffset = new Vector2(0, 20);
        var was = GUI.color;
        GUI.color = GetTitleBoxColor();
        bool selected = NodeEditor.curEditorState.selectedNode == this || NodeEditor.curEditorState.selectedNodes.Contains(this);
        if (selected)
            GUI.color = Color.white;
        Rect bodyRect = new Rect(nodeRect.x, nodeRect.y + contentOffset.y, Mathf.Max(m_Width,50), Mathf.Max(m_Height,50) - contentOffset.y);

        Rect headerRect = new Rect(nodeRect.x, nodeRect.y, Mathf.Max(nodeRect.width,50), contentOffset.y);
        if (selected)
        {
            //draw a large selection rect
            Rect bigger = bodyRect;
            bigger.x -= 3;
            bigger.y -= 3;
            bigger.y -= headerRect.height;
            bigger.size += new Vector2(6, 6 + headerRect.height);

            GUI.BeginGroup(bigger, GUI.skin.label);
            bigger.position = Vector2.zero;
            if (ms_SelectedBox == null)
            {
                ms_SelectedBox = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                ms_SelectedBox.SetPixel(0, 0, Color.yellow);
                ms_SelectedBox.wrapMode = TextureWrapMode.Repeat;
                ms_SelectedBox.Apply();

            }
            GUI.DrawTexture(bigger, ms_SelectedBox, ScaleMode.StretchToFill); //ScaleMode.StretchToFill);

            GUI.EndGroup();

        }
        NodeEditorGUI.nodeBox.normal.textColor = Color.white;
        // Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold

        GUI.Label(headerRect, name,
            selected ? NodeEditorGUI.nodeBoxBold : NodeEditorGUI.nodeBox);
        GUI.color = was;




        int wasFS = GUI.skin.textArea.fontSize;
        GUI.skin.textArea.fontSize = (int)m_FontSize;
        

        GUI.TextArea(bodyRect,m_Comment);
        GUI.skin.textArea.fontSize = wasFS;
    }
    protected internal override void NodeGUI()
    {

    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        
        m_Comment =GUILayout.TextArea( m_Comment, GUILayout.Height(100), GUILayout.Width(300));
        m_Width.SliderLabel(this, "Text Box Width");
        m_Height.SliderLabel(this, "text Box Height");
        m_FontSize.SliderLabel(this, "Font Size");

        rect.width = Mathf.Max(m_Width,50);
        rect.height = Mathf.Max(m_Height,50);


    }

}

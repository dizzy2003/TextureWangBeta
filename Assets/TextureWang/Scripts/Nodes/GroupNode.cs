#if POO
using System.Collections.Generic;
using System.Reflection;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;

[Node(false, "GroupNode")]
public class GroupNode : Node
{
    protected int m_NodeWidth = 256;
    protected int m_NodeHeight = 256;

    public List<Node> m_Children=new List<Node>();

    public const string ID = "GroupNode";

    

    public override string GetID
    {
        get { return ID; }
    }

    public override Node Create(Vector2 pos)
    {

        GroupNode node = CreateInstance<GroupNode>();

        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "Group";
        node.m_CanDragSize = true;
        node.m_MainBoxColor=Color.cyan;
        node.m_MainBoxColor.a = 0.5f;
        return node;
    }

    protected internal override void CopyScriptableObjects(
        System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {
        // Get the fields of the specified class.
        FieldInfo[] myField = this.GetType().GetFields();
        foreach (var x in myField)
        {
            // if(x.FieldType is FloatRemap)
            if (x.GetValue(this) is FloatRemap)
            {
                FloatRemap fr = (FloatRemap) x.GetValue(this); //its a struct this makes a copy
                //                Debug.Log(x + " " + x.FieldType + " " + x.GetType() + " " + x.ReflectedType+" fr val:"+fr.m_Value);

                if (fr.m_Replacement != null)
                {
                    NodeKnob knob = fr.m_Replacement;
                    NodeInput replace = replaceSerializableObject.Invoke(knob) as NodeInput;
                    if (replace != null)
                    {
                        fr.m_Replacement = replace;
                    }
                    else
                    {
                        fr.m_Replacement = null;
                    }
                }
                x.SetValue(this, fr); //its a god damn struct it needs to be saved back out

            }
        }
    }
/*
    protected internal override void DrawNode(bool _fromParent = false)
    {
        base.DrawNode(false);
        foreach (var x in m_Children)
        {
            if(x!=null)
                x.DrawNode(true);
        }
    }
*/
    protected internal override void DrawConnections()
    {

    }

    protected override void InitBase()
    {
        base.InitBase();

    }

    protected internal override void NodeGUI()
    {
    }
    private void OnGUI()
    {
        NodeGUI();
    }


    public override void DrawNodePropertyEditor()
    {
        m_TitleBoxColor = EditorGUILayout.ColorField("Title Color",m_TitleBoxColor);
        m_MainBoxColor =EditorGUILayout.ColorField("Main Color",m_MainBoxColor);

    }

    protected virtual void PostDrawNodePropertyEditor()
    {
    }

    public override bool Calculate()
    {
        return true;
    }
}
#endif
using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework;
using UnityEngine;

[System.Serializable]
[Node (false, "Float/MathOp")]
public class MathOp : Node 
{
    public enum CalcType { Cos,Sin,Tan,Floor,Frac,Sqrt,Clamp01 }
    public CalcType m_Type = CalcType.Cos;

    public const string ID = "MathOp";
    public override string GetID { get { return ID; } }


    public FloatRemap m_Value1;
    

    public override Node Create (Vector2 pos) 
    {
        MathOp node = CreateInstance <MathOp> ();
		
        node.name = "MathOp";
        node.rect = new Rect (pos.x, pos.y, 100, 50);
        node.m_Value1=new FloatRemap(0,-1,10);
        


        node.CreateOutput ("Output 1", "Float");

        return node;
    }
    protected internal override void CopyScriptableObjects(System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {
        TextureNode.ConnectRemapFloats(this, replaceSerializableObject);
    }

    protected internal override void NodeGUI () 
    {
        GUILayout.BeginHorizontal ();
        GUILayout.BeginVertical ();
        GUILayout.Label("Val1:"+(float)m_Value1);
        //		if (Inputs [0].connection != null)
        //			GUILayout.Label (Inputs [0].name);
        //		else
        //			Input1Val = RTEditorGUI.FloatField (GUIContent.none, Input1Val);
        //		InputKnob (0);
        // --
        //		if (Inputs [1].connection != null)
        //			GUILayout.Label (Inputs [1].name);
        //		else
        //			Input2Val = RTEditorGUI.FloatField (GUIContent.none, Input2Val);
        //		InputKnob (1);

        GUILayout.EndVertical ();
        GUILayout.BeginVertical ();

        Outputs [0].DisplayLayout ();

        GUILayout.EndVertical ();
        GUILayout.EndHorizontal ();

#if UNITY_EDITOR
        m_Type = (CalcType)UnityEditor.EditorGUILayout.EnumPopup (new GUIContent ("Calculation Type", "The type of calculation performed on Input 1 and Input 2"), m_Type);
#else
		GUILayout.Label (new GUIContent ("Calculation Type: " + type.ToString (), "The type of calculation performed on Input 1 and Input 2"));
#endif

        if (GUI.changed)
            NodeEditor.RecalculateFrom (this);
    }


    public override void DrawNodePropertyEditor()
    {
#if UNITY_EDITOR
        m_Type = (CalcType)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1 "), m_Type);
#else
		GUILayout.Label (new GUIContent ("Calculation Type: " + type.ToString (), "The type of calculation performed on Input 1 and Input 2"));
#endif

        m_Value1.SliderLabel(this, "Value1");
        

    }
    public override bool Calculate () 
    {

        switch (m_Type) 
        {
            case CalcType.Cos:
                Outputs[0].SetValue<float> (Mathf.Cos(m_Value1));
                break;
            case CalcType.Sin:
                Outputs[0].SetValue<float>(Mathf.Sin(m_Value1));
                break;
            case CalcType.Tan:
                Outputs[0].SetValue<float>(Mathf.Tan(m_Value1));
                break;
            case CalcType.Floor:
                Outputs[0].SetValue<float>(Mathf.Floor(m_Value1));
                break;
            case CalcType.Frac:
                Outputs[0].SetValue<float>(m_Value1-Mathf.Floor(m_Value1));
                break;
            case CalcType.Sqrt:
                Outputs[0].SetValue<float>(Mathf.Sqrt(m_Value1));
                break;
            case CalcType.Clamp01:
                Outputs[0].SetValue<float>(Mathf.Clamp01(m_Value1));
                break;
        }

        return true;
    }
}
using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework;
using UnityEngine;

[System.Serializable]
[Node (false, "Float/Calculation")]
public class CalcNodeExtended : Node 
{
    public enum CalcType { Add, Substract, Multiply, Divide,Mod,Min,Max,Pow }
    public CalcType type = CalcType.Add;

    public const string ID = "calcNodeExtended";
    public override string GetID { get { return ID; } }

//	public float Input1Val = 1f;
//	public float Input2Val = 1f;

    public FloatRemap m_Value1;
    public FloatRemap m_Value2;

    public override Node Create (Vector2 pos) 
    {
        CalcNodeExtended node = CreateInstance <CalcNodeExtended> ();
		
        node.name = "Calc Node";
        node.rect = new Rect (pos.x, pos.y, 200, 100);
        node.m_Value1=new FloatRemap(0,-1,1);
        node.m_Value2 = new FloatRemap(0, -1, 1);


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
        GUILayout.Label("Val1:"+(float)m_Value1+" Val2:"+(float)m_Value2);


        GUILayout.EndVertical ();
        GUILayout.BeginVertical ();
        if(Outputs[0]!=null)
            Outputs [0].DisplayLayout ();

        GUILayout.EndVertical ();
        GUILayout.EndHorizontal ();

#if UNITY_EDITOR
        type = (CalcType)UnityEditor.EditorGUILayout.EnumPopup (new GUIContent ("Calculation Type", "The type of calculation performed on Input 1 and Input 2"), type);
#else
		GUILayout.Label (new GUIContent ("Calculation Type: " + type.ToString (), "The type of calculation performed on Input 1 and Input 2"));
#endif

        if (GUI.changed)
            NodeEditor.RecalculateFrom (this);
    }


    public override void DrawNodePropertyEditor()
    {
#if UNITY_EDITOR
        type = (CalcType)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Calculation Type", "The type of calculation performed on Input 1 and Input 2"), type);
#else
		GUILayout.Label (new GUIContent ("Calculation Type: " + type.ToString (), "The type of calculation performed on Input 1 and Input 2"));
#endif

        m_Value1.SliderLabel(this, "Value1");
        m_Value2.SliderLabel(this, "Value2");

    }
    public override bool Calculate () 
    {
/*
		if (Inputs[0].connection != null)
			Input1Val = Inputs[0].connection.GetValue<float> ();
		if (Inputs[1].connection != null)
			Input2Val = Inputs[1].connection.GetValue<float> ();
*/
        switch (type) 
        {
            case CalcType.Add:
                Outputs[0].SetValue<float> (m_Value1 + m_Value2);
                break;
            case CalcType.Substract:
                Outputs[0].SetValue<float> (m_Value1 - m_Value2);
                break;
            case CalcType.Multiply:
                Outputs[0].SetValue<float> (m_Value1 * m_Value2);
                break;
            case CalcType.Divide:
                Outputs[0].SetValue<float> (m_Value1 / m_Value2);
                break;
            case CalcType.Mod:
                float div = m_Value1/m_Value2;
                Outputs[0].SetValue<float>((div-Mathf.Floor(div))*m_Value2);
                break;
            case CalcType.Min:
                Outputs[0].SetValue<float>(Mathf.Min(m_Value1, m_Value2));
                break;
            case CalcType.Max:
                Outputs[0].SetValue<float>(Mathf.Max(m_Value1, m_Value2));
                break;
            case CalcType.Pow:
                Outputs[0].SetValue<float>(Mathf.Pow(m_Value1, m_Value2));
                break;
        }

        return true;
    }
}
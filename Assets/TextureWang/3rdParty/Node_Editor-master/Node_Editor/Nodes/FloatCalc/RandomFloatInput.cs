using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[System.Serializable]
[Node (false, "Float/RandInput")]
public class RandomFloatInput : Node 
{
    public const string ID = "RandomFloatInput";
    public override string GetID { get { return ID; } }

    
    public FloatRemap m_Min ;
    public FloatRemap m_Max ;
    public FloatRemap m_Seed;
    protected internal override void CopyScriptableObjects(System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {
        TextureNode.ConnectRemapFloats(this, replaceSerializableObject);
    }

    public override Node Create (Vector2 pos) 
    { // This function has to be registered in Node_Editor.ContextCallback
        RandomFloatInput node = CreateInstance <RandomFloatInput> ();
		
        node.name = "RandomFloatInput";
        node.rect = new Rect (pos.x, pos.y, 100, 80);;
        node.m_Seed=new FloatRemap(Random.value,0,1);
        node.m_Min = new FloatRemap(0, -1, 1);
        node.m_Max = new FloatRemap(1, -1, 1);
        node.m_Seed.m_Mult = 9999999;
        NodeOutput.Create (node, "Value", "Float");

        return node;
    }

    private float m_LastValue;
    protected internal override void NodeGUI () 
    {
        GUILayout.Label("Min:"+(float)m_Min);
        GUILayout.Label("Max:" + (float)m_Max);
        GUILayout.Label("LastUsed:" + m_LastValue);
        //OutputKnob (0);

    }
    public override void DrawNodePropertyEditor()
    {
        m_Min.SliderLabel(this, "Min");
        m_Max.SliderLabel(this, "Max");
        m_Seed.SliderLabel(this, "Seed");

    }
    public override bool Calculate ()
    {
        m_LastValue = Random.Range(m_Min, m_Max);
//        Debug.Log(" Random Float Value: "+m_LastValue);
        Outputs[0].SetValue<float> (m_LastValue);
        return true;
    }
}
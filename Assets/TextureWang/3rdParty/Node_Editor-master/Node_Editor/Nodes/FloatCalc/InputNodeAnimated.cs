using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework;
using UnityEngine;

[System.Serializable]
[Node (false, "Float/InputFloatAnimated")]
public class InputNodeAnimated : Node 
{
    public const string ID = "inputNodeAnimated";
    public override string GetID { get { return ID; } }

	
    public FloatRemap m_Value;

    public override Node Create (Vector2 pos) 
    { // This function has to be registered in Node_Editor.ContextCallback
        InputNodeAnimated node = CreateInstance <InputNodeAnimated> ();
		
        node.name = "Input Float Animated";
        node.rect = new Rect (pos.x, pos.y, 100, 50);;
        node.m_Value=new FloatRemap(1,-1,1);
        NodeOutput.Create (node, "Value", "Float");

        return node;
    }
    protected internal override void CopyScriptableObjects(System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
    {
        TextureNode.ConnectRemapFloats(this, replaceSerializableObject);
    }


    protected internal override void NodeGUI () 
    {
        //value = RTEditorGUI.FloatField (new GUIContent ("Value", "The input value of type float"), value);
        GUILayout.Label("Value:" + (float)m_Value);
        OutputKnob (0);

    }

    public override void DrawNodePropertyEditor()
    {
        m_Value.SliderLabel(this, "Value");

    }
    public override bool Calculate () 
    {
//        Debug.Log("InputNode set output "+((float)m_Value));
        Outputs[0].SetValue<float> (m_Value);
        return true;
    }
}
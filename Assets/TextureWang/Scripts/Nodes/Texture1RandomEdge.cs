using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/RandomEdge")]
    public class Texture1RandomEdge : TextureMathOp
    {
        public const string ID = "Texture1RandomEdge";
        public override string GetID { get { return ID; } }

        public FloatRemap m_DestMin;

        public override Node Create (Vector2 pos) 
        {

            Texture1RandomEdge node = CreateInstance<Texture1RandomEdge> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "RandomEdge";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.RandomEdge;
            node.m_Value1=new FloatRemap(1.0f,0.0f,10.0f);
            node.m_Value2 = new FloatRemap(0.1f, 1.0f, 1.0f);
            node.m_Value3 = new FloatRemap(0.1f, 0.0f, 1.0f);
            node.m_DestMin = new FloatRemap(0.2f, 0.0f, 1.0f);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Dist");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"Min Edge Val to Find");//, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"Prob To add adjacent pixel");//, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            m_DestMin.SliderLabel(this, "Dest Min Add Mask");



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_DestMin.m_Value, 0,0,0));
        }
    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/DotProd")]
    public class Texture1Dot : TextureMathOp
    {
        public const string ID = "Texture1Dot";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Dot node = CreateInstance<Texture1Dot> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Dot (Cos of angle)";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Dot;
            node.m_Value1=new FloatRemap(0,0,1);
            node.m_Value2 = new FloatRemap(0, 0, 1);
            node.m_Value3 = new FloatRemap(1, 0, 1);
            return node;
        }
        Color m_Col ;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"X");//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"Y"); //,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"Z"); //,new GUIContent("Red", "Float"), m_R);

        }

    }
}
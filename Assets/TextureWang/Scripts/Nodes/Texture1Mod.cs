using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Mod")]
    public class Texture1Mod : TextureMathOp
    {
        public const string ID = "Texture1Mod";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Mod node = CreateInstance<Texture1Mod> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Mod";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Stepify;
            node.m_Value1 = new FloatRemap(0.5f, 0, 1);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"StepSize");//,m_Value1, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.Set(m_Value1);
            m_Value3.Set(m_Value1);




        }

    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Cos")]
    public class Texture1Cos : TextureMathOp
    {
        public const string ID = "Texture1Cos";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Cos node = CreateInstance<Texture1Cos> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Cos";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Cos;
            node.m_Value1 = new FloatRemap(1f, 0, 10);
            node.m_Value2 = new FloatRemap(1f, 0, 10);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"ScaleAngle");
            m_Value2.SliderLabel(this, "ScaleResult");
        }

    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "OneInput/EdgeDistFixedDir")]
    public class Texture2EdgeDistFixedDir : TextureMathOp
    {
        private const string m_Help = "look in a fixed direction and record distance to that edge";
        public override string GetHelp() { return m_Help; }
        public const string ID = "EdgeDistFixedDir";
        public override string GetID { get { return ID; } }

        public override Node Create(Vector2 pos)
        {

            Texture2EdgeDistFixedDir node = CreateInstance<Texture2EdgeDistFixedDir>();

            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "EdgeDistFixedDir";
            node.CreateInputOutputs();
            node.m_OpType = MathOp.EdgeDistFixedDir;
            node.m_Value1 = new FloatRemap(20.0f, 0, 500);//dist
            node.m_Value2 = new FloatRemap(0.5f, 0, 1); //value to find
            node.m_Value3 = new FloatRemap(0, 0, 360);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this, "Distance");
            m_Value2.SliderLabel(this, "MinValueToFindDistTo"); 
            m_Value3.SliderLabel(this, "Direction");



        }

    }
}
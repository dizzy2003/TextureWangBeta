using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Step")]
    public class Texture1Step : TextureMathOp
    {
        private const string m_Help = "Everthing below Threshold is black, everything above is white";
        public override string GetHelp() { return m_Help; }
        public const string ID = "Texture1Step";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Step node = CreateInstance<Texture1Step> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Step";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Step;
            node.m_Value1 = new FloatRemap(0.4f, 0, 1);
            node.m_Value2 = new FloatRemap(0.4f, 0, 1);
            node.m_Value3 = new FloatRemap(0.4f, 0, 1);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Red Threshold");//,new GUIContent("Red", "Float"), m_R);
            if (m_TexMode == TexMode.ColorRGB)
            {
                m_Value2.SliderLabel(this, "Green Threshold"); //,new GUIContent("Red", "Float"), m_R);
                m_Value3.SliderLabel(this, "Blue Threshold"); //,new GUIContent("Red", "Float"), m_R);
            }



        }

    }
}
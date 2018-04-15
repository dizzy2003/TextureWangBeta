using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Power")]
    public class Texture1Power : TextureMathOp
    {
        public const string ID = "Texture1Power";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Power node = CreateInstance<Texture1Power> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Power";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Power;
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Red");//,m_Value1, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            if (m_TexMode == TexMode.ColorRGB)
            {
                m_Value2.SliderLabel(this,"Green");//, m_Value2, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
                m_Value3.SliderLabel(this,"Blue");//, m_Value3, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            }
            else
            {
                m_Value2.Set(m_Value1);
                m_Value3.Set(m_Value1);
            }



        }

    }
}
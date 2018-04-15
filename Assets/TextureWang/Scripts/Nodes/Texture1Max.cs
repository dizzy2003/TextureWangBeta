using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Max")]
    public class Texture1Max : TextureMathOp
    {
        public const string ID = "Texture1Max";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Max node = CreateInstance<Texture1Max> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Max";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Max;
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Red");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            if (m_TexMode == TexMode.ColorRGB)
            {
                m_Value2.SliderLabel(this,"Green");//, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
                m_Value3.SliderLabel(this,"Blue");//, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            }
            else
            {
                m_Value2.Set(m_Value1);
                m_Value3.Set(m_Value1);
            }



        }

    }
}
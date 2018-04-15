using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Invert")]
    public class Texture1Invert : TextureMathOp
    {
        public const string ID = "Texture1Invert";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Invert node = CreateInstance<Texture1Invert> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Invert";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Invert;
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
/*
        m_Value1.SliderLabel(this,"Red");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
        if (m_TexMode == TexMode.ColorRGB)
        {
            m_Value2.SliderLabel(this,"Green");//, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"Blue");//, -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
        }
*/


        }

    }
}
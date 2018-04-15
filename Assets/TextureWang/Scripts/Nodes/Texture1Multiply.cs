using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Multiply")]
    public class Texture1Multiply : TextureMathOp
    {
        public const string ID = "Texture1Multiply";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Multiply node = CreateInstance<Texture1Multiply> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Multiply";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Multiply;
            node.m_Value1=new FloatRemap(1,0,10);
            return node;
        }
        Color m_Col ;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Red");//,new GUIContent("Red", "Float"), m_R);
            if (m_TexMode == TexMode.ColorRGB)
            {
                m_Value2.SliderLabel(this,"Green"); //,new GUIContent("Red", "Float"), m_R);
                m_Value3.SliderLabel(this,"Blue"); //,new GUIContent("Red", "Float"), m_R);
            }
            else
            {
                m_Value2.Set(m_Value1);
                m_Value3.Set(m_Value1);
            }
            if (GUI.changed)
            {
                m_Col = new Color(m_Value1, m_Value2, m_Value3);
            }
            else
            {
                m_Col = EditorGUILayout.ColorField(m_Col);
                if (GUI.changed)
                {
                    m_Value1.Set(m_Col.r);
                    m_Value2.Set(m_Col.g);
                    m_Value3.Set(m_Col.b);
                }

            }





        }

    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Splatter")]
    public class Texture1Splatter : TextureMathOp
    {
        public FloatRemap m_BrightnessMin;
        public FloatRemap m_BrightnessMax;
        public FloatRemap m_ScaleMin;
        public FloatRemap m_ScaleMax;
        public const string ID = "Texture1Splatter";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Splatter node = CreateInstance<Texture1Splatter> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Splatter";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Splatter;
            node.m_Value1 = new FloatRemap(1.0f,-10000,10000);
            node.m_Value2 = new FloatRemap(4.0f, 0, 10.0f); //scale out
            node.m_Value3 = new FloatRemap(3.0f,0,100); //count
            node.m_BrightnessMin = new FloatRemap(0,0,1);
            node.m_BrightnessMax = new FloatRemap(1, 0, 1);
            node.m_ScaleMin = new FloatRemap(1, 0, 10);
            node.m_ScaleMax = new FloatRemap(1, 0, 10);
            return node;
        }

        public enum SumMode
        {
            Max = 0,
            Add = 1
        };

        public SumMode m_SumMode;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Seed");//, -10000.0f, 10000.0f) ;//,new GUIContent("Red", "Float"), m_R);
//        m_Value2.SliderLabel(this,"Scale Out");//, 0.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabelInt(this,"Count");//, 0, 100);//,new GUIContent("Red", "Float"), m_R);
            m_BrightnessMin.SliderLabel(this, "Bright Min");//, 0.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
            m_BrightnessMax.SliderLabel(this, "Bright Max");//, 0.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);

            m_ScaleMin.SliderLabel(this, "Scale Min");//, 0.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
            m_ScaleMax.SliderLabel(this, "Scale Max");//, 0.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.m_Value = m_ScaleMin.m_Value;
            m_SumMode = (SumMode)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Sum Type", "blending the overlays"), m_SumMode, GUILayout.MaxWidth(200));


            //        m_OffsetX.SliderLabel(this,"OffsetX", m_OffsetX, 0.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            //        m_OffsetY.SliderLabel(this,"OffsetY", m_OffsetY, 0.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_BrightnessMin, m_BrightnessMax, (int)m_SumMode, m_ScaleMax));
        }
    }
}
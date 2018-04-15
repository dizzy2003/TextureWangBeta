using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/GridSplatterProbability")]
    public class Texture2GridSplatterProbability : Texture2OpBase
    {
        public const string ID = "Texture2GridSplatterProbability";
        public override string GetID { get { return ID; } }

        private const string m_Help = "Plot the src texture to each cell of a fgrid, but randomised (rotatio/brightness offset). use the probability texture to determins if the object should be placed";
        public override string GetHelp() { return m_Help; }
        public FloatRemap m_Value3;
        public FloatRemap m_OffsetX;
        public FloatRemap m_OffsetY;
        public FloatRemap m_OffsetRow;
        public FloatRemap m_Angle;
        public FloatRemap m_ScaleMin;
        public FloatRemap m_ScaleMax;
        public FloatRemap m_Brightness;

        public FloatRemap m_Probability;

        public override Node Create (Vector2 pos) 
        {

            Texture2GridSplatterProbability node = CreateInstance<Texture2GridSplatterProbability> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "GridSplatterProbability";
            node.CreateInputOutputs();
            node.Inputs[0].name = "SrcTex";
            node.Inputs[1].name = "ProbabilityPerCell";
            node.m_Value1 = new FloatRemap(1.0f, 0, 99999);
            node.m_Value2 = new FloatRemap(0.0f, 0, 1);
            node.m_Value3 = new FloatRemap(32.0f, 0, 128);
            node.m_OffsetX = new FloatRemap(0.0f, 0, 1);
            node.m_OffsetY = new FloatRemap(0.0f, 0, 1);
            node.m_OffsetRow = new FloatRemap(0.0f, 0, 1);
            node.m_Probability = new FloatRemap(0.5f, 0, 1);
            node.m_Angle = new FloatRemap(0, 0, 180);
            node.m_ScaleMin = new FloatRemap(1.0f, 0, 5);
            node.m_ScaleMax = new FloatRemap(1.0f, 0, 5);
            node.m_Brightness = new FloatRemap(0.0f, 0, 1);
            node.m_ClampInputUV = true;

            node.m_OpType=TexOp.GridSplatterProb;
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this, "Seed");//, -10000.0f, 10000.0f) ;//,new GUIContent("Red", "Float"), m_R);
            //m_Value2.SliderLabel(this,"Randomize");//, 0.001f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabelInt(this, "Repeat");//, 1, 100);//,new GUIContent("Red", "Float"), m_R);
            m_Probability.SliderLabel(this, "Probability Black");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_OffsetX.SliderLabel(this, "Random OffsetX");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_OffsetY.SliderLabel(this, "Random OffsetY");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_OffsetRow.SliderLabel(this, "Random OffsetRow");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Angle.SliderLabel(this, "RandomAngleScale");
            m_ScaleMin.SliderLabel(this, "RandomZoomScaleMin");
            m_ScaleMax.SliderLabel(this, "RandomZoomScaleMax");
            m_Brightness.SliderLabel(this, "Brightness Randomness");



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply", new Vector4(m_Value1, m_Value2, m_Value3, 1));
            _mat.SetVector("_Multiply2", new Vector4(m_OffsetX, m_OffsetY, m_OffsetRow, m_Probability));
            _mat.SetVector("_Multiply3", new Vector4(m_Angle, m_ScaleMin, m_ScaleMax, m_Brightness));
        }
    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/GridSplatter")]
    public class Texture1GridSplatter : TextureMathOp
    {
        public FloatRemap m_OffsetX;
        public FloatRemap m_OffsetY;
        public FloatRemap m_OffsetRow;
        public FloatRemap m_Angle;
        public FloatRemap m_ScaleMin;
        public FloatRemap m_ScaleMax;
        public FloatRemap m_Brightness;

        public FloatRemap m_Probability;

        public const string ID = "GridSplatter";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1GridSplatter node = CreateInstance<Texture1GridSplatter> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "GridSplatter";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.SplatterGrid;
            node.m_Value1 = new FloatRemap(1.0f,0,99999);
            node.m_Value2 = new FloatRemap(0.0f,0,1);
            node.m_Value3 = new FloatRemap(8.0f,0,128);
            node.m_OffsetX = new FloatRemap(0.5f, 0, 1);
            node.m_OffsetY = new FloatRemap(0.5f, 0, 1);
            node.m_OffsetRow = new FloatRemap(0.0f,0,1);
            node.m_Probability = new FloatRemap(0.5f, 0, 1);
            node.m_Angle = new FloatRemap(180, 0, 180);
            node.m_ScaleMin = new FloatRemap(0.8f, 0, 5);
            node.m_ScaleMax = new FloatRemap(1.0f, 0, 5);
            node.m_Brightness = new FloatRemap(1.0f, 0, 1);
            node.m_ClampInputUV = true;

            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Seed");//, -10000.0f, 10000.0f) ;//,new GUIContent("Red", "Float"), m_R);
            //m_Value2.SliderLabel(this,"Randomize");//, 0.001f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabelInt(this,"Repeat");//, 1, 100);//,new GUIContent("Red", "Float"), m_R);
            m_Probability.SliderLabel(this, "Probability Black");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);

            m_OffsetX.SliderLabel(this,"Random OffsetX");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_OffsetY.SliderLabel(this, "Random OffsetY");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
//            m_OffsetRow.SliderLabel(this, "Random OffsetRow");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Angle.SliderLabel(this, "RandomAngleScale");
            m_ScaleMin.SliderLabel(this, "RandomZoomScaleMin");
            m_ScaleMax.SliderLabel(this, "RandomZoomScaleMax");
            m_Brightness.SliderLabel(this, "Brightness Randomness");



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_OffsetX, m_OffsetY, m_OffsetRow, m_Probability));
            _mat.SetVector("_Multiply3", new Vector4(m_Angle, m_ScaleMin,m_ScaleMax, m_Brightness));
        }
    }
}
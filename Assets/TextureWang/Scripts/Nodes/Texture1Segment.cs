using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/Segment")]
    public class Texture1Segment : CreateOp
    {
        public const string ID = "Segment";
        
        public bool m_SolidFill=true;



        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Segment node = CreateInstance<Texture1Segment> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Segment";
//        node.CreateInputOutputs();
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_ShaderOp = ShaderOp.Segment;
            node.m_TexMode = TexMode.Greyscale;
            node.m_Value1 = new FloatRemap(0.0f,-720,720);
            node.m_Value2 = new FloatRemap(270, -720, 720);
            node.m_Value3 = new FloatRemap(0.0f, -1, 1);
            node.m_Value4 = new FloatRemap(0.0f, -1, 1);
//            node.m_Value5 = new FloatRemap(0.0f, -180, 180);
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"StartAngle");//, 0.0f, 1.0f) ;//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"EndAngle");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"dx");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value4.SliderLabel(this,"dy");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
//            m_Value5.SliderLabel(this, "OffsetAngle0");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);


            m_SolidFill = GUILayout.Toggle(m_SolidFill, "SolidFill");
//        m_Value6 = (FloatRemap)(m_InvertFill ? 1 : 0);



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply", new Vector4((m_Value1-180)*Mathf.Deg2Rad, (m_Value2 - 180) * Mathf.Deg2Rad, m_Value3,m_Value4 ));
            _mat.SetVector("_Multiply2", new Vector4(0 * Mathf.Deg2Rad, m_SolidFill ? 1 : 0,0, 0));
        
        }
    }
}
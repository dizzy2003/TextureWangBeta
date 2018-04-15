using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/Circle")]
    public class Texture1Circle : CreateOp
    {
        public float m_OffsetX;
        public float m_OffsetY;

        public const string ID = "Circle";
        public bool m_Fill = true;
        public bool m_InvertFill;



        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Circle node = CreateInstance<Texture1Circle> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Circle";
//        node.CreateInputOutputs();
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_ShaderOp = ShaderOp.Circle;
            node.m_TexMode = TexMode.Greyscale;
            node.m_Value1 = new FloatRemap(0.5f,0,1);
            node.m_Value2 = new FloatRemap(0.0f, -1, 1);
            node.m_Value3 = new FloatRemap(0.0f, -1, 1);
            node.m_Value4 = new FloatRemap(0.5f, 0, 1);
            node.m_Value5 = new FloatRemap(1.0f, 0, 1);
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"RadiusX");//, 0.0f, 1.0f) ;//,new GUIContent("Red", "Float"), m_R);
            m_Value4.SliderLabel(this,"RadiusY");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"dx");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"dy");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);


            m_Value5.SliderLabel(this, "Fill %");

                                             
            m_InvertFill = GUILayout.Toggle(m_InvertFill, "InvertFill");
//        m_Value6 = (FloatRemap)(m_InvertFill ? 1 : 0);



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_Value5, m_InvertFill ? 1 : 0,m_OffsetX, m_OffsetY));
        
        }
    }
}
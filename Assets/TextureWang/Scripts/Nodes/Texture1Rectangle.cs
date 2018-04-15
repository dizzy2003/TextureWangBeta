using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/Rectangle")]
    public class Texture1Rectangle : CreateOp
    {
        public float m_OffsetX;
        public float m_OffsetY;

        public const string ID = "Rectangle";
    
        public bool m_InvertFill;



        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Rectangle node = CreateInstance<Texture1Rectangle> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Rectangle";
//        node.CreateInputOutputs();
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_ShaderOp = ShaderOp.Square;
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
            m_Value1.SliderLabel(this,"Width");//, 0.0f, 1.0f) ;//,new GUIContent("Red", "Float"), m_R);
            m_Value4.SliderLabel(this,"Height");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"dx");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"dy");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);

            m_Value5.SliderLabel(this, "Fill %");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);


            m_InvertFill = GUILayout.Toggle(m_InvertFill, "InvertFill");



        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_Value5, m_InvertFill ? 1 : 0,m_OffsetX, m_OffsetY));
        
        }
    }
}
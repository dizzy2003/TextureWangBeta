using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/Weave")]
    public class CreateOpWeave : CreateOp
    {


        public const string ID = "CreateOpWeave";
        public override string GetID { get { return ID; } }

        //public Texture m_Cached;



        public override Node Create(Vector2 pos)
        {

            CreateOpWeave node = CreateInstance<CreateOpWeave>();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Weave";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_Value1 = new FloatRemap(10.0f);
            node.m_Value2 = new FloatRemap(10.0f);

            node.m_ShaderOp = ShaderOp.Weave;
            node.m_TexMode = TexMode.Greyscale;


            return node;
        }
        protected internal override void InspectorNodeGUI()
        {

        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            {
                m_Value1.SliderLabel(this,"Columns");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value2.SliderLabel(this,"Rows");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value3.SliderLabel(this,"Thickness");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value4.SliderLabel(this,"Shadows");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            }

        }



    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/Bricks")]
    public class CreateOpGrid : CreateOp
    {


        public const string ID = "CreateOpGrid";
        public override string GetID { get { return ID; } }

        //public Texture m_Cached;



        public override Node Create(Vector2 pos)
        {

            CreateOpGrid node = CreateInstance<CreateOpGrid>();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "CreateOpGrid";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_Value1 = new FloatRemap(10.0f,0,100);
            node.m_Value2 = new FloatRemap(10.0f,0,100);

            node.m_ShaderOp = ShaderOp.Grid;
            node.m_TexMode=TexMode.Greyscale;

            return node;
        }
        protected internal override void InspectorNodeGUI()
        {

        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_Value5, m_Value6, m_Value7 , m_Value8 ));
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();

            {
            
                m_Value1.SliderLabel(this,"Columns");   
                m_Value2.SliderLabel(this,"Rows");      
                m_Value3.SliderLabel(this,"FillSize");  
                m_Value4.SliderLabel(this,"MortarSize");     
                m_Value5.SliderLabel(this,"OddRowOffset");      
                m_Value6.SliderLabel(this,"YawScale");          
                m_Value7.SliderLabel(this,"Color Variation");   
                m_Value8.SliderLabel(this,"Seed");                 
            }




        }

    }
}

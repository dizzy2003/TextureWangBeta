using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/WhiteNoise")]
    public class CreateOpWangNoise : CreateOp
    {


        public const string ID = "CreateOpWangNoise";
        public override string GetID { get { return ID; } }

        //public Texture m_Cached;



        public override Node Create(Vector2 pos)
        {

            CreateOpWangNoise node = CreateInstance<CreateOpWangNoise>();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "WhiteNoise";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_Value1 = new FloatRemap(12345,0,999999.0f);
        
            node.m_ShaderOp = ShaderOp.WangNoise;
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
                m_Value1.SliderLabelInt(this,"Seed");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            }

        }



    }
}
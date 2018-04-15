using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/DiamondPattern")]
    public class CreateOpPattern : CreateOp
    {


        public const string ID = "CreateOpPattern";
        public override string GetID { get { return ID; } }

        //public Texture m_Cached;



        public override Node Create(Vector2 pos)
        {

            CreateOpPattern node = CreateInstance<CreateOpPattern>();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "DiamondPattern";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_Value1 = new FloatRemap(10.0f,0,20);
            node.m_Value2 = new FloatRemap(10.0f,0,20);
            node.m_Value5 = new FloatRemap(0,-Mathf.PI,Mathf.PI);
            node.m_Value6 = new FloatRemap(0, -Mathf.PI, Mathf.PI);
        

            node.m_ShaderOp = ShaderOp.Pattern;
            node.m_TexMode = TexMode.Greyscale;
            return node;
        }
        protected internal override void InspectorNodeGUI()
        {

        }

        private bool m_AbsResult=false;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            {
                m_Value1.SliderLabel(this,"Freq x");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value2.SliderLabel(this,"Freq y");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value5.SliderLabel(this, "OffsetX");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value6.SliderLabel(this, "OffsetY");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_AbsResult = GUILayout.Toggle(m_AbsResult, "Abs Result");
                m_gain.Set( m_AbsResult ? 1.0f : 0.0f);

            }

        }



    }
}

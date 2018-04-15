using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "ThreeInput/EdgeDistDir")]
    public class Texture2EdgeDistDir : Texture2OpBase
    {
        public const string ID = "Texture2EdgeDistDir";
        public override string GetID { get { return ID; } }



        public override Node Create (Vector2 pos) 
        {

            Texture2EdgeDistDir node = CreateInstance<Texture2EdgeDistDir> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "EdgeDistDir";
            node.m_Value = new FloatRemap(10.0f, 0.0f, 500.0f);
            node.CreateInput("TextureSrc", "TextureParam", NodeSide.Left, 30);
            node.CreateInput("DirectionX", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("DirectionY", "TextureParam", NodeSide.Left, 70);
            node.CreateOutput("Result", "TextureParam", NodeSide.Right, 50);
            node.m_Value=new FloatRemap(20.0f,0,100.0f);
            node.m_Value1 = new FloatRemap(0.5f, 0, 1.0f);


            node.m_OpType=TexOp.EdgeDistDir;
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
        
            m_Value.SliderLabel(this,"Dist");//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"FindPixelBrightness");//,new GUIContent("Red", "Float"), m_R);        

            PostDrawNodePropertyEditor();

        }
        public override void SetUniqueVars(Material _mat)
        {
            if (Inputs[1].connection != null)
                _mat.SetTexture("_GradientTex2", Inputs[2].connection.GetValue<TextureParam>().GetHWSourceTexture());
        }
        public override bool Calculate()
        {
            if (!allInputsReady())
            {
                // Debug.LogError(" input no ready");
                return false;
            }
            TextureParam input = null;
            TextureParam input2 = null;
            if (Inputs[0].connection != null)
                input = Inputs[0].connection.GetValue<TextureParam>();
            if (Inputs[1].connection != null)
                input2 = Inputs[1].connection.GetValue<TextureParam>();
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth, m_TexHeight);
            if (input == null || input2 == null)
                return false;

            if (m_Param != null)
            {
 
                Material mat = GetMaterial("TextureOps");
                mat.SetVector("_Multiply", new Vector4(m_Value, m_Value2, m_Value, 0));
                General(input, input2, m_Param, ShaderOp.EdgeDistDir);
                 
                //m_Cached = m_Param.GetHWSourceTexture();
                CreateCachedTextureIcon();
            }

            //m_Cached = m_Param.CreateTexture();
            Outputs[0].SetValue<TextureParam>(m_Param);
            return true;
        }
    }
}
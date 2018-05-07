using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "FourInput/Quadify")]
    public class Texture4Quadify : Texture2OpBase
    {
        public const string ID = "Texture4Quadify";
        public override string GetID { get { return ID; } }



        public override Node Create (Vector2 pos) 
        {

            Texture4Quadify node = CreateInstance<Texture4Quadify> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Quadify";
            node.m_Value =  new FloatRemap(10.0f);
            node.m_Value2 = new FloatRemap(1.0f);
            node.CreateInput("TL", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("TR", "TextureParam", NodeSide.Left, 90);
            node.CreateInput("BL", "TextureParam", NodeSide.Left, 130);
            node.CreateInput("BR", "TextureParam", NodeSide.Left, 130);
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_OpType=TexOp.Quadify;
            return node;
        }

        public bool m_Uniform = true;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
        

            PostDrawNodePropertyEditor();

        }


        public void Blend(TextureParam _input, TextureParam _inputB, TextureParam _inputC, TextureParam _inputD, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", _inputB.IsGrey() ? 1 : 0);
            mat.SetTexture("_GradientTex", _inputB.GetHWSourceTexture());
            mat.SetTexture("_GradientTex2", _inputC.GetHWSourceTexture());
            mat.SetTexture("_GradientTex3", _inputD.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_inputB.m_Width, 1.0f / (float)_inputB.m_Height, m_Value, m_Value2));
            mat.SetVector("_Multiply", new Vector4(m_Value, m_Value1, m_Value2, 0.0f));
            SetCommonVars(mat);
            CalcOutputFormat(_input);
            RenderTexture destination = CreateRenderDestination(_input, _output);

            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat,(int)ShaderOp.Quadify);

        }

        public override bool Calculate()
        {

            TextureParam input = null;
            TextureParam input2 = null;
            TextureParam input3 = null;
            TextureParam input4 = null;
            if (!GetInput(0, out input))
                return false;
            if (!GetInput(1, out input2))
                return false;
            if (!GetInput(2, out input3))
                return false;
            if (!GetInput(3, out input4))
                return false;

            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth, m_TexHeight);
            if (input == null || input2 == null)
                return false;

            if (m_Param != null)
            {
                Blend(input, input2,input3,input4, m_Param);
                CreateCachedTextureIcon();
            }

            //m_Cached = m_Param.CreateTexture();
            Outputs[0].SetValue<TextureParam>(m_Param);
            return true;
        }
    }
}
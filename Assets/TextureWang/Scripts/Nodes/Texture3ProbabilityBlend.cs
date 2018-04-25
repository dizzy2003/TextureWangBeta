using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "ThreeInput/ProbabilityBlend")]
    public class Texture3ProbabilityBlend : Texture2OpBase
    {
        public const string ID = "Texture3ProbabilityBlend";
        public override string GetID { get { return ID; } }



        public override Node Create (Vector2 pos) 
        {

            Texture3ProbabilityBlend node = CreateInstance<Texture3ProbabilityBlend> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "ProbabilityBlend";
            node.m_Value =  new FloatRemap(10.0f);
            node.m_Value2 = new FloatRemap(1234.0f,0,999999);
            node.CreateInput("SrcA", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("SrcB", "TextureParam", NodeSide.Left, 90);
            node.CreateInput("Probability", "TextureParam", NodeSide.Left, 130);
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_OpType=TexOp.Distort;
            return node;
        }

        public bool m_Uniform = true;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value2.SliderLabel(this,"Seed");

            PostDrawNodePropertyEditor();

        }


        public void Blend(TextureParam _input, TextureParam _inputB, TextureParam _inputC, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", _inputB.IsGrey() ? 1 : 0);
            mat.SetTexture("_GradientTex", _inputB.GetHWSourceTexture());
            mat.SetTexture("_GradientTex2", _inputC.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_inputB.m_Width, 1.0f / (float)_inputB.m_Height, m_Value, m_Value2));
            mat.SetVector("_Multiply", new Vector4(m_Value, m_Value1, m_Value2, 0.0f));
            SetCommonVars(mat);
            CalcOutputFormat(_input);
            RenderTexture destination = CreateRenderDestination(_input, _output);

            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat,(int)ShaderOp.ProbabilityBlend);

            //output.TexFromRenderTarget();

            //RenderTexture.ReleaseTemporary(destination);
            //        Debug.LogError(" Distort in Final" + timer.ElapsedMilliseconds + " ms");
        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("m_GeneralInts", new Vector4( 0, 0, 0, (int)m_Value2));
        }
        public override bool Calculate()
        {

            TextureParam input = null;
            TextureParam input2 = null;
            TextureParam input3 = null;
            if (!GetInput(0, out input))
                return false;
            if (!GetInput(1, out input2))
                return false;
            if (!GetInput(2, out input3))
                return false;


            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth, m_TexHeight);
            if (input == null || input2 == null)
                return false;

            if (m_Param != null)
            {
                Blend(input, input2,input3, m_Param);
                CreateCachedTextureIcon();
            }

            //m_Cached = m_Param.CreateTexture();
            Outputs[0].SetValue<TextureParam>(m_Param);
            return true;
        }
    }
}
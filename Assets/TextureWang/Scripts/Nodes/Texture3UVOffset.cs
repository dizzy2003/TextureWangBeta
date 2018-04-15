using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "ThreeInput/UVOffset")]
    public class Texture3UVOffset : Texture2OpBase
    {
        public const string ID = "Texture3UVOffset";
        public override string GetID { get { return ID; } }



        public override Node Create (Vector2 pos) 
        {

            Texture3UVOffset node = CreateInstance<Texture3UVOffset> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "UVOffset";
            node.CreateInput("U", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("V", "TextureParam", NodeSide.Left, 90);
            node.CreateInput("Src", "TextureParam", NodeSide.Left, 130);
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_Value=new FloatRemap(1.0f,0.0f,10.0f);
            node.m_OpType=TexOp.Distort;
            return node;
        }

        public bool m_Uniform = true;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value.SliderLabel(this, "ScaleUVs");

            PostDrawNodePropertyEditor();

        }


        public void UVOffset(TextureParam _inputU, TextureParam _inputV, TextureParam _inputSrc, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _inputSrc.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", _inputU.IsGrey() ? 1 : 0);
            mat.SetTexture("_GradientTex", _inputU.GetHWSourceTexture());
            mat.SetTexture("_GradientTex2", _inputV.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_inputU.m_Width, 1.0f / (float)_inputSrc.m_Height, m_Value, m_Value2));
            mat.SetVector("_Multiply", new Vector4(m_Value, m_Value1, m_Value2, 0.0f));
            SetCommonVars(mat);
            CalcOutputFormat(_inputSrc);
            RenderTexture destination = CreateRenderDestination(_inputSrc, _output);

            Graphics.Blit(_inputSrc.GetHWSourceTexture(), destination, mat,(int)ShaderOp.UVOffset);

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
            if (!allInputsReady())
            {
                // Debug.LogError(" input no ready");
                return false;
            }
            TextureParam input = null;
            TextureParam input2 = null;
            TextureParam input3 = null;
            if (Inputs[0].connection != null)
                input = Inputs[0].connection.GetValue<TextureParam>();
            if (Inputs[1].connection != null)
                input2 = Inputs[1].connection.GetValue<TextureParam>();
            if (Inputs[2].connection != null)
                input3 = Inputs[2].connection.GetValue<TextureParam>();

            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth, m_TexHeight);
            if (input == null || input2 == null)
                return false;

            if (m_Param != null)
            {
                UVOffset(input, input2,input3, m_Param);
                CreateCachedTextureIcon();
            }

            //m_Cached = m_Param.CreateTexture();
            Outputs[0].SetValue<TextureParam>(m_Param);
            return true;
        }
    }
}
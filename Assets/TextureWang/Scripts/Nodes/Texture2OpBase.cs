using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    public abstract class Texture2OpBase : TextureNode
    {
        public TexOp m_OpType = TexOp.Add;
        public FloatRemap m_Value;
        public FloatRemap m_Value1;
        public FloatRemap m_Value2;

        //This really shouldnt exist anymore now every op has its own class
        public enum TexOp { Add, Min, Multiply, Power,Gradient,Blend ,Distort,Max,Sub,DirectionalWarp, SrcBlend,EdgeDistDir,AngleWarp,GridSplatterProb, SmoothDirection,Quadify }

        protected internal override void InspectorNodeGUI()
        {

        }
        
  

        public void Gradient( TextureParam _input, TextureParam _inputB, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", _inputB.IsGrey() ? 1 : 0);
            mat.SetTexture("_GradientTex", _inputB.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_inputB.m_Width, 1.0f / (float)_inputB.m_Height, m_Value, 0));
            SetCommonVars(mat);
            //Texture2D tex = input.CreateTexture(TextureParam.ms_TexFormat);

            //input.FillInTexture(tex);

            //RenderTexture destination = RenderTexture.GetTemporary((int)input.m_Width, (int)input.m_Height, 24, TextureParam.ms_RTexFormat);
            //RenderTexture destination = new RenderTexture((int)input.m_Width, (int)input.m_Height, 24, TextureParam.ms_RTexFormat);
            RenderTexture destination = CreateRenderDestination(_input, _output);
            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)ShaderOp.Gradient);
            //output.TexFromRenderTarget();

            //        RenderTexture.ReleaseTemporary(destination);
            //m_Cached = destination as Texture;
//        Debug.LogError(" Gradient in Final" + timer.ElapsedMilliseconds + " ms");
        }

        public void Distort(TextureParam _input, TextureParam _inputB, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", _inputB.IsGrey() ? 1 : 0);
            mat.SetTexture("_GradientTex", _inputB.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_inputB.m_Width, 1.0f / (float)_inputB.m_Height, m_Value, m_Value2));
            mat.SetVector("_Multiply",new Vector4(m_Value,m_Value1,m_Value2,0.0f));
            SetCommonVars(mat);

            //Texture2D tex = input.CreateTexture(TextureParam.ms_TexFormat);
            //input.FillInTexture(tex);

            //RenderTexture destination = RenderTexture.GetTemporary((int)input.m_Width, (int)input.m_Height, 24, TextureParam.ms_RTexFormat);
            RenderTexture destination = CreateRenderDestination(_input, _output);

            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)ShaderOp.Distort);

            //output.TexFromRenderTarget();

            //RenderTexture.ReleaseTemporary(destination);
//        Debug.LogError(" Distort in Final" + timer.ElapsedMilliseconds + " ms");
        }

        public void General(TextureParam _input, TextureParam _inputB, TextureParam _output, ShaderOp _shaderOp)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Material mat = GetMaterial("TextureOps");
            SetCommonVars(mat);
        
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", _inputB.IsGrey() ? 1 : 0);
            mat.SetTexture("_GradientTex", _inputB.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_input.m_Width, 1.0f / (float)_input.m_Height, m_Value, 0));
            RenderTexture destination = CreateRenderDestination(_input, _output);

            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)_shaderOp);
//        Debug.LogError(""+ _shaderOp+"  in Final" + timer.ElapsedMilliseconds + " ms");
        }
    
        public override bool Calculate()
        {

            TextureParam input = null;
            TextureParam input2 = null;

            if (!GetInput(0, out input))
                return false;
            if (!GetInput(1, out input2))
                return false;

            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
            if (input == null || input2==null)
                return false;

            if ( m_Param != null)
            {
                //TODO get rid of this crap now each enum has its own class
                switch (m_OpType)
                {
                    case TexOp.SmoothDirection:
                        General(input, input2, m_Param, ShaderOp.SmoothedDirection);
                        break;
                    case TexOp.Distort:
                        Distort(input, input2, m_Param);
                        break;
                    case TexOp.Blend:
                    {
                        Material mat = GetMaterial("TextureOps");
                        mat.SetVector("_Multiply", new Vector4(m_Value, m_Value, m_Value, 0));
                        General(input, input2, m_Param, ShaderOp.Blend2);
                    }

                    
                        break;
                    case TexOp.EdgeDistDir:
                    {
                        Material mat = GetMaterial("TextureOps");
                        mat.SetVector("_Multiply", new Vector4(m_Value, m_Value2, m_Value, 0));
                        General(input, input2, m_Param, ShaderOp.EdgeDistDir);
                    }
                        break;
                    case TexOp.Gradient:
                        Gradient(input, input2, m_Param);
                        break;
                    case TexOp.Add:
                        General(input, input2, m_Param,ShaderOp.Add2);
                        break;
                    case TexOp.Min:
                        General(input, input2, m_Param, ShaderOp.Min);
                        //Add(input, input2, m_Param);
                        break;
                    case TexOp.Max:
                        General(input, input2, m_Param, ShaderOp.Max);
                        //Add(input, input2, m_Param);
                        break;
                    case TexOp.Multiply:
                        General(input, input2, m_Param, ShaderOp.Mult2);
                        break;
                    case TexOp.Power:
                        General(input, input2, m_Param, ShaderOp.Pow2);
                        break;
                    case TexOp.Sub:
                        General(input, input2, m_Param, ShaderOp.Sub2);
                        //Add(input, input2, m_Param);
                        break;
                    case TexOp.DirectionalWarp:
                    {
                    
                    
                        Material mat = GetMaterial("TextureOps");
                        mat.SetVector("_Multiply", new Vector4(m_Value, 0, m_Value2, 0));
                        General(input, input2, m_Param, ShaderOp.DirectionWarp);
                    }
                        break;
                    case TexOp.AngleWarp:
                    {
                        Material mat = GetMaterial("TextureOps");
                        mat.SetVector("_Multiply", new Vector4(m_Value*Mathf.Deg2Rad, 0, m_Value2, 0));
                        General(input, input2, m_Param, ShaderOp.AngleWarp);
                    }
                        break;
                    case TexOp.SrcBlend:
                    {
                        Material mat = GetMaterial("TextureOps");
                        mat.SetVector("_Multiply", new Vector4(m_Value, m_Value, m_Value, 0));
                        General(input, input2, m_Param, ShaderOp.SrcBlend);
                    }
                        break;
                    case TexOp.GridSplatterProb:
                        General(input, input2, m_Param, ShaderOp.SplatterGridProb);
                        break;
                    default:
                        Debug.LogError(" un defined texture 2 base op");
                        break;

                }
                //m_Cached = m_Param.GetHWSourceTexture();
                CreateCachedTextureIcon();
            }
        
            //m_Cached = m_Param.CreateTexture();
            Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }

    }
}
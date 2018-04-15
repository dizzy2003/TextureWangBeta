using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    public abstract class Texture2MathOp : TextureNode
    {
        public MathOp m_OpType = MathOp.Add;
        public FloatRemap m_Value;
        public FloatRemap m_Value2;

        public enum MathOp { Add=ShaderOp.Add2, Multiply=ShaderOp.Mult2, Power=ShaderOp.Pow2,Sub=ShaderOp.Sub2, Min = ShaderOp.Min, Max = ShaderOp.Max,Blend =ShaderOp.Blend2, SrcBlend = ShaderOp.SrcBlend,Divide=ShaderOp.Divide,SmoothMasked=ShaderOp.SmoothedMask }

        protected internal override void InspectorNodeGUI()
        {

        }
        protected override Color GetTitleBoxColor()
        {
            return Color.blue+Color.white*0.2f;
        }

        protected Texture2MathOp()
        {
            m_Value = new FloatRemap(0.5f, 0, 1);
        }


        public void General(TextureParam _input, TextureParam _inputB, TextureParam _output, ShaderOp _shaderOp)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Material mat = GetMaterial("TextureOps");
            SetCommonVars(mat);
            mat.SetInt("_MainIsGrey",_input.IsGrey()?1:0);
            mat.SetInt("_TextureBIsGrey", _inputB.IsGrey() ? 1 : 0);
        
            mat.SetVector("_Multiply", new Vector4(m_Value,m_Value,m_Value,m_Value));
            mat.SetTexture("_GradientTex", _inputB.GetHWSourceTexture());
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_inputB.m_Width, 1.0f / (float)_inputB.m_Height, m_Value, 0));
            CalcOutputFormat(_input,_inputB);

            RenderTexture destination = CreateRenderDestination(_input, _output);

            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)_shaderOp);
//        Debug.LogError(""+ _shaderOp+"  in Final" + timer.ElapsedMilliseconds + " ms");
        }

        public override bool Calculate()
        {
            if (!allInputsReady())
            {
                Debug.LogError(" input not ready "+this);
                return false;
            }
            TextureParam input = null;
            TextureParam input2 = null;
            if (Inputs == null||Inputs.Count==0)
            {
                Debug.LogError(" null inputs in "+this);
                return false;
            }
            if (Inputs[0] == null)
            {
                return false;
            }
            if (Inputs[0].connection != null)
                input = Inputs[0].connection.GetValue<TextureParam>();
            if (Inputs[1].connection != null)
                input2 = Inputs[1].connection.GetValue<TextureParam>();
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
            if (input == null || input2==null)
                return false;

            if ( m_Param != null)
            {
//            Debug.Log("execute "+this);
                General(input, input2, m_Param, (ShaderOp)m_OpType);
                //m_Cached = m_Param.GetHWSourceTexture();
                CreateCachedTextureIcon();
            }
        
            Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }

        public override void DrawNodePropertyEditor()
        {


            base.DrawNodePropertyEditor();
            m_OpType = (MathOp)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            //TODO implement local function, get rid of enum 
            if (m_OpType == MathOp.SmoothMasked)
                m_Value.SliderLabel(this, "Smooth Dist:");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            if (m_OpType == MathOp.Blend)
                m_Value.SliderLabel(this,"Blend:");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            if (m_OpType == MathOp.SrcBlend)
                m_Value.SliderLabel(this, "AlphaMult:");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);

            PostDrawNodePropertyEditor();

        }
    }
}
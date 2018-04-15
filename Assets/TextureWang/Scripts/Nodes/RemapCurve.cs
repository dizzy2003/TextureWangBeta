using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/RemapCurve")]
    public class RemapCurve : TextureNode
    {
        public const string ID = "RemapCurve";
        public override string GetID { get { return ID; } }

        public AnimationCurve m_RemapCurve=new AnimationCurve();
        protected Texture2D gradient;// = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        //public Texture m_Cached;


        protected internal override void InspectorNodeGUI()
        {
        
        }

        public override Node Create (Vector2 _pos) 
        {

            RemapCurve node = CreateInstance<RemapCurve> ();
        
            node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "RemapCurve";
            node.CreateInput("Texture", "TextureParam", NodeSide.Left, 50);
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            return node;
        }

        private void OnGUI()
        {
            NodeGUI();
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            //m_TexMode = (TexMode)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Colors", "3 components per texture or one"), m_TexMode, GUILayout.MaxWidth(200));

//        m_Value1 = RTEditorGUI.Slider(m_Value1, -100.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_RemapCurve = EditorGUILayout.CurveField(m_RemapCurve);
        }

    
        public void ExecuteRemapCurve(float _size, TextureParam _input, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);

            if(gradient==null)
                gradient=new Texture2D(1024,1,TextureFormat.RFloat,false); //1024 possible levels

            Color c = Color.white;
            for (float x = 0.0f; x < (float)gradient.width; x += 1.0f)
            {
                c.g=c.b=c.r = m_RemapCurve.Evaluate(x/(float)gradient.width);

                gradient.SetPixel((int)x,0,c);
            }
            gradient.wrapMode = TextureWrapMode.Clamp;
            gradient.Apply();
        


            mat.SetTexture("_GradientTex", gradient);
            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_input.m_Width, 1.0f / (float)_input.m_Height, _size, 0));
            SetCommonVars(mat);
//        m_TexMode =TexMode.ColorRGB;
            RenderTexture destination = CreateRenderDestination(_input, _output);

            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)ShaderOp.Gradient);


//        Debug.LogError(" multiply in Final" + timer.ElapsedMilliseconds + " ms");
        }

  
        public override bool Calculate()
        {
            if (!allInputsReady())
            {
                //Debug.LogError(" input no ready");
                return false;
            }
            TextureParam input = null;
            if (Inputs[0].connection != null)
                input = Inputs[0].connection.GetValue<TextureParam>();
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
            if (input == null)
                Debug.LogError(" input null");
        

            if (input != null && m_Param != null)
            {
                ExecuteRemapCurve(0.0f, input, m_Param);

            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}
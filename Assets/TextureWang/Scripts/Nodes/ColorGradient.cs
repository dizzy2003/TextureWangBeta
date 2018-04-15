using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/ColorGradient")]
    public class ColorGradient : CreateOp
    {
        public const string ID = "ColorGradient";
        public override string GetID { get { return ID; } }

        public Gradient m_Gradient=new Gradient();


        //public Texture m_Cached;


        protected internal override void InspectorNodeGUI()
        {
        
        }

        public override Node Create (Vector2 _pos) 
        {

            ColorGradient node = CreateInstance<ColorGradient> ();
        
            node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "ColorGradient";
        
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_TexMode = TexMode.ColorRGB;

            return node;
        }

        private void OnGUI()
        {
            NodeGUI();
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            SerializedObject serializedGradient = new SerializedObject(this);
            SerializedProperty colorGradient = serializedGradient.FindProperty("m_Gradient");
            EditorGUILayout.PropertyField(colorGradient, true, null);
            if (GUI.changed)
            {
                serializedGradient.ApplyModifiedProperties();
            }

        


        }
        protected Texture2D gradientX;// = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        public void ExecuteRemapCurve(float _size,  TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", m_TexMode == TexMode.Greyscale ? 1 : 0);
            mat.SetInt("_TextureBIsGrey", 1);
            mat.SetVector("_Multiply",new Vector4(1,1,0,0));

            AnimCurveToTexture(ref gradientX, m_Gradient);

//        m_TexMode=TexMode.Greyscale;
            RenderTexture destination = _output.CreateRenderDestination(m_TexWidth, m_TexHeight, TextureParam.GetRTFormat(m_TexMode == TexMode.Greyscale, m_PixelDepth));
            SetCommonVars(mat);
            Graphics.Blit(gradientX, destination, mat, (int)ShaderOp.CopyRGBA);


//        Debug.LogError(" multiply in Final" + timer.ElapsedMilliseconds + " ms");
        }

        private void AnimCurveToTexture(ref Texture2D gradient, Gradient _curve)
        {
            if (gradient == null)
                gradient = new Texture2D(1024, 1, TextureFormat.RGBAFloat, false); //1024 possible levels

            Color c = Color.white;
            for (float x = 0.0f; x < (float) gradient.width; x += 1.0f)
            {
                c = _curve.Evaluate(x/(float) gradient.width);

                gradient.SetPixel((int) x, 0, c);
            }
            gradient.wrapMode = TextureWrapMode.Repeat;
            gradient.Apply();
        }


        public override bool Calculate()
        {
            if (!allInputsReady())
            {
                //Debug.LogError(" input no ready");
                return false;
            }
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
        

            if ( m_Param != null)
            {
                ExecuteRemapCurve(m_Value1,  m_Param);

            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            m_Param.GetHWSourceTexture().wrapMode=TextureWrapMode.Clamp;
            Outputs[0].SetValue<TextureParam> (m_Param);

            return true;
        }
    }
}
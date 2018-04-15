using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TextureOp/CalcNormal")]
    public class CalcNormal : TextureNode
    {
        public const string ID = "CalcNormal";
        public override string GetID { get { return ID; } }

        private const string m_Help = "Turns heightmap into a normal with z pointing out of the screen, adjust Slider to scale up the effects of bumps";
        public override string GetHelp() { return m_Help; }

        //public Texture m_Cached;

        public FloatRemap m_Value1=new FloatRemap(10,0,100);
    

        protected internal override void InspectorNodeGUI()
        {
        
        }

        public override Node Create (Vector2 _pos) 
        {

            CalcNormal node = CreateInstance<CalcNormal> ();
        
            node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "CalcNormal";
            node.CreateInput("Texture", "TextureParam", NodeSide.Left, 50);
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
//        node.m_OpType = TexOP.CalcNormal;
            return node;
        }

        private void OnGUI()
        {
            NodeGUI();
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"strength");//, -100.0f, 100.0f);

        }

        public void CalcNormals(float _size, TextureParam _input, TextureParam _output)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            Material mat = GetMaterial("TextureOps");
            mat.SetInt("_MainIsGrey", _input.IsGrey() ? 1 : 0);


            mat.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)_input.m_Width, 1.0f / (float)_input.m_Height, _size, 0));
            m_TexMode=TexMode.ColorRGB;
            RenderTexture destination = CreateRenderDestination(_input, _output);
            SetCommonVars(mat);
            Graphics.Blit(_input.GetHWSourceTexture(), destination, mat, (int)ShaderOp.Normal);


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
//        float[] values = { m_Value1, m_Value1, m_Value1 };

            if (input != null && m_Param != null)
            {
                CalcNormals(m_Value1, input, m_Param);

            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}
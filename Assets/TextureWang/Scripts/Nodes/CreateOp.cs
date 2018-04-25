using UnityEngine;

//[Node (false, "Standard/Example/CreateOp")]
namespace Assets.TextureWang.Scripts.Nodes
{
    public abstract class CreateOp : TextureNode 
    {
//    public enum TexOP { SetCol,PerlinBm,PerlinTurb,PerlinRidge,VeroniNoise,Pattern,Grid }
//    public TexOP m_OpType = TexOP.SetCol;
        public ShaderOp m_ShaderOp;

/*
    public const string ID = "CreateOp";
	public override string GetID { get { return ID; } }
*/
        //public Texture m_Cached;

        public FloatRemap m_Value1 = new FloatRemap(0.5f);
        public FloatRemap m_Value2 = new FloatRemap(0.5f);
        public FloatRemap m_Value3 = new FloatRemap(0.5f);
        public FloatRemap m_Value4 = new FloatRemap(0.01f);
        public FloatRemap m_Value5 = new FloatRemap(0.5f); //grid is half brick offset
        public FloatRemap m_Value6 = new FloatRemap(0.1f); //rotation
        public FloatRemap m_Value7 = new FloatRemap(0.1f); //random colour variation ammount
        public FloatRemap m_Value8 = new FloatRemap(0.1f); //random colour variation ammount

        //todo GET RID OF ALL THIS FROM cREATEoP AND MOVE IT TO CLAS STHAT NEEDS IT
        public int m_seed = 0;
        public FloatRemap m_frequency = new FloatRemap(1.0f);
        public FloatRemap m_lacunarity = new FloatRemap(2.0f);
        public FloatRemap m_gain = new FloatRemap(0.5f);
        public FloatRemap m_jitter = new FloatRemap(1.0f);
        public FloatRemap m_amp = new FloatRemap(1.0f);
        public int m_Octaves = 1;

        static public ImprovedPerlinNoise m_perlin;


        /*
        public override Node Create (Vector2 pos) 
        {

            CreateOp node = CreateInstance<CreateOp> ();
            
            node.rect = new Rect (pos.x, pos.y, 250, 340);
            node.name = "CreateOp";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            return node;
        }
    */
        protected internal override void InspectorNodeGUI()
        {

        }
        protected override Color GetTitleBoxColor()
        {
            return Color.white;
        }

        protected RenderTexture CreateRenderDestination( TextureParam _output)
        {
            return _output.CreateRenderDestination(m_TexWidth, m_TexHeight,  TextureParam.GetRTFormat(m_TexMode == TexMode.Greyscale, m_PixelDepth),m_Filter);
        }

        public Texture2D m_debugGradient;
        public void General(float r,float g,float b,  TextureParam output, ShaderOp _shaderOp)
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            Material mat = GetMaterial("CreateTexture");
        
            mat.SetVector("_Multiply", new Vector4(r, g, b, m_Value4));
            mat.SetVector("_Multiply2", new Vector4(m_Value5, m_Value6, m_Value7* m_Value5, m_Value8* m_Value6));
            SetCommonVars(mat);
            mat.SetFloat("_Frequency", m_frequency);
            mat.SetFloat("_Lacunarity", m_lacunarity);
            mat.SetFloat("_Gain", m_gain);
            mat.SetFloat("_Jitter", m_jitter);
            mat.SetFloat("_Octaves", m_Octaves);

            if (m_perlin != null)
            {
                mat.SetTexture("_PermTable1D", m_perlin.GetPermutationTable1D());
                mat.SetTexture("_Gradient2D", m_perlin.GetGradient2D());
            }
            m_debugGradient = m_perlin.GetGradient2D();
            RenderTexture destination = CreateRenderDestination( output);
        
            Graphics.Blit(null, destination, GetMaterial("CreateTexture"), (int)_shaderOp);
//        Debug.LogError(""+_shaderOp+" time: " + timer.ElapsedMilliseconds + " ms");
        }

/*
    public override bool Calculate()
    {

        if (m_Param == null)
            m_Param = new TextureParam(m_TexWidth,m_TexHeight);
        

        if ( m_Param != null)
        {
            switch (m_OpType)
            {
                case TexOP.SetCol:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.SetCol);
                    break;
                case TexOP.PerlinBm:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.PerlinBm);
                    break;
                case TexOP.PerlinTurb:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.PerlinTurb);
                    break;
                case TexOP.PerlinRidge:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.PerlinRidge);
                    break;
                case TexOP.VeroniNoise:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.VeroniNoise);
                    break;
                case TexOP.Pattern:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.Pattern);
                    break;
                case TexOP.Grid:
                    General(m_Value1, m_Value2, m_Value3, null, m_Param, ShaderOp.Grid);
                    break;
            }
        }
        m_Cached = m_Param.GetHWSourceTexture();
        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
*/
        public override bool Calculate()
        {

            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);


            if (m_Param != null)
            {

                General(m_Value1, m_Value2, m_Value3,  m_Param, m_ShaderOp);
            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            if(Outputs.Count>0)
                Outputs[0].SetValue<TextureParam>(m_Param);

            CheckDiskIcon(name, m_Param);
            return true;
        }



    }
}

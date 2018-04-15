#if OLD_BLUR
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node (false, "TextureOp/BlurTexOp")]
public class BlurTexOp : TextureOp
{
    public const string ID = "BlurTexOp";
    public override string GetID { get { return ID; } }

    //public Texture m_Cached;


    protected internal override void InspectorNodeGUI()
    {
        base.InspectorNodeGUI();
    }

    public override Node Create (Vector2 _pos) 
    {

        BlurTexOp node = CreateInstance<BlurTexOp> ();
        
        node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "Blur2";
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
        m_Value1 = RTEditorGUI.Slider(m_Value1, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
        m_Value2 = RTEditorGUI.Slider(m_Value2, -30.0f, 30.0f);//,new GUIContent("Red", "Float"), m_R);
        m_Value3 = RTEditorGUI.Slider(m_Value3, -30.0f, 30.0f);//,new GUIContent("Red", "Float"), m_R);

        if(m_Cached!=null)
            GUILayout.Label(m_Cached);
    }

    /// BlurTexOp iterations - larger number means more blur.
    [Range(0, 10)]
    public int m_Iterations = 3;

    /// BlurTexOp spread for each iteration. Lower values
    /// give better looking blur, but require more iterations to
    /// get large blurs. Value is usually between 0.5 and 1.0.
    [Range(0.0f, 1.0f)]
    public float m_BlurSpread = 0.6f;


    // --------------------------------------------------------
    // The blur iteration shader.
    // Basically it just takes 4 texture samples and averages them.
    // By applying it repeatedly and spreading out sample locations
    // we get a Gaussian blur approximation.

    //    public Shader blurShader = null;

    static Material m_BlurMaterial = null;
    protected Material BlurMaterial
    {
        get
        {
            if (m_BlurMaterial == null)
            {
                m_BlurMaterial = new Material(Shader.Find("Hidden/BlurEffectConeTap"));
                m_BlurMaterial.hideFlags = HideFlags.DontSave;
            }
            return m_BlurMaterial;
        }
    }
 
    public void FourTapCone(Texture _source, RenderTexture _dest, int _iteration)
    {
        float off = 0.5f + _iteration * m_BlurSpread;
        Graphics.BlitMultiTap(_source, _dest, BlurMaterial,
                               new Vector2(-off, -off),
                               new Vector2(-off, off),
                               new Vector2(off, off),
                               new Vector2(off, -off)
            );
    }

    // Downsamples the texture to a quarter resolution.
    private void DownSample4X(Texture _source, RenderTexture _dest)
    {
        float off = 1.0f;
        Graphics.BlitMultiTap(_source, _dest, BlurMaterial,
                               new Vector2(-off, -off),
                               new Vector2(-off, off),
                               new Vector2(off, off),
                               new Vector2(off, -off)
            );
    }

    private void DoBlur(Texture _source, RenderTexture _destination)
    {
        int rtW = _source.width / 4;
        int rtH = _source.height / 4;
        RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);


        // Copy source to the 4x4 smaller texture.
        DownSample4X(_source, buffer);

        // BlurTexOp the small texture
        for (int i = 0; i < m_Iterations; i++)
        {
            RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
            FourTapCone(buffer, buffer2, i);
            RenderTexture.ReleaseTemporary(buffer);
            buffer = buffer2;
        }

        Graphics.Blit(buffer, _destination);

        RenderTexture.ReleaseTemporary(buffer);
    }

    public void CalcBlur(TextureParam _input, TextureParam _output)
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        m_Iterations = 1 + (int)m_Value1;
        m_BlurSpread = m_Value2;

        //Texture2D tex = new Texture2D((int)input.m_Width, (int)input.m_Height, TextureFormat.RGBAFloat, false);
        //Texture2D tex = input.CreateTexture(TextureParam.ms_TexFormat);

        //Debug.LogError(" blur in b " + timer.ElapsedMilliseconds + " ms");

        //RenderTexture buffer = RenderTexture.GetTemporary((int)input.m_Width, (int)input.m_Height, 24, TextureParam.ms_RTexFormat);
        CalcOutputFormat(_input);
        RenderTexture destination = CreateRenderDestination(_input, _output);
        DoBlur(_input.GetHWSourceTexture(), destination);

        //output.TexFromRenderTarget();

        //RenderTexture.ReleaseTemporary(buffer);
//        Debug.LogError(" blur in Final" + timer.ElapsedMilliseconds + " ms");
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
            CalcBlur(input, m_Param);

        }
        CreateCachedTextureIcon();
        //m_Cached = m_Param.GetHWSourceTexture();
        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
    }
}
#endif
#if UNUSED
using UnityEngine;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;

[Node (false, "TextureOp/Mixed")]
public class TextureOp : TextureNode
{
    public enum TexOP { Add, ClipMin, Multiply, Power,Blur,CalcNormal,Level1,Transform,Gradient,Min,Max }
    public TexOP m_OpType = TexOP.Add;


    public const string ID = "TextureOp";
	public override string GetID { get { return ID; } }

    //public Texture m_Cached;

    public FloatRemap m_Value1 = new FloatRemap(0.5f);
    public FloatRemap m_Value2 = new FloatRemap(0.5f);
    public FloatRemap m_Value3 = new FloatRemap(0.5f);
    public Gradient m_Gradient = new UnityEngine.Gradient();

    protected internal override void InspectorNodeGUI()
    {
        
    }

    

    public override Node Create (Vector2 _pos) 
	{

        TextureOp node = CreateInstance<TextureOp> ();
        
        node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "Op";
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
        m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
        if (m_OpType == TexOP.Level1)
        {
            m_Value1.SliderLabel(this,"Low");//, -2.0f, 2.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"High");//, -2.0f, 2.0f);//,new GUIContent("Red", "Float"), m_R);

        }
        else
        if (m_OpType == TexOP.Gradient)
        {
            m_Value2.SliderLabel(this, "");//, -2.0f, 2.0f);//,new GUIContent("Red", "Float"), m_R);

        }
        else
        {
            m_Value1.SliderLabel(this, "");//, - 10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this, "");//, -30.0f, 30.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this, "");//, -30.0f, 30.0f);//,new GUIContent("Red", "Float"), m_R);
        }


/*
        GUILayout.Label ("This is a custom Node!");

		GUILayout.BeginHorizontal ();
		GUILayout.BeginVertical ();

		

		GUILayout.EndVertical ();
		GUILayout.BeginVertical ();
		
		Outputs [0].DisplayLayout ();
		
		GUILayout.EndVertical ();
		GUILayout.EndHorizontal ();

    */

    }
    static Material m_TexOpMaterial = null;
    protected Material TexOpMaterial
    {
        get
        {
            if (m_TexOpMaterial == null)
            {
                m_TexOpMaterial = new Material(Shader.Find("Hidden/TextureOps"));
                m_TexOpMaterial.hideFlags = HideFlags.DontSave;
            }
            return m_TexOpMaterial;
        }
    }


    public void General(float _r,float _g,float _b, TextureParam _input, TextureParam _output, ShaderOp _shaderOp)
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        TexOpMaterial.SetVector("_Multiply", new Vector4(_r, _g, _b, 1));
        SetCommonVars(TexOpMaterial);
        RenderTexture destination = CreateRenderDestination(_input, _output);
        Graphics.Blit(_input.GetHWSourceTexture(), destination, TexOpMaterial, (int)_shaderOp);
//        Debug.LogError(" multiply in Final" + timer.ElapsedMilliseconds + " ms");
    }
    public void Add(float _r, float _g, float _b, TextureParam _input, TextureParam _output)
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        TexOpMaterial.SetVector("_Multiply", new Vector4(_r, _g, _b, 1));
        SetCommonVars(TexOpMaterial);
        //Texture2D tex = input.CreateTexture(TextureParam.ms_TexFormat);
        //        input.FillInTexture(tex);

        //RenderTexture destination = RenderTexture.GetTemporary((int)input.m_Width, (int)input.m_Height, 24, TextureParam.ms_RTexFormat);
        RenderTexture destination = CreateRenderDestination(_input, _output);

        Graphics.Blit(_input.GetHWSourceTexture(), destination, TexOpMaterial, 4);


        //output.TexFromRenderTarget();

        //RenderTexture.ReleaseTemporary(destination);
        Debug.LogError(" add in Final" + timer.ElapsedMilliseconds + " ms");
    }

    protected Texture2D gradient;// = new Texture2D(256, 1, TextureFormat.ARGB32, false);
    public void Gradient(TextureParam _input,  TextureParam _output)
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        if(gradient==null)
            gradient=   new Texture2D(256, 1, TextureFormat.ARGB32, false);
        Color[] cols = new Color[256];
        for (int x = 0; x < 256; x++)
        {
            float t = (float)x / 255.0f;
            cols[x]=m_Gradient.Evaluate(t);
        }
        gradient.SetPixels(cols);
        gradient.Apply();

        TexOpMaterial.SetTexture("_GradientTex", gradient);
        TexOpMaterial.SetVector("_TexSizeRecip", new Vector4(1.0f / (float)255.0f, 1.0f , 0.0f, 0));
        RenderTexture destination = CreateRenderDestination(_input, _output);
        Graphics.Blit(_input.GetHWSourceTexture(), destination, TexOpMaterial, (int)ShaderOp.Gradient);

        m_Cached = destination as Texture;
        Debug.LogError(" Gradient in Final" + timer.ElapsedMilliseconds + " ms");
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
        float[] values = { m_Value1, m_Value2, m_Value3 };
        float[] gauss = { 0.0585f,0.0965f,0.0585f,0.0965f,0.1591f,0.0965f,0.0585f,0.0965f,0.0585f };

        if (input != null && m_Param != null)
        {
            switch (m_OpType)
            {
                case TexOP.Add:
                    General(m_Value1, m_Value2, m_Value3, input, m_Param, ShaderOp.AddRGB);
                    break;
                case TexOP.Min:
                    General(m_Value1, m_Value2, m_Value3, input, m_Param, ShaderOp.Min1);
                    break;
                case TexOP.Max:
                    General(m_Value1, m_Value2, m_Value3, input, m_Param, ShaderOp.Max1);
                    break;
                case TexOP.ClipMin:
                    General(m_Value1, m_Value2, m_Value3, input, m_Param, ShaderOp.clipMin);
                    break;
                case TexOP.Multiply:
                    General(m_Value1, m_Value2, m_Value3, input, m_Param,ShaderOp.MultRGB);
                    break;
                case TexOP.Power:
                    General(m_Value1, m_Value2, m_Value3, input, m_Param, ShaderOp.Power);
                    break;
                case TexOP.Level1:
                    float mult = (m_Value2 - m_Value1);
                    float add = m_Value1 ;
                    General(mult, add, m_Value3, input, m_Param, ShaderOp.Level1);
                    break;
                case TexOP.Transform:
                    float cos = Mathf.Cos(m_Value1)*m_Value2;
                    float sin = Mathf.Sin(m_Value1) * m_Value2;
                    General(cos, sin, m_Value3, input, m_Param, ShaderOp.Transform);
                    break;
                case TexOP.Gradient:
                    Gradient(input,  m_Param);
                    break;

            }

        }
        CreateCachedTextureIcon();
        //m_Cached = m_Param.GetHWSourceTexture();
        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
}
#endif
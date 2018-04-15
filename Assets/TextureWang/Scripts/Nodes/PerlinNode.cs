#if UNUSED_OLD_CPU_CODE
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Standard/Example/PerlinNode")]
public class PerlinNode : TextureNode
{
	public const string ID = "PerlinNode";
	public override string GetID { get { return ID; } }

    public float m_ScaleX=11.0f;
    public float m_ScaleY = 11.0f;
    public float m_Noise=1.0f;
    public int m_Octaves=4;

    public float m_OffsetX = 0.0f;
    public float m_OffsetY = 0.0f;


    public TextureParam m_Param;
    //public Texture2D m_Cached;

    public override Node Create (Vector2 pos) 
	{
        
        PerlinNode node = CreateInstance<PerlinNode> ();
        
        node.rect = new Rect (pos.x, pos.y, 150, 340);
		node.name = "Perlin Noise";
		
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        return node;
	}
	
	protected internal override void InspectorNodeGUI() 
	{
        m_OffsetX = RTEditorGUI.Slider(m_OffsetX, -1000.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_OffsetY = RTEditorGUI.Slider(m_OffsetY, -1000.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_ScaleX = RTEditorGUI.Slider(m_ScaleX, 0.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_ScaleY = RTEditorGUI.Slider(m_ScaleY, 0.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);

        m_Noise = RTEditorGUI.Slider(m_Noise, 0.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);

        m_Octaves = RTEditorGUI.IntSlider(m_Octaves, 0, 10);//,new GUIContent("Red", "Float"), m_R);

        if (m_Cached != null)
        {

            GUILayout.Label(m_Cached);
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
	
	public override bool Calculate () 
	{
		if (!allInputsReady ())
			return false;
        if (m_Param == null)
            m_Param = new TextureParam(m_TexWidth,m_TexHeight);
        for (int x = 0; x < m_Param.m_Width; x++)
        {
            for (int z = 0; z < m_Param.m_Height; z++)
            {
                float px = ((float)(x)/ m_Param.m_Width) * m_ScaleX+ m_OffsetX;
                float pz = ((float)(z)/ m_Param.m_Height) * m_ScaleY+ m_OffsetY;
                float py = 0;
                float mag = 1.0f;
                float scaleh = 1.0f;
                for (int detail = 0; detail < m_Octaves; detail++)
                {
                    py += (Mathf.PerlinNoise(px * mag, pz * mag) ) * (float)scaleh * m_Noise;
                    mag *= 2.0f;
                    scaleh *= 0.5f;
                    
                }
                m_Param.Set(x, z, py, py, py, 1.0f);
            }
        }
        
        m_Cached = m_Param.CreateTexture();

        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
}
#endif
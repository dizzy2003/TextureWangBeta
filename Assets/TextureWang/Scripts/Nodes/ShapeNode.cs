#if UNUSED_CPU_CODE
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Standard/Example/ShapeNode")]
public class ShapeNode : TextureNode
{
	public const string ID = "ShapeNode";
	public override string GetID { get { return ID; } }

    public float m_ScaleX=11.0f;
    public float m_ScaleY = 11.0f;

    public float m_OffsetX = 0.0f;
    public float m_OffsetY = 0.0f;


    public TextureParam m_Param;
    //public Texture2D m_Cached;
    private float m_Power=0.5f;

    public override Node Create (Vector2 pos) 
	{

        ShapeNode node = CreateInstance<ShapeNode> ();
        
        node.rect = new Rect (pos.x, pos.y, 150, 340);
		node.name = "ShapeNode";
		
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        return node;
	}
	
	protected internal override void InspectorNodeGUI() 
	{
        m_OffsetX = RTEditorGUI.Slider(m_OffsetX, -1000.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_OffsetY = RTEditorGUI.Slider(m_OffsetY, -1000.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_ScaleX = RTEditorGUI.Slider(m_ScaleX, 0.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_ScaleY = RTEditorGUI.Slider(m_ScaleY, 0.0f, 1000.0f);//,new GUIContent("Red", "Float"), m_R);
        m_Power = RTEditorGUI.Slider(m_Power, 0.0f, 1000.0f);


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
        float mx = m_Param.m_Width * (0.5f + m_OffsetX);
        float my = m_Param.m_Height * (0.5f + m_OffsetY);
        for (int x = 0; x < m_Param.m_Width; x++)
        {
            for (int y = 0; y < m_Param.m_Height; y++)
            {
                float dx = (x - mx)/m_ScaleX;
                float dy = (y - my) / m_ScaleY;
                float dist = 1.0f/Mathf.Pow(dx*dx + dy*dy,m_Power)-1.0f;
                dist = Mathf.Clamp01(dist);
                m_Param.Set(x, y, dist, dist, dist, 1.0f);
            }
        }
        m_Cached = m_Param.CreateTexture();

        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
}
#endif
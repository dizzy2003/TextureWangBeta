#if OLD_CPU_SYSTEM

using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Standard/Example/Splatter")]
public class Splatter : TextureNode
{


    public const string ID = "Splatter";
	public override string GetID { get { return ID; } }

    //public Texture2D m_Cached;

    public int m_Value1 = 5;
    public float m_Value2 = 0.5f;
    public float m_Value3 = 0.1f;


    public TextureParam m_Param;

    public override Node Create (Vector2 pos) 
	{

        Splatter node = CreateInstance<Splatter> ();
        
        node.rect = new Rect (pos.x, pos.y, 250, 340);
		node.name = "Op";
        node.CreateInput("Texture", "TextureParam", NodeSide.Left, 50);
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        return node;
	}
	
	protected internal override void InspectorNodeGUI() 
	{

        m_Value1 = RTEditorGUI.IntSlider(m_Value1, 0, 10);//,new GUIContent("Red", "Float"), m_R);
        m_Value2 = RTEditorGUI.Slider(m_Value2, 0.0f, 500.0f);//,new GUIContent("Red", "Float"), m_R);
        m_Value3 = RTEditorGUI.Slider(m_Value3, 0.0f, 0.5f);//,new GUIContent("Red", "Float"), m_R);


        if(m_Cached!=null)
            GUILayout.Label(m_Cached);


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

        if (input != null && m_Param!=null)
        {
            Random.seed = 0x127365;
            for (int x = 0; x < m_Param.m_Width; x++)
            {
                for (int y = 0; y < m_Param.m_Height; y++)
                {
                    m_Param.SetCol(x, y, Color.black);
                }
            }
            for (int i = 0; i < m_Value1; i++)
            {
                float dx = Random.Range(-m_Value2, m_Value2);
                float dy = Random.Range(-m_Value2, m_Value2);

                
                for (float x = 0; x < m_Param.m_Width; x+=1.0f/m_Value3)
                {
                    for (float y = 0; y < m_Param.m_Height; y += 1.0f / m_Value3)
                    {
                        Color col = input.GetCol((int)(x), (int)y);
                        
                        Color d = m_Param.GetCol((int)(x*m_Value3 + dx), (int)(y * m_Value3 + dy));
                        d += col;
                        m_Param.SetCol((int)(x * m_Value3 + dx), (int)(y*m_Value3 + dy),d);
                    }
                }
            }
        }
        m_Cached = m_Param.CreateTexture();
        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
}
#endif
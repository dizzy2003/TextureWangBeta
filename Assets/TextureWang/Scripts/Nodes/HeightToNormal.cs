#if OLD_CPU_VERSION
using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Standard/Example/HeightToNormal")]
public class HeightToNormal : TextureNode 
{


    public const string ID = "HeightToNormal";
	public override string GetID { get { return ID; } }

    //public Texture2D m_Cached;

    public FloatRemap m_Value1 = (FloatRemap)0.5f;


    public TextureParam m_Param;

    public override Node Create (Vector2 pos) 
	{

        HeightToNormal node = CreateInstance<HeightToNormal> ();
        
        node.rect = new Rect (pos.x, pos.y, 250, 340);
		node.name = "HeightToNormal";
        node.CreateInput("Texture", "TextureParam", NodeSide.Left, 50);
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        return node;
	}
	
	protected internal override void InspectorNodeGUI() 
	{

        m_Value1 = RTEditorGUI.Slider(m_Value1, -100.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);



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
        if (input != null && m_Param!=null)
        {
            float[] heights = new float[m_Param.m_Width * m_Param.m_Height];

            for (int x = 0; x < m_Param.m_Width; x++)
            {
                for (int y = 0; y < m_Param.m_Height; y++)
                {
                    Color col = input.GetCol(x, y);
                    float h = col.r * col.r + col.g * col.g + col.b * col.b;
                    h = Mathf.Sqrt(h);
                    heights[x + y * m_Param.m_Width] = h;
                }
            }
            
            for (int x = 0; x < m_Param.m_Width-1; x++)
            {
                for (int y = 0; y < m_Param.m_Height-1; y++)
                {
                    float h0 = heights[x + y * m_Param.m_Width];
                    float hright= heights[x+1 + y * m_Param.m_Width];
                    float hdown = heights[x  + (y+1) * m_Param.m_Width];

                    Vector3 right = new Vector3(1, (hright - h0)* m_Value1, 0);
                    Vector3 down = new Vector3(0, (hdown - h0) * m_Value1, 1);
                    Vector3 norm = Vector3.Cross(down,right );
                    norm.Normalize();
                    m_Param.SetCol(x,y,new Color(norm.x*0.5f+0.5f, norm.z * 0.5f + 0.5f, norm.y * 0.5f + 0.5f));
                }
            }

        }
        m_Cached = m_Param.CreateTexture();
        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
	}
}
#endif
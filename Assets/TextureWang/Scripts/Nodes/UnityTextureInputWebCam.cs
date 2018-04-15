#if DOESNTWORK
using System.Collections.Generic;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

[Node (false, "Standard/Example/InputWebCam")]
public class UnityTextureInputWebCam : TextureNode
{
    public const string ID = "InputWebCam";
    public override string GetID { get { return ID; } }

    public WebCamTexture m_Input;
//    public WebCamTexture m_InputCam;


    public TextureParam m_Param;
    //public Texture2D m_Cached;

    List<Color> m_GradientCols = new List<Color>();

    public override Node Create (Vector2 pos) 
    {

        UnityTextureInputWebCam node = CreateInstance<UnityTextureInputWebCam> ();
        
        node.rect = new Rect (pos.x, pos.y, 150, 150);
        node.name = "InputWebCam";
		
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
        node.m_Input = new WebCamTexture(256,256);
        node.m_Input.Play();
        return node;
    }
    int m_PrevX = -1;
    int m_PrevY = -1;
    protected internal override void InspectorNodeGUI() 
    {
/*
        if (Event.current.type == EventType.MouseDrag && m_Input != null)
        {
            float px = (Event.current.mousePosition.x/128.0f)*m_Input.width;
            float py = (Event.current.mousePosition.y / 128.0f) * m_Input.height;
            int ipx = (int)px;
            int ipy = (int)py;


            Color col = m_Input.GetPixel(ipx, ipy);
            //m_Input.SetPixel((int)px, (int)py,Color.black);
            if (ipx != px && ipy != py)
            {
                m_GradientCols.Add(col);
                m_PrevX = ipx;
                m_PrevY = ipy;
                //Debug.Log("Current detected event: " + Event.current.mousePosition + " col " + col + " px " + px + " py " + py + " m_Input.width " + m_Input.width + " m_Input.height " + m_Input.height);
            }

            m_Input.Apply();

        }
*/
#if UNITY_EDITOR
       // m_Input =(Texture2D) EditorGUI.ObjectField(new Rect(0, 0, 200, 200), m_Input, typeof(Texture2D),false);
#endif




    }

    public override bool Calculate()
    {
        if (!allInputsReady())
            return false;



        if (m_Input == null)
        {
            m_Input = new WebCamTexture(256, 256);
            m_Input.Play();
            Debug.Log("alloc new web cam");
        }
        
        if (!m_Input.isPlaying)
        {
            if (WebCamTexture.devices.Length > 0)
                m_Input.deviceName = WebCamTexture.devices[0].name;
            m_Input.Play();
            Debug.Log(" web cam not playing ");
            foreach(var x in WebCamTexture.devices)
                Debug.Log(" device "+x.name);
            return false;
        }
        if (m_Param == null)
            m_Param = new TextureParam(m_TexWidth,m_TexHeight);
        try
        {
            for (int x = 0; x < m_Param.m_Width; x++)
            {
                for (int y = 0; y < m_Param.m_Height; y++)
                {
                    Color col = m_Input.GetPixel((int)(((float)x / (float)m_Param.m_Width) * m_Input.width), (int)(((float)y / (float)m_Param.m_Height) * m_Input.height));
                    m_Param.Set(x, y, col.r, col.g, col.b, 1.0f);
                    if(y==0)
                        Debug.Log(" col "+x+" "+col);
                }
            }
        }
        catch (System.Exception _ex)
        {
            Debug.LogError("exception caught: " + _ex);
        }
        Debug.Log(" did something " );
        m_Cached = m_Param.CreateTexture();

        Outputs[0].SetValue<TextureParam> (m_Param);
        return true;
    }
}
#endif
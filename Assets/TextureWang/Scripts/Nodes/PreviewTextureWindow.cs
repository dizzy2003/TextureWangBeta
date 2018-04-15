using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    public class PreviewTextureWindow : EditorWindow
    {

        static List<PreviewTextureWindow> m_List=new List<PreviewTextureWindow>();
//        public RenderTexture m_Preview;
        public TextureNode m_Source;
        public Texture2D m_tex;
        public bool m_Locked;
        public bool m_Histogram=false;



        void OnDestroy()
        {
            if(m_Buffer!=null)
                m_Buffer.Release();

            m_List.Remove(this);
            if (m_Source != null)
                m_Source.RemoveRefreshWindow(this);
        }

        static ComputeShader m_ComputeShader2;
        ComputeBuffer m_Buffer;
        Material m_Material;
        RenderTexture m_HistogramTexture;

        void CreatePreviewHistogram(RenderTexture preview)
        {
            if (m_ComputeShader2 == null)
                m_ComputeShader2 = (ComputeShader)Resources.Load("EyeHistogram");
            var cs = m_ComputeShader2;
            if(m_Buffer==null)
                m_Buffer = new ComputeBuffer(512 * 1, sizeof(uint) << 2);
            int kernel = cs.FindKernel("KHistogramClear");
            cs.SetBuffer(kernel, "_Histogram", m_Buffer);
            cs.Dispatch(kernel, 1, 1, 1);

            kernel = cs.FindKernel("KHistogramGather");
            cs.SetBuffer(kernel, "_Histogram", m_Buffer);
            Texture source = m_Source.m_Cached;
            cs.SetTexture(kernel, "_Source", source);
            cs.SetInt("_IsLinear", GraphicsUtils.isLinearColorSpace ? 1 : 0);
            cs.SetVector("_Res", new Vector4(source.width, source.height, 0f, 0f));
            cs.SetVector("_Channels", new Vector4(1f, 1f, 1f, 0f));
        
            cs.Dispatch(kernel, Mathf.CeilToInt(source.width / 16f), Mathf.CeilToInt(source.height / 16f), 1);

            kernel = cs.FindKernel("KHistogramScale");
            cs.SetBuffer(kernel, "_Histogram", m_Buffer);
            cs.SetVector("_Res", new Vector4(512, 512, 1.0f, 0f));
            cs.Dispatch(kernel, 1, 1, 1);
            Material m = TextureNode.GetMaterial("TextureOps");
        
            m.SetVector("_Multiply",new Vector4(preview.width,preview.height,0,0));
            m.SetBuffer("_Histogram", m_Buffer);
            Graphics.Blit(m_Source.m_Cached, preview, m,  (int)ShaderOp.Histogram);
        }
        public static void Init(TextureNode _src)
        {

            PreviewTextureWindow window = null;
            foreach (var x in m_List)
            {
                if (x!=null && !x.m_Locked)
                {
                    window = x;
                    break;
                }
            }
            if (window == null)
            {
                window = CreateInstance<PreviewTextureWindow>();
                m_List.Add(window);
            }
            else
            {
                if (window.m_Source != null)
                    window.m_Source.RemoveRefreshWindow(window);
            }
            //GetWindow<PreviewTextureWindow>();//ScriptableObject.CreateInstance<PreviewTextureWindow>();
            _src.AddRefreshWindow( window);
            window.m_Source = _src;
            window.m_Window = new Rect(0, 0, 1, 1);


//            window.m_Preview = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
//window.position = new Rect(_inst.canvasWindowRect.x+ _inst.canvasWindowRect.width*0.5f, _inst.canvasWindowRect.y + _inst.canvasWindowRect.height * 0.5f, 350, 250);
            window.titleContent=new GUIContent("Preview");
            window.Show();
            window.Repaint();

        }

        public void AllocTex(int width, int height)
        {
            m_tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        }

        private Rect m_Window;
        public bool m_Drag;
        public static Rect Scale(Rect rect, Vector2 pivot, Vector2 scale)
        {
            rect.position = Vector2.Scale(rect.position - pivot, scale) + pivot;
            rect.size = Vector2.Scale(rect.size, scale);
            return rect;
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.ScrollWheel)
            {
                //mous epos is in window cords 0 y is top, 40 is start of texture
                Vector2 adjustedMouse = Event.current.mousePosition;
                adjustedMouse.y -= 40;
                adjustedMouse.y = position.height - adjustedMouse.y; // texture cords have 0,0 bottom left, mouse co ords are 0,0 top left
                Vector2 tPos = adjustedMouse;
                
                tPos.x *= m_Window.width/ position.width;
                tPos.y *= m_Window.height/ position.height;
                tPos.x += m_Window.x;
                tPos.y += m_Window.y;

                //we now have the cord we are zooming in on in texture space
                float size = m_Window.width*0.5f;//(tPos - m_Window.min).magnitude;
                size *= 1.0f+(Event.current.delta.y*0.03f);
                m_Window.width = size * 2;
                m_Window.height = size * 2;

                //after we zoom in or out we want tpos to be at the same mouse pos
                // new tposx=adjustedMouse*(m_Window.width/ position.width)+m_WindowX
                //m_WindowX=tposx-adjustedMouse*(m_Window.width/ position.width)
                //
                m_Window.x = tPos.x - adjustedMouse.x*(m_Window.width/position.width);
                m_Window.y =( tPos.y- adjustedMouse.y * (m_Window.height / position.height));
            



                Repaint();
//                Debug.Log(" mouse wheel "+ Event.current.delta+" scale "+m_Scale+" mouse pos "+ Event.current.mousePosition+" windowpos "+position+" tpos "+tPos+" "+m_Window);
            }
            if (Event.current.type == EventType.MouseDrag &&Event.current.button==2)
            {
                m_Window.x -= m_Window.width*Event.current.delta.x/position.width; //steps .3 at a time
                m_Window.y += m_Window.height*Event.current.delta.y/position.height; //steps .3 at a time
                
                Repaint();
            }
            GUILayout.BeginHorizontal();
            m_Locked = GUILayout.Toggle(m_Locked, "Locked");
            m_Histogram = GUILayout.Toggle(m_Histogram, "Histogram");

            GUILayout.EndHorizontal();


            if (m_Source == null||m_Source.m_Param == null|| m_Source.m_Cached==null)
                return;
            int wantWidth = m_Source.m_Cached.width;
            int wantHeight = m_Source.m_Cached.height;

            if (m_Histogram)
            {
                wantWidth = 512;
                wantHeight = 512;
            }

            if (m_tex == null || m_tex.width != wantWidth || m_tex.height != wantHeight)
                AllocTex(wantWidth,wantHeight);
            Rect texRect = new Rect(2, 20, position.width - 4, position.height - 24);
            if (Event.current.type == EventType.Repaint)
            {
                RenderTexture preview = RenderTexture.GetTemporary(wantWidth, wantHeight, 0, RenderTextureFormat.ARGB32);

                Material m = TextureNode.GetMaterial("TextureOps");
                m.SetVector("_Multiply", new Vector4(1.0f, 0, 0, 0));
                if (m_Histogram)
                {
                    CreatePreviewHistogram(preview); 
                }
                else
                {
                    Graphics.Blit(m_Source.m_Cached, preview, m, m_Source.m_TexMode == TextureNode.TexMode.Greyscale
                        ? (int) ShaderOp.CopyGrey
                        : (int) ShaderOp.CopyColor);
                }
                m_tex.ReadPixels(new Rect(0, 0, wantWidth, wantHeight), 0, 0);
                m_tex.Apply();
                RenderTexture.active = null;

                //            EditorGUILayout.LabelField("\n Warning: Erases Current Canvas", EditorStyles.wordWrappedLabel);
                //            EditorGUILayout.Separator();
                m_tex.filterMode=FilterMode.Point;
                GUILayout.BeginArea(texRect, GUI.skin.box);
                //GUI.DrawTexture(texRect, m_tex,ScaleMode.StretchToFill);//ScaleMode.StretchToFill);
                GUI.DrawTextureWithTexCoords(texRect, m_tex, m_Window);
                //new Rect(0+m_Scale,0 + m_Scale, 1- m_Scale, 1- m_Scale));//ScaleMode.StretchToFill);
                //GUI.DrawTexture(texRect, m_Preview, ScaleMode.ScaleToFit);//ScaleMode.StretchToFill);
                GUILayout.EndArea();
                RenderTexture.ReleaseTemporary(preview);
            }
            else
            {
                m_tex.filterMode = FilterMode.Point;
                GUILayout.BeginArea(texRect, GUI.skin.box);
                GUI.DrawTextureWithTexCoords(texRect, m_tex, m_Window);
                GUILayout.EndArea();

            }
        }
    }
}

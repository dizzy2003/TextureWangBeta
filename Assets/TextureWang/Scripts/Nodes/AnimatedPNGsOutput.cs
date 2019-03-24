using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NodeEditorFramework;
using NodeEditorFramework.Standard;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.TextureWang.Scripts.Nodes
{
    public class AnimatedPNGPreviewWindow : EditorWindow
    {

        float m_FPS = 15;
        private int m_Frame = 0;
        ScaleMode m_ScaleMode=ScaleMode.StretchToFill;
        public Stopwatch m_SW;
        public bool m_Animate;

        public AnimatedPNGsOutput m_Src;
        public RenderTexture[] m_Frames;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/My Window")]
        public static void Init(AnimatedPNGsOutput _src,RenderTexture[] frames)
        {
            // Get existing open window or if none, make a new one:
            AnimatedPNGPreviewWindow window = (AnimatedPNGPreviewWindow)EditorWindow.GetWindow(typeof(AnimatedPNGPreviewWindow));
            window.m_Src = _src;
            window.m_Frames = frames;
            window.m_SW=Stopwatch.StartNew();
            window.Show();
        }

        void OnGUI()
        {
            float size = Mathf.Min(position.width - 20, position.height - 100);
            GUI.BeginGroup(new Rect(0, 0, size, size));
            if (m_Frames != null && m_Frames.Length > 0)
            {
                Texture t = m_Frames[m_Frame%m_Frames.Length]; //m_Src.GenerateFrame(m_Frame);
                if (t != null)
                    GUI.DrawTexture(new Rect(0, 0, size, size), t, ScaleMode.StretchToFill); //ScaleMode.StretchToFill);
            }
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0, size, 600, 100));
            m_FPS = EditorGUILayout.Slider("FPS", m_FPS, 1, 144);
            m_Frame = (int) EditorGUILayout.Slider("Frame", m_Frame, 0, m_Frames.Length);
            m_Animate = EditorGUILayout.Toggle("Animate", m_Animate);
            m_ScaleMode =
                (ScaleMode) EditorGUILayout.EnumPopup(new GUIContent("ScaleMode", ""), m_ScaleMode, GUILayout.MaxWidth(200));
            if (GUILayout.Button("Recaculate Frames"))
            {
                m_Frames = m_Src.ReCalcFrames();
            }
    

            GUI.EndGroup();
            if (m_Animate && m_SW.ElapsedMilliseconds > 1000/m_FPS)
            {
                m_Frame++;
                if (m_Frame >= m_Frames.Length)
                    m_Frame = 0;
                m_SW.Reset();
                m_SW.Start();
            }
            Repaint();
            /*
                GUILayout.Label("Base Settings", EditorStyles.boldLabel);
                myString = EditorGUILayout.TextField("Text Field", myString);

                groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
                myBool = EditorGUILayout.Toggle("Toggle", myBool);
                myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
                EditorGUILayout.EndToggleGroup();
        */
        }
    }

    [Node (false, "Output/AnimatedPNGsOutput")]
    public class AnimatedPNGsOutput : TextureNode
    {

        private const string m_Help = "Save a series of PNG's that form an animation. The output is a float fed back to the input textures to generate a different texture each loop";
        public override string GetHelp() { return m_Help; }


        public const string ID = "AnimatedPNGsOutput";
        public override string GetID { get { return ID; } }

        public string m_PathName="";

        public float m_StartAnimatedValue=0;
        public float m_EndAnimatedValue=1;
        public int m_LoopCount = 10;
        public bool m_ExportAsSingleSpriteSheet;







        //public Texture2D m_Cached;

        public override Node Create (Vector2 pos) 
        {

            AnimatedPNGsOutput node = CreateInstance<AnimatedPNGsOutput> ();
        
            node.rect = new Rect (pos.x, pos.y, 150, 150);
            node.name = "AnimatedPNGsOutput";
            node.CreateInput("RGB", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("Alpha", "TextureParam", NodeSide.Left, 70);
            //node.CreateInput("AnimatedValue", "Float", NodeSide.Left, 90);
            node.CreateOutput("AnimatedValue", "Float", NodeSide.Right, 50);
            return node;
        }
        public override bool AllowRecursion { get { return true; } }
        protected internal override void InspectorNodeGUI()
        {
        }
        /*
        public Texture GenerateFrame(int _frame)
        {

            _frame = _frame%m_LoopCount;
            InputNode m_AnimatedValue = Inputs[2].connection.body as InputNode;
            if (m_AnimatedValue != null)
            {

                if (m_LoopCount > 0 && m_LoopCount < 500)
                {

                    float step = (m_EndAnimatedValue - m_StartAnimatedValue)/m_LoopCount;
                    //for (float t = m_StartAnimatedValue; t < m_EndAnimatedValue; t += step)
                    float t = m_StartAnimatedValue + step*_frame;
                    {
                        m_AnimatedValue.m_Value.Set(t);
                        NodeEditor.RecalculateFrom(m_AnimatedValue);

                        if (m_Param != null && m_Param.m_Destination != null)
                        {
                            return m_Param.m_Destination;

                        }
                    }
                }
            }
            return null;
        }
    */

        public RenderTexture[] ReCalcFrames()
        {
            RenderTexture[] frames = new RenderTexture[m_LoopCount];

            if (m_LoopCount > 0 && m_LoopCount < 500)
            {
                Material m = GetMaterial("TextureOps");
                foreach (var x in CalculateIE(0))
                {
                    CreateOutputTexture();
                    if (m_Param != null && m_Param.m_Destination != null)
                    {
                        m.SetInt("_MainIsGrey", m_Param.IsGrey() ? 1 : 0);

                        RenderTexture rt = new RenderTexture(m_Param.m_Width, m_Param.m_Height, 0, RenderTextureFormat.ARGB32);
                        frames[x] = rt;
                        Graphics.Blit(m_Param.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyColorAndAlpha);
                    }
                }
            
            }
            return frames;
        }


        public override void DrawNodePropertyEditor() 
        {
            base.DrawNodePropertyEditor();
            m_ExportAsSingleSpriteSheet = RTEditorGUI.Toggle(m_ExportAsSingleSpriteSheet,"Export as Sprite Sheet");
            m_StartAnimatedValue = RTEditorGUI.Slider("Start Animated Value ", m_StartAnimatedValue, -10, 10);
            m_EndAnimatedValue = RTEditorGUI.Slider("End Animated Value ", m_EndAnimatedValue, -10, 10);
            m_LoopCount = (int)RTEditorGUI.Slider("Frame Count ", m_LoopCount, 1, 64);
            if (m_LoopCount < 1)
                m_LoopCount = 1;

            if (GUILayout.Button("Choose OutputPath"))
            {
            
                m_PathName = EditorUtility.SaveFilePanel("SavePNG", "Assets/", m_PathName, "png");

            }
            if (GUILayout.Button("preview"))
            {
                if (m_Param == null)
                {
                    NodeEditor.RecalculateFrom(this);
                }
                
            
                RenderTexture[] frames= new RenderTexture[m_LoopCount];

                if ( m_LoopCount > 0 && m_LoopCount < 500)
                {
                    Material m = GetMaterial("TextureOps");
                    foreach (var x in CalculateIE(0))
                    {
                        CreateOutputTexture();
                        if (m_Param != null && m_Param.m_Destination != null)
                        {
                            m.SetInt("_MainIsGrey", m_Param.IsGrey() ? 1 : 0);

                            RenderTexture rt =new RenderTexture(m_Param.m_Width,m_Param.m_Height,0,RenderTextureFormat.ARGB32);
                            frames[x] = rt;
                            Graphics.Blit(m_Param.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyColorAndAlpha);
                        }
                    }
                    AnimatedPNGPreviewWindow.Init(this, frames);
                }



            
            }

            if (GUILayout.Button("save png's "))
            {
                Texture2D texSpriteSheet = null;
                if (m_ExportAsSingleSpriteSheet && m_Param != null)
                    texSpriteSheet = new Texture2D(m_Param.m_Width * m_LoopCount,m_Param.m_Height,TextureFormat.ARGB32, false);
                

                foreach (var x in CalculateIE(0))
                {
                    CreateOutputTexture();
                    if (m_Param != null && m_Param.m_Destination != null)
                    {
                        string pathrename;
                        if (x < 10)
                            pathrename = m_PathName.Replace(".png", "0" + x + ".png");
                        else
                            pathrename = m_PathName.Replace(".png", "" + x + ".png");

                        if (texSpriteSheet != null)
                        {
                            texSpriteSheet.SetPixels(x * m_Param.m_Width,0, m_Param.m_Width, m_Param.m_Height,m_Param.GetTex2D().GetPixels(0,0, m_Param.m_Width, m_Param.m_Height));

                          
                        }
                        else
                            m_Param.SavePNG(pathrename,m_Param.m_Width,m_Param.m_Height);
                    }
                    else
                    {
                        Debug.LogError(" null m_Param after rebuild all " + m_Param);
                        break;
                    }

                }
                if (texSpriteSheet)
                {
                    texSpriteSheet.Apply();
                    byte[] bytes = texSpriteSheet.EncodeToPNG();

                    if (!string.IsNullOrEmpty(m_PathName))
                    {
                        File.WriteAllBytes(m_PathName, bytes);
                    }
                }
                /*
                        InputNode m_AnimatedValue = Inputs[2].connection.body as InputNode;
                        if (m_AnimatedValue != null)
                        {

                            if (!string.IsNullOrEmpty(m_PathName)&& m_LoopCount > 0 && m_LoopCount < 500)
                            {
                                int count = 0;
                                float step = (m_EndAnimatedValue - m_StartAnimatedValue)/ m_LoopCount;
                                for (float t = m_StartAnimatedValue; t < m_EndAnimatedValue; t += step)
                                {
                                    m_AnimatedValue.m_Value.Set(t);
                                    //NodeEditor.RecalculateFrom(m_AnimatedValue);


                                    if (m_Param != null && m_Param.m_Destination != null)
                                    {
                                        string pathrename;
                                        if(count<10)
                                            pathrename = m_PathName.Replace(".png", "0" + count + ".png");
                                        else
                                            pathrename = m_PathName.Replace(".png", "" + count + ".png");
                                        count++;
                                        m_Param.SavePNG(pathrename);
                                    }
                                    else
                                    {
                                        Debug.LogError(" null m_Param after rebuild all "+m_Param);
                                        break;
                                    }
                                }
                            }
                        }
            */
            }
#if UNITY_EDITOR
            m_PathName = (string)GUILayout.TextField(m_PathName);
#endif


            /*
                GUILayout.BeginArea(new Rect(0, 40, 150, 256));
                if (m_Cached != null)
                {
                    GUILayout.Label(m_Cached);
                }
                GUILayout.EndArea();
        */

        }
        protected internal override void NodeGUI()
        {

            base.NodeGUI();
        }

        /*
    Set all our children to un calculated, when they loop back to us then stop
    */
        public void SetDirty(Node n)
        {

            n.m_DirtyID = Node.ms_GlobalDirtyID;
            foreach (var x in n.Outputs)
            {
                foreach (var c in x.connections)
                {
                    //                Debug.Log("set dirty " + c.body + " from " + n);
                    if (c == null)
                        continue;
                    c.body.calculated = false;
                    if (c.body.m_DirtyID != Node.ms_GlobalDirtyID)
                        SetDirty(c.body);
                }
            }

        }
        private int m_Loops;

        public override void OnClearCalculation()
        {
            //        Debug.LogError(" OnClearCalculation  m_Loops set to 0");

            m_Loops = 0;
        }


        IEnumerable<int> CalculateIE(int startIndex)
        {
            float step = (m_EndAnimatedValue - m_StartAnimatedValue) / (m_LoopCount);
            float t = m_StartAnimatedValue+step* startIndex;
            m_Loops = startIndex;
            while (m_Loops < m_LoopCount)
            {

//                Debug.Log("Set Output 0 to "+t);
                Outputs[0].SetValue<float>(t);
#if DEBUG_LOOPBASIC
            Debug.LogError("Loop Count Inc" + m_Loops + " / " + m_LoopCount + " percentage " +
                           ((float) m_Loops/(float) m_LoopCount));
#endif

                Node.ms_GlobalDirtyID++;
                SetDirty(this);
                var workList = new List<Node>();
                if (Outputs[0] != null) //percentage
                {
                    foreach (var c in Outputs[0].connections)
                    {
                        if (c != null)
                        {
                            if (!workList.Contains(c.body))
                                workList.Add(c.body);
                        }
                    }
                }

                if (workList.Count > 0)
                {
                    calculated = true; //so the descendant can calculate that uses output 0
                    //                    Debug.LogError(NodeEditor.GetPad() + " LoopBasic Executes Worklist");
                    NodeEditor.StartCalculation(workList); //go finish this worklist
                    //                    Debug.LogError(NodeEditor.GetPad() + " LoopBasic Finished Execute Worklist " + workList.Count);
                    calculated = false;
                }

                if (workList.Count != 0)
                {
                    m_Loops--; //we didnt execute
                    break;
                }
            
                CreateOutputTexture();

                yield return m_Loops;
                m_Loops++;
                t += step;



            }
        }

        public override bool Calculate()
        {
            allInputsReady();
/*
        if ( (Inputs[0].connection == null || Inputs[0].connection.IsValueNull))//!allInputsReady())
        {
            //            Debug.LogError(" m_LoopCount set to 0 input 0 is null");
            
            return false;
        }
*/
            foreach (var x in CalculateIE(m_LoopCount-1))
            {
                break; // just create the first one
            }


            CreateCachedTextureIcon();
            return true;
        }

        private bool CreateOutputTexture()
        {
//Get RGB
            TextureParam input = null;
            //Get Alpha
            TextureParam input2 = null;

            if (!GetInput(0, out input))
                return false;
            if (!GetInput(1, out input2))
                return false;

            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth, m_TexHeight);
            m_TexMode = TexMode.ColorRGB;

            Material m = GetMaterial("TextureOps");
            m.SetInt("_MainIsGrey", input.IsGrey() ? 1 : 0);
            m.SetInt("_TextureBIsGrey", input2.IsGrey() ? 1 : 0);
            m.SetTexture("_GradientTex", input2.GetHWSourceTexture());
            Graphics.Blit(input.GetHWSourceTexture(), CreateRenderDestination(input, m_Param), m,
                (int) ShaderOp.CopyColorAndAlpha);
            return true;
        }
    }
}
using System.IO;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Output/UnityTextureOutput")]
    public class UnityTextureOutput : TextureNode
    {
        public const string ID = "UnityTextureOutput";
        public override string GetID { get { return ID; } }

        public Texture2D m_Output;

        public string m_TexName="";

        static public bool ms_ExportPNG = false;



        //public Texture2D m_Cached;

        public override Node Create (Vector2 pos) 
        {

            UnityTextureOutput node = CreateInstance<UnityTextureOutput> ();
        

            node.rect = new Rect (pos.x, pos.y, 150, 150);
            node.name = "UnityTextureOutput";
		
            node.CreateInput("RGB", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("Alpha", "TextureParam", NodeSide.Left, 70);

            return node;
        }

        protected internal override void InspectorNodeGUI()
        {
        }

        private string ms_PathName;
        public override void DrawNodePropertyEditor() 
        {
            if (GUILayout.Button("save png"))
            {
                string name = Path.GetFileName(ms_PathName);
                ms_PathName = EditorUtility.SaveFilePanel("SavePNG", Path.GetDirectoryName(ms_PathName), name, "png");
                Debug.Log(" path "+ ms_PathName);
                
                SavePNG(m_Output, ms_PathName);
            }
            //miked        m_TitleBoxColor = Color.green;
#if UNITY_EDITOR
            
            m_TexName = (string)GUILayout.TextField(m_TexName);
            m_Output = (Texture2D)EditorGUILayout.ObjectField( m_Output, typeof(Texture2D), false, GUILayout.MinHeight(200), GUILayout.MinHeight(200));
            //GUILayout.EndArea();
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


            if (m_Output != null)
                GUILayout.Label(m_Output);

            base.NodeGUI();
        }

        public void SavePNG(Texture2D tex,string path)
        {
            Debug.Log("save png width "+tex.width+" height "+tex.height);
            byte[] bytes = tex.EncodeToPNG();

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        }

        public override bool Calculate()
        {


            if (m_Output == null)
                return false;
            TextureParam input = null;
            TextureParam input2 = null;

            if (!GetInput(0, out input))
                return false;
            if (!GetInput(1, out input2))
                return false;


            if (m_Output.width != input.m_Width)
            {
                Texture2D texture = new Texture2D(input.m_Width, input.m_Height, TextureFormat.ARGB32, false);
                EditorUtility.CopySerialized(texture, m_Output);
                AssetDatabase.SaveAssets();
            }

            //m_Output.width = 256;
            //m_Output.height = 256;
            //int x = 0, y = 0;
            if (m_Output.format != TextureFormat.ARGB32 && m_Output.format != TextureFormat.RGBA32 && m_Output.format != TextureFormat.RGB24)
            {
                Debug.LogError(" Ouput Texture " + m_Output + "wrong Format " + m_Output.format);
            }
            else
            {
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                RenderTexture rt=new RenderTexture(m_Output.width,m_Output.height,0,RenderTextureFormat.ARGB32);

                Material m = GetMaterial("TextureOps");
                m.SetInt("_MainIsGrey", input.IsGrey() ? 1 : 0);
                m.SetInt("_TextureBIsGrey", input2.IsGrey() ? 1 : 0);
                m.SetTexture("_GradientTex", input2.GetHWSourceTexture());
                string path=AssetDatabase.GetAssetPath(m_Output);
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
                if (importer.textureType == TextureImporterType.NormalMap)
                {
                    Graphics.Blit(input.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyNormalMap);

                    RenderTexture.active = rt;
                    m_Output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    //input.DestinationToTexture(m_Output);
                    m_Output.Apply();
                    RenderTexture.active = null;
                    rt.DiscardContents();
                    rt.Release();
                    rt = null;
                    /*
                                //unity appears to have changed their internal format                
                                //so instead save asset as typical normal map png and have unity reimport

                                input.SavePNG(path);
                                importer.compressionQuality = importer.compressionQuality + 1; //try and force the import
                                importer.crunchedCompression = false;
                                importer.SaveAndReimport();
                                AssetDatabase.Refresh();
                */
                }
                else
                {
                    Graphics.Blit(input.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyColorAndAlpha);

                    RenderTexture.active = rt;
                    m_Output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    //input.DestinationToTexture(m_Output);
                    m_Output.Apply();
                    RenderTexture.active = null;
                    rt.DiscardContents();
                    rt.Release();
                    rt = null;

                }


                if (ms_ExportPNG)
                {
                    SavePNG(m_Output, path);
                    importer.compressionQuality = importer.compressionQuality + 1; //try and force the import
                    importer.SaveAndReimport();
                }



                //            Debug.Log("applied output to "+ m_Output+" time: "+timer.ElapsedMilliseconds+" ms res: "+input.m_Width+" minred "+minred+" max red "+maxred + " minalpha " + minalpha + " max alpha " + maxalpha);
            }
       

            //Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}

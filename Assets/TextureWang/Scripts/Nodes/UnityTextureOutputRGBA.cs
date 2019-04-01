using System.IO;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Output/UnityTextureOutputRGBA")]
    public class UnityTextureOutputRGBA : TextureNode
    {
        public const string ID = "UnityTextureOutputRGBA";
        public override string GetID { get { return ID; } }

        public Texture2D m_Output;

        public string m_TexName="";

        static public bool ms_ExportPNG = false;



        //public Texture2D m_Cached;

        public override Node Create (Vector2 pos) 
        {

            UnityTextureOutputRGBA node = CreateInstance<UnityTextureOutputRGBA> ();
        

            node.rect = new Rect (pos.x, pos.y, 150, 150);
            node.name = "UnityTextureOutputRGBA";
		
            node.CreateInput("Red", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("Green", "TextureParam", NodeSide.Left, 60);
            node.CreateInput("Bblue", "TextureParam", NodeSide.Left, 70);
            node.CreateInput("Alpha", "TextureParam", NodeSide.Left, 80);

            return node;
        }

        protected internal override void InspectorNodeGUI()
        {
        }

        public override void DrawNodePropertyEditor() 
        {
//miked        m_TitleBoxColor = Color.green;
#if UNITY_EDITOR
            m_TexName = (string)GUILayout.TextField(m_TexName);
            m_Output = (Texture2D)EditorGUILayout.ObjectField(m_Output, typeof(Texture2D), false, GUILayout.MinHeight(200), GUILayout.MinHeight(200));

#endif



        }
        protected internal override void NodeGUI()
        {


            if (m_Output != null)
                GUILayout.Label(m_Output);

            base.NodeGUI();
        }

        public void SavePNG(Texture2D tex,string path)
        {

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
            TextureParam input3 = null;
            TextureParam input4 = null;

            if (!GetInput(0, out input))
                return false;
            if (!GetInput(1, out input2))
                return false;
            if (!GetInput(2, out input3))
                return false;
            if (!GetInput(3, out input4))
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
                m.SetTexture("_GradientTex2", input3.GetHWSourceTexture());
                m.SetTexture("_GradientTex3", input4.GetHWSourceTexture());
                string path=AssetDatabase.GetAssetPath(m_Output);
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
              
                Graphics.Blit(input.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyRGBAChannels);

                RenderTexture.active = rt;
                m_Output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                //input.DestinationToTexture(m_Output);
                m_Output.Apply();
                RenderTexture.active = null;
                rt.DiscardContents();
                rt.Release();

                if (ms_ExportPNG)
                {
                    if (UnityTextureOutput.ms_ExportPNGAnimated)
                        path = path.Replace(".png", "" + UnityTextureOutput.ms_ExportPNGFrame + ".png");

                    SavePNG(m_Output, path);
                    importer.compressionQuality = importer.compressionQuality + 1; //try and force the import
                    importer.SaveAndReimport();
                }
              
            }
       

            //Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}
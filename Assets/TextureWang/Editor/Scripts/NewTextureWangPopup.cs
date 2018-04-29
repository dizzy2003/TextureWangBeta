using System.IO;
using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TextureWang
{

    public class NewTextureWangPopup : EditorWindow
    {
        int m_Width = 1024;
        int m_Height = 1024;
        private ChannelType m_PixelDepth = ChannelType.Float;
        private NodeEditorTWWindow m_Parent;
        private string m_Path = "Assets/TextureWang/OutputTextures/Default.png";
        public bool m_CreateUnityTex = true;
//        public bool m_CreateUnityMaterial = true;
//        public bool m_CreateTesselatedMaterial = true;
        public bool m_LoadTestCubeScene = true;
        public bool m_CreateMaterial = true;

        public int m_CreateMaterialType;

        public static void Init(NodeEditorTWWindow _inst)
        {

            NewTextureWangPopup window = ScriptableObject.CreateInstance<NewTextureWangPopup>();
            window.m_Parent = _inst;
            window.position = new Rect(_inst.canvasWindowRect.x + _inst.canvasWindowRect.width*0.5f,
                _inst.canvasWindowRect.y + _inst.canvasWindowRect.height*0.5f, 450, 350);
            window.titleContent = new GUIContent("New TextureWang Canvas");
            window.ShowUtility();
        }

        public string MakePNG(string path, string _append)
        {


            string name = path.Replace(".png", _append + ".png");
            var tex = new Texture2D(m_Width, m_Height, TextureParam.ms_TexFormat, false);
            byte[] bytes = tex.EncodeToPNG();

            if (!string.IsNullOrEmpty(name))
            {
                File.WriteAllBytes(name, bytes);
            }
            return name;
        }

        void OnGUI()
        {


            EditorGUILayout.LabelField("\n Warning: Erases Current Canvas", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width");
            m_Width = EditorGUILayout.IntField(m_Width);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height");
            m_Height = EditorGUILayout.IntField(m_Height);
            EditorGUILayout.EndHorizontal();
            m_PixelDepth = (ChannelType)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Pixel Depth", "bytes Per pixel/accuracy"), m_PixelDepth, GUILayout.MaxWidth(300));

            EditorGUILayout.Separator();
            m_CreateUnityTex = EditorGUILayout.Toggle("Create UnityTextureOutput nodes and textures", m_CreateUnityTex);
            m_LoadTestCubeScene = EditorGUILayout.Toggle("Throw Away current Scene and load test cube",
                m_LoadTestCubeScene);
            GUI.enabled = m_CreateUnityTex;
            m_CreateMaterial=EditorGUILayout.BeginToggleGroup("Create new material ", m_CreateMaterial);
            EditorGUI.indentLevel++;
            var text = new string[] { "Create new standard Unity material with new textures", "Create new tesselated dx11 material with new textures" };
            m_CreateMaterialType=GUILayout.SelectionGrid(m_CreateMaterialType, text, 1, EditorStyles.radioButton);
            EditorGUI.indentLevel--;
            //m_CreateUnityMaterial = EditorGUILayout.Toggle("Create new standard Unity material with new textures",m_CreateUnityMaterial);
            //m_CreateTesselatedMaterial = EditorGUILayout.Toggle("Create new tesselated dx11 material with new textures", m_CreateTesselatedMaterial);
            EditorGUILayout.EndToggleGroup();



            m_Path = EditorGUILayout.TextField(m_Path);
            if (GUILayout.Button(new GUIContent("Browse Output Path", "Path to Output Textures to")))
            {
                m_Path = EditorUtility.SaveFilePanelInProject("Save Node Canvas", "", "png", "", m_Path);
            }
            GUI.enabled = true;
            EditorGUILayout.Separator();



            //            m_Height = EditorGUILayout.IntField(m_Height);
            //            m_Noise = EditorGUILayout.FloatField(m_Noise);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                this.Close();
            if (GUILayout.Button("Create"))
            {
                m_Parent.NewNodeCanvas(m_Width, m_Height);
                if (m_CreateUnityTex)
                {
                    if (m_Path.EndsWith("_albedo.png"))
                        m_Path = m_Path.Replace("_albedo.png", ".png");
                    if (m_Path.EndsWith("_normal.png"))
                        m_Path = m_Path.Replace("_albedo.png", ".png");
                    if (m_Path.EndsWith("_MetalAndRoughness.png"))
                        m_Path = m_Path.Replace("_MetalAndRoughness.png", ".png");
                    if (m_Path.EndsWith("_height.png"))
                        m_Path = m_Path.Replace("_height.png", ".png");
                    if (m_Path.EndsWith("_occlusion.png"))
                        m_Path = m_Path.Replace("_occlusion.png", ".png");

                    if (m_LoadTestCubeScene)
                    {
                        if(m_CreateMaterial && m_CreateMaterialType == 1)
                            EditorSceneManager.OpenScene("Assets/TextureWang/Scenes/DoNotEdit/testplaneTesselated.unity");
                        else
                            EditorSceneManager.OpenScene("Assets/TextureWang/Scenes/DoNotEdit/testcube.unity");
                    }
                    //Sigh, unity destroys scriptable objects when you call OpenScene, and you cant use dontdestroyonload
                    NodeEditor.ReInit(true);
                    m_Parent.NewNodeCanvas(m_Width, m_Height,m_PixelDepth);

                    //required to add nodes to canvas
                    NodeEditor.curNodeCanvas = NodeEditorTWWindow.canvasCache.nodeCanvas;

                    float yOffset = 200;
                    var albedo = MakeTextureNodeAndTexture("_albedo", new Vector2(0, 0));
                    var norms = MakeTextureNodeAndTexture("_normal", new Vector2(0, 1*yOffset), true);

                    var height = MakeTextureNodeAndTexture("_height", new Vector2(0, 2*yOffset));
                    var metal = MakeTextureNodeAndTexture("_MetalAndRoughness", new Vector2(0, 3*yOffset),false, "OutputMetalicAndRoughness");
                    Texture2D occ = null;
                    if (m_CreateMaterialType==0 || !m_CreateMaterial)
                        occ = MakeTextureNodeAndTexture("_occlusion", new Vector2(0, 4*yOffset));
                    if (m_CreateMaterial)
                    {
                        if (m_CreateMaterialType == 0)
                        {
                            var m = new Material(Shader.Find("Standard"));
                            m.mainTexture = albedo;
                            m.SetTexture("_BumpMap", norms);
                            m.SetTexture("_ParallaxMap", height);
                            m.SetTexture("_MetallicGlossMap", metal);
                            m.SetTexture("_OcclusionMap", occ);

                            string matPath = m_Path.Replace(".png", "_material.mat");
                            AssetDatabase.CreateAsset(m, matPath);
                            AssetDatabase.ImportAsset(matPath, ImportAssetOptions.ForceSynchronousImport);
                            EditorUtility.SetDirty(m);
                            var mr = FindObjectOfType<MeshRenderer>();
                            if (mr != null)
                                mr.material = m;
                        }
                        else //if (m_CreateTesselatedMaterial)
                        {
                            var m = new Material(Shader.Find("Tessellation/Standard Fixed"));
                            if (m != null)
                            {
                                m.mainTexture = albedo;
                                m.SetTexture("_NormalMap", norms);
                                m.SetTexture("_DispTex", height);
                                m.SetTexture("_MOS", metal);
                                m.SetFloat("_Metallic", 1.0f);
                                m.SetFloat("_Glossiness", 1.0f);
                                m.SetFloat("_Tess", 100.0f);
                                string matPath = m_Path.Replace(".png", "_material.mat");
                                AssetDatabase.CreateAsset(m, matPath);
                                AssetDatabase.ImportAsset(matPath, ImportAssetOptions.ForceSynchronousImport);
                                EditorUtility.SetDirty(m);

                                var mr = FindObjectOfType<MeshRenderer>();
                                if (mr != null)
                                    mr.material = m;
                            }
                        }
                    }
                }

                m_Parent.Repaint();
                this.Close();

            }
            GUILayout.EndHorizontal();
        }

        private Texture2D MakeTextureNodeAndTexture(string texName, Vector2 _pos, bool _isNorm = false,string _nodeTypeName= "UnityTextureOutput")
        {
            string albedo = MakePNG(m_Path, texName);
            AssetDatabase.Refresh();
            TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(albedo);
            if (_isNorm)
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;


            }
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            AssetDatabase.ImportAsset(albedo, ImportAssetOptions.ForceSynchronousImport);
            Texture2D albedoTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(albedo, typeof (Texture2D));



            var n = Node.Create(_nodeTypeName, _pos);
            UnityTextureOutput uto = n as UnityTextureOutput;
            if (uto != null)
            {
                uto.m_Output = albedoTexture;
                uto.m_TexName = albedo;
            }
            UnityTextureOutputMetalicAndRoughness utometal = n as UnityTextureOutputMetalicAndRoughness;
            if (utometal != null)
            {
                utometal.m_Output = albedoTexture;
                utometal.m_TexName = albedo;
            }
            return albedoTexture;
        }
    }
}
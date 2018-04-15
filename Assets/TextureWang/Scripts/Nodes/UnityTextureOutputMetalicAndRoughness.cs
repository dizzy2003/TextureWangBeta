using NodeEditorFramework;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Output/OutputMetalicAndRoughness")]
    public class UnityTextureOutputMetalicAndRoughness : TextureNode
    {
        public const string ID = "OutputMetalicAndRoughness";
        public override string GetID { get { return ID; } }

        public Texture2D m_Output;
        public string m_TexName = "";




        //public Texture2D m_Cached;

        public override Node Create (Vector2 pos) 
        {

            UnityTextureOutputMetalicAndRoughness node = CreateInstance<UnityTextureOutputMetalicAndRoughness> ();
        
            node.rect = new Rect (pos.x, pos.y, 150, 150);
            node.name = "OutputMetalicAndRoughness";
		
            node.CreateInput("Metalic", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("Roughness", "TextureParam", NodeSide.Left, 80);

            return node;
        }
	
        protected internal override void InspectorNodeGUI() 
        {


        }
        public override void DrawNodePropertyEditor()
        {
#if UNITY_EDITOR
            m_Output = (Texture2D)EditorGUI.ObjectField(new Rect(0, 300, 300, 300), m_Output, typeof(Texture2D), false);
            m_TexName = (string)GUILayout.TextField(m_TexName);
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

        public override bool Calculate()
        {
            if (!allInputsReady())
                return false;

            if (m_Output == null)
                return false;
            TextureParam input = null;
            if (Inputs[0].connection != null)
                input = Inputs[0].connection.GetValue<TextureParam>();

            TextureParam input2 = null;
            if (Inputs[1].connection != null)
                input2 = Inputs[1].connection.GetValue<TextureParam>();


            //input.DestinationToTexture(m_Output);


        
            if (m_Output.width != input.m_Width)
            {
                Texture2D texture = new Texture2D(input.m_Width, input.m_Height, TextureFormat.ARGB32, false);
                EditorUtility.CopySerialized(texture, m_Output);
                AssetDatabase.SaveAssets();
            }
            if (m_Output.format != TextureFormat.ARGB32 && m_Output.format != TextureFormat.RGBA32 && m_Output.format != TextureFormat.RGB24)
            {
                Debug.LogError(" Ouput Texture " + m_Output + "wrong Format " + m_Output.format);
            }
            else
            {

                RenderTexture rt = new RenderTexture(m_Output.width, m_Output.height, 0, RenderTextureFormat.ARGB32);

                Material m = GetMaterial("TextureOps");
                m.SetInt("_MainIsGrey", input.IsGrey() ? 1 : 0);
                m.SetInt("_TextureBIsGrey", input2.IsGrey() ? 1 : 0);
                m.SetTexture("_GradientTex", input2.GetHWSourceTexture());
                Graphics.Blit(input.GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyRandA);

                RenderTexture.active = rt;
                m_Output.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                //input.DestinationToTexture(m_Output);
                m_Output.Apply();

            }
       

            //Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}

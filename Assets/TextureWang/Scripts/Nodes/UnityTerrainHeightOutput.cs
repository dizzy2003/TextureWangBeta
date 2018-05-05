using System.IO;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Output/UnityTerrainHeightOutput")]
    public class UnityTerrainHeightOutput : TextureNode
    {
        public const string ID = "UnityTerrainHeightOutput";
        public override string GetID { get { return ID; } }

        public Terrain m_Output;

        public string m_TexName="";

        static public bool ms_ExportPNG = false;



        //public Texture2D m_Cached;

        public override Node Create (Vector2 pos) 
        {

            UnityTerrainHeightOutput node = CreateInstance<UnityTerrainHeightOutput> ();
        

            node.rect = new Rect (pos.x, pos.y, 150, 150);
            node.name = "UnityTerrainHeight";
		
            node.CreateInput("Height", "TextureParam", NodeSide.Left, 50);

            return node;
        }

        protected internal override void InspectorNodeGUI()
        {
        }

        private string ms_PathName;
        public override void DrawNodePropertyEditor() 
        {
            
#if UNITY_EDITOR
            
            m_TexName = (string)GUILayout.TextField(m_TexName);
            m_Output = (Terrain)EditorGUILayout.ObjectField( m_Output, typeof(Terrain), true, GUILayout.MinHeight(200), GUILayout.MinHeight(20));
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




            base.NodeGUI();
        }

        public Texture2D m_Debug;

        public override bool Calculate()
        {


            if (m_Output == null)
                return false;
            TextureParam input = null;

            if (!GetInput(0, out input))
                return false;

            Texture2D tex=input.GetTex2D();
            m_Debug = tex;
            Color[] data=tex.GetPixels(0, 0, tex.width, tex.height);
            float[,] heights=new float[tex.width,tex.height];
            int count = 0;
            for (int x = 0; x < tex.width; x++)
            {
                for (int z = 0; z < tex.height; z++)
                {
                    float val = data[x + z*tex.width].r;
//                    if (val > 0.001f)
//                        count++;
                    heights[x, z] = val;

                }
            }
//            Debug.Log("set heightmap with "+count+" non zero values");
            m_Output.terrainData.SetHeights(1,1,heights);
            //m_Output.Flush();




            //Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}
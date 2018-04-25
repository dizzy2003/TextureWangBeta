using System;
using System.Collections.Generic;
using NodeEditorFramework;
using UnityEngine;
using Random = UnityEngine.Random;


namespace UnityEditor.TreeViewExamples
{

	[Serializable]
	public class MyTreeElement : TreeElement
	{
		//public float floatValue1, floatValue2, floatValue3;
		public Material material;
		public string text = "";
		public bool enabled;
	    public string m_NodeID;
	    public NodeCanvas m_Canvas;

        public static Dictionary<string,Texture2D > ms_LookupTextures ;

        public MyTreeElement (string name, int depth, int id,string _nodeID) : base (name, depth, id)
		{
			//floatValue1 = Random.value;
			//floatValue2 = Random.value;
			//floatValue3 = Random.value;
			enabled = true;
            m_NodeID = _nodeID;
		}

        public override Texture2D GetIcon1()
        {
            if (ms_LookupTextures == null)
            {
                ms_LookupTextures=new Dictionary<string, Texture2D>();
            }
            Texture2D ret;
            if (ms_LookupTextures.TryGetValue(text, out ret))
            {
                return ret;
            }
            else
            {
                string assetName = "Assets/TextureWang/Icons/" + name + ".png";
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetName);
                ms_LookupTextures[name] = tex;
//                if(assetName.Contains("Ce"))
//                    Debug.Log(" Find asset "+ assetName+" found "+tex);
                return tex;

            }
            return null;
        }
    }
}

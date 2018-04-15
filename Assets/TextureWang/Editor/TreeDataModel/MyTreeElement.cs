using System;
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

        public MyTreeElement (string name, int depth, int id,string _nodeID) : base (name, depth, id)
		{
			//floatValue1 = Random.value;
			//floatValue2 = Random.value;
			//floatValue3 = Random.value;
			enabled = true;
            m_NodeID = _nodeID;
		}
	}
}

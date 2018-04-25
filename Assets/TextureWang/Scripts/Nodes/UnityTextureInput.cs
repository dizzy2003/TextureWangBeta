using System.Collections.Generic;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/UnityTextureInput")]
    public class UnityTextureInput : TextureNode
    {
        public const string ID = "UnityTextureInput";
        public override string GetID { get { return ID; } }

        public Texture2D m_Input;
//    public WebCamTexture m_InputCam;


        public TextureParam m_Param;
        //public Texture2D m_Cached;


        public override Node Create (Vector2 pos) 
        {

            UnityTextureInput node = CreateInstance<UnityTextureInput> ();
        
            node.rect = new Rect (pos.x, pos.y, 150, 150);
            node.name = "UnityTextureInput";
		
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            return node;
        }


        protected internal override void InspectorNodeGUI() 
        {

    
#if UNITY_EDITOR
//        m_Input = (Texture2D)EditorGUI.ObjectField(new Rect(0, 90, 250, 250), m_Input, typeof(Texture2D), false);
#endif
            if (m_Cached != null)
            {
                GUI.DrawTexture(new Rect(0, 0, 250, 250), m_Cached, ScaleMode.StretchToFill);
                //GUILayout.Label(m_Cached);
            }




        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            
            m_Input = (Texture2D)EditorGUILayout.ObjectField(m_Input, typeof(Texture2D), false, GUILayout.MinHeight(200), GUILayout.MinHeight(200));

        }
        public override bool Calculate()
        {
            if (!allInputsReady())
                return false;



            if (m_Input == null)
                return false;
            if (m_Param == null )
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);

            m_Param.SetTex(m_Input);
    
            m_Cached = m_Param.GetHWSourceTexture();

            Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}

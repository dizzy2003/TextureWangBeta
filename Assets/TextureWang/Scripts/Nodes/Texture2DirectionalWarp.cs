using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/DirectionalWarp")]
    public class Texture2DirectionalWarp : Texture2OpBase
    {
        public const string ID = "Texture2DirectionalWarp";
        public override string GetID { get { return ID; } }

        private const string m_Help = "Each Pixel in Second Texture is treated as an angle to distort the original pixel in that direction";
        public override string GetHelp() { return m_Help; }


        public override Node Create (Vector2 pos) 
        {

            Texture2DirectionalWarp node = CreateInstance<Texture2DirectionalWarp> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "DirectionalWarp";
            node.CreateInputOutputs();
            node.Inputs[0].name = "SrcTex";
            node.Inputs[1].name = "AnglePerPixel";
            node.m_Value = new FloatRemap(1.0f, 0, 20.0f);
            node.m_Value2 = new FloatRemap(1.0f, 0, 100.0f);

            node.m_OpType=TexOp.DirectionalWarp;
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            //m_OpType = (TexOp)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            //if(m_OpType == TexOP.Blend)
            m_Value.SliderLabel(this,"Angle Scale");//, -50.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
            if (m_OpType == TexOp.DirectionalWarp)
                m_Value2.SliderLabel(this,"Dist Scale");//, -50.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);        

            PostDrawNodePropertyEditor();
/*
        Texture tex = CreateTextureIcon(256);

        GUILayout.Label(tex);
*/

        }
    }
}
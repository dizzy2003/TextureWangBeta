using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/AngleWarp")]
    public class Texture2AngleWarp : Texture2OpBase
    {
        public const string ID = "Texture2AngleWarp";
        public override string GetID { get { return ID; } }

        private const string m_Help = "Offset each pixel from source in a fixed direction given by angle, vary distance based off each pixel in second input";
        public override string GetHelp() { return m_Help; }

        public override Node Create (Vector2 pos) 
        {

            Texture2AngleWarp node = CreateInstance<Texture2AngleWarp> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "AngleWarp";
            node.CreateInputOutputs();
            node.Inputs[0].name = "SrcTex";
            node.Inputs[1].name = "DistancePerPixel";
            node.m_OpType=TexOp.AngleWarp;
            node.m_Value=new FloatRemap(1.0f,0,360.0f);
            node.m_Value2 = new FloatRemap(1.0f, 0, 100.0f);
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            //m_OpType = (TexOp)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            //if(m_OpType == TexOP.Blend)
            m_Value.SliderLabel(this,"Angle To Offset In");//, -50.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"Dist Scale");//, -50.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);        

            PostDrawNodePropertyEditor();
/*
        Texture tex = CreateTextureIcon(256);

        GUILayout.Label(tex);
*/

        }
    }
}
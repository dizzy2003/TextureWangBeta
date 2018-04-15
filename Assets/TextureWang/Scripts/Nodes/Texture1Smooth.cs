using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Smooth")]
    public class Texture1Smooth : TextureMathOp
    {
        private const string m_Help = "Gauss Blur/Smooth Source Dist is number of pixels to use for smooth filter";
        public override string GetHelp() {  return m_Help;  }
        public const string ID = "Texture1Smooth";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Smooth node = CreateInstance<Texture1Smooth> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Smooth";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Smooth;
            node.m_Value1=new FloatRemap(2.0f,0,20);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabelInt(this,"Dist");//,m_Value1, 1, 50);//,new GUIContent("Red", "Float"), m_R);



        }

    }
}
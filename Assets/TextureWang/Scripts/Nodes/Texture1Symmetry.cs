using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Symmetry")]
    public class Texture1Symmetry : TextureMathOp
    {
        public const string ID = "Texture1Symmetry";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Symmetry node = CreateInstance<Texture1Symmetry> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Symmetry";
            node.m_Value1=new FloatRemap(0,-180,180);
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Symetry;
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();

            m_Value1.SliderLabel(this,"Angle");//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);


        }

    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/Distance")]
    public class Texture1Distance : CreateOp
    {

        public const string ID = "Distance";
        public bool m_Fill = true;
        public bool m_InvertFill;



        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Distance node = CreateInstance<Texture1Distance> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Distance";
//        node.CreateInputOutputs();
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_ShaderOp = ShaderOp.Distance;
            node.m_TexMode = TexMode.Greyscale;
            node.m_Value1 = new FloatRemap(0.0f,-1,1);
            node.m_Value2 = new FloatRemap(0.0f, -1, 1);
            node.m_Value3 = new FloatRemap(1.0f, -10, 10);
            return node;
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"PosX");
            m_Value2.SliderLabel(this,"PosY");
            m_Value3.SliderLabel(this, "Scale");

        }
    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/SmoothDirection")]
    public class Texture2DSmoothDirection : Texture2OpBase
    {
        private const string m_Help = "Takes a greyscale heightmap as a direction the normal(slope) is calculated at each pixel then pixels are smoothed in that direction";
        public override string GetHelp() { return m_Help; }

        public const string ID = "Texture2SmoothDirection";
        public override string GetID { get { return ID; } }

        public FloatRemap m_Value3;

        public override Node Create (Vector2 pos) 
        {

            Texture2DSmoothDirection node = CreateInstance<Texture2DSmoothDirection> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "SmoothDirection";
            node.m_Value = new FloatRemap(5.0f, 0.1f, 10);
            node.m_Value1 =  new FloatRemap(10.0f,0,100);
            node.m_Value2 = new FloatRemap(32.0f,0,32);
            node.m_Value3 = new FloatRemap(1.0f, 0, 3);
            node.CreateInput("Src", "TextureParam", NodeSide.Left, 50);
            node.CreateInput("Direction(Slope)", "TextureParam", NodeSide.Left, 70);

            node.m_OpType=TexOp.SmoothDirection;
            return node;
        }
        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply", new Vector4(m_Value1, m_Value3, m_Value2));

        }
        public bool m_Uniform = true;
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this, "Dist");
            m_Value2.SliderLabel(this, "Steps");

            m_Value.SliderLabel(this,  "GradientMult" );
            m_Value3.SliderLabel(this, "normal Average Area");

            PostDrawNodePropertyEditor();

        }
    }
}
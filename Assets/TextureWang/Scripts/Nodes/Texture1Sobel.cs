using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Filter/EdgeDetectSobel")]
    public class Texture1Sobel : TextureMathOp
    {
        public const string ID = "Texture1Sobel";
        public override string GetID { get { return ID; } }

        //private bool m_UseNearestColor=false;

        public override Node Create (Vector2 pos) 
        {

            Texture1Sobel node = CreateInstance<Texture1Sobel> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "EdgeDetectSobel";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Sobel;
            node.m_Value1 = new FloatRemap(1.0f,0.0f,1.0f);
            node.m_Value2 = new FloatRemap(0.5f);

            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Dist");
                                          

//        m_UseNearestColor = GUILayout.Toggle(m_UseNearestColor, "Use Nearest Color");


        }

        public override void SetUniqueVars(Material _mat)
        {
//        _mat.SetVector("m_GeneralInts", new Vector4(m_UseNearestColor ? 1 : 0,0,0,0));
        }

    }
}
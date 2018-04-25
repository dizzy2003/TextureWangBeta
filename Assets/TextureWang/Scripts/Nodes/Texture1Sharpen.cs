using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Filter/SharpenFilter")]
    public class Texture1Sharpen : TextureMathOp
    {
        public const string ID = "Texture1Sharpen";
        public override string GetID { get { return ID; } }

    

        public override Node Create (Vector2 pos) 
        {

            Texture1Sharpen node = CreateInstance<Texture1Sharpen> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "SharpenFilter";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Sharpen;
            node.m_Value1 = new FloatRemap(1.0f,0.0f,1.0f);
            node.m_Value2 = new FloatRemap(0.5f);

            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Dist");
                                          

        


        }

        public override void SetUniqueVars(Material _mat)
        {
//        _mat.SetVector("m_GeneralInts", new Vector4(m_UseNearestColor ? 1 : 0,0,0,0));
        }

    }
}
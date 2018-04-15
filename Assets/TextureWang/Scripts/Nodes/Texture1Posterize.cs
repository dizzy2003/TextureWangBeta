using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Posterize")]
    public class Texture1Posterize : TextureMathOp
    {
        public const string ID = "Texture1Posterize";
        public override string GetID { get { return ID; } }

    

        public override Node Create (Vector2 pos) 
        {

            Texture1Posterize node = CreateInstance<Texture1Posterize> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Posterize";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Posterize;
            node.m_Value1 = new FloatRemap(6.0f,0.0f,256.0f);

            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabel(this,"Shades");
        }

        public override void SetUniqueVars(Material _mat)
        {
//        _mat.SetVector("m_GeneralInts", new Vector4(m_UseNearestColor ? 1 : 0,0,0,0));
        }

    }
}
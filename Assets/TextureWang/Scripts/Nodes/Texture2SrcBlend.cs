using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/SrcBlend")]
    public class Texture2SrcBlend : Texture2MathOp
    {
        private const string m_Help = "Add second texture to first using first textures brightness as a inverse blend value for tex2 (tex2 is used when tex1 is black), multiplied by user setting";
        public override string GetHelp() { return m_Help; }

        public const string ID = "Texture2SrcBlend";
        public override string GetID { get { return ID; } }

        public FloatRemap m_Value1;

        public override Node Create (Vector2 pos) 
        {

            Texture2SrcBlend node = CreateInstance<Texture2SrcBlend> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "2SrcBlend";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.SrcBlend;
            node.m_Value = new FloatRemap(1f, 0, 10);
            return node;
        }


    }
}
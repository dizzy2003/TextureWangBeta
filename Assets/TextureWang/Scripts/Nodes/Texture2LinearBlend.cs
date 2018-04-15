using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/LinearBlend")]
    public class Texture2LinearBlend : Texture2MathOp
    {
        public const string ID = "Texture2LinearBlend";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2LinearBlend node = CreateInstance<Texture2LinearBlend> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "2LinearBlend";
            node.m_Value=new FloatRemap(.5f,0,1);
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Blend;
            return node;
        }


    }
}
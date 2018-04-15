using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Divide2")]
    public class Texture2Divide : Texture2MathOp
    {
        public const string ID = "Texture2Div";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Divide node = CreateInstance<Texture2Divide> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Divide";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Divide;
            return node;
        }


    }
}
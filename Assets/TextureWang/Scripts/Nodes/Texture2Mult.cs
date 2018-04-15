using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Multiply2")]
    public class Texture2Mult : Texture2MathOp
    {
        public const string ID = "Texture2Mult";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Mult node = CreateInstance<Texture2Mult> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Mult";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Multiply;
            return node;
        }


    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Power2")]
    public class Texture2Pow : Texture2MathOp
    {
        public const string ID = "Texture2Power";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Pow node = CreateInstance<Texture2Pow> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Power";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Multiply;
            return node;
        }


    }
}
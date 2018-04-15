using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Sub2")]
    public class Texture2Sub : Texture2MathOp
    {
        public const string ID = "Texture2Sub";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Sub node = CreateInstance<Texture2Sub> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Sub";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Sub;
            return node;
        }


    }
}
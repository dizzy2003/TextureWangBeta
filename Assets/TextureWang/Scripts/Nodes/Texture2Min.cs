using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Min2")]
    public class Texture2Min : Texture2MathOp
    {
        public const string ID = "Texture2Min";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Min node = CreateInstance<Texture2Min> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "2Min";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Min;
            return node;
        }


    }
}
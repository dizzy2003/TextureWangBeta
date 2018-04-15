using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Max2")]
    public class Texture2Max : Texture2MathOp
    {
        public const string ID = "Texture2Max";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Max node = CreateInstance<Texture2Max> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "2Max";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Max;
            //node.m_Value = new FloatRemap(0.5f, 0, 1);
            return node;
        }


    }
}
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/Add2")]
    public class Texture2Add : Texture2MathOp
    {
        public const string ID = "Texture2Add";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2Add node = CreateInstance<Texture2Add> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Add";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Add;
            //node.m_Value=new FloatRemap(0.5f,0,1);
            return node;
        }


    }
}

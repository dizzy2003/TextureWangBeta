using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "TwoInput/SmoothMasked")]
    public class Texture2SmoothMasked : Texture2MathOp
    {
        public const string ID = "Texture2SmoothMasked";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture2SmoothMasked node = CreateInstance<Texture2SmoothMasked> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "SmoothMasked";
            node.m_Value=new FloatRemap(.5f,0,1);
            node.CreateInputOutputs();
            node.m_OpType=MathOp.SmoothMasked;
            return node;
        }


    }
}
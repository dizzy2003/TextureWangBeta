using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/BlackEdge")]
    public class Texture1BlackEdge : TextureMathOp
    {
        public const string ID = "Texture1BlackEdge";
        public override string GetID { get { return ID; } }

        private bool m_UseNearestColor=false;

        public override Node Create (Vector2 pos) 
        {

            Texture1BlackEdge node = CreateInstance<Texture1BlackEdge> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "BlackEdge";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.BlackEdge;
            node.m_Value1 =new FloatRemap(10.0f);
            node.m_Value2 = new FloatRemap(0.5f);

            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabelInt(this,"Dist");//, 0, 100);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"Min Threshold");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            //        m_UseNearestColor = GUILayout.Toggle(m_UseNearestColor, "Use Nearest Color");

//        if (m_Cached != null)
//            GUILayout.Label(m_Cached);

        }
/*
    public override void SetUniqueVars(Material _mat)
    {
        _mat.SetVector("m_GeneralInts", new Vector4(m_UseNearestColor ? 1 : 0,0,0,0));
    }
*/
    }
}
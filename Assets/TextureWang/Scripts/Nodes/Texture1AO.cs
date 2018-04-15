using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/AmbientOcclusion")]
    public class Texture1AO : TextureMathOp
    {
        private const string m_Help = "depth based AmbientOcclusion";
        public override string GetHelp() {  return m_Help;  }
        public const string ID = "Texture1AmbientOcclusion";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1AO node = CreateInstance<Texture1AO> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "AmbientOcclusion";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.AO;
            node.m_Value1=new FloatRemap(2.0f,0,20);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Value1.SliderLabelInt(this,"Radius");//,m_Value1, 1, 50);//,new GUIContent("Red", "Float"), m_R);



        }

    }
}
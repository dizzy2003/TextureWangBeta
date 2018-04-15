using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/MapCylinder")]
    public class Texture1MapCylinder : TextureMathOp
    {
        public const string ID = "T1MapCylinder";
        public override string GetID { get { return ID; } }



        public override Node Create (Vector2 pos) 
        {

            Texture1MapCylinder node = CreateInstance<Texture1MapCylinder> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "MapCylinder";
            node.CreateInputOutputs();

            node.m_OpType=MathOp.MapCylinder;
            node.m_Value1=new FloatRemap(1.0f,0.0f,2.0f);
            node.m_Value2 = new FloatRemap(0.0f, 0.0f, 2.0f);
            node.m_Value3 = new FloatRemap(1.0f, 0.0f, 2.0f);
            return node;
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();

            m_Value1.SliderLabel(this,"Curvuture");//,  -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this, "Offset");
            m_Value3.SliderLabel(this, "WrapRate");


        }

        public override bool Calculate()
        {
            Debug.Log(" map cyclinder opffset "+((float)m_Value2));
            return base.Calculate();
        }
    }
}
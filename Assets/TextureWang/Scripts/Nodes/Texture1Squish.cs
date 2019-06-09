using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Squish")]
    public class Texture1Squish : TextureMathOp
    {
        private const string m_Help = "Rotate, scale and transform input texture";
        public override string GetHelp() { return m_Help; }
        public FloatRemap m_OffsetX;
        public FloatRemap m_OffsetY;
        public FloatRemap m_TopX;
        public FloatRemap m_BottomX;
        public FloatRemap m_LeftY;
        public FloatRemap m_RightY;
        //        public FloatRemap m_Angle;



        public const string ID = "Texture1Squish";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Squish node = CreateInstance<Texture1Squish> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Squish";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Squish;
//            node.m_Angle = new FloatRemap(0.0f,0,360);
            node.m_TopX = new FloatRemap(0.0f,0,2);
            node.m_BottomX = new FloatRemap(1.0f,0,2);
            node.m_LeftY = new FloatRemap(1.0f, 0, 2);
            node.m_RightY = new FloatRemap(1.0f, 0, 2);

            node.m_OffsetX = new FloatRemap(0.0f,-1, 1);
            node.m_OffsetY = new FloatRemap(0.0f, -1, 1);

            return node;
        }
    
        public override void DrawNodePropertyEditor()
        {

            base.DrawNodePropertyEditor();
//            m_Angle.SliderLabel(this,"Angle");//,m_Value1, -180.0f, 180.0f) ;//,new GUIContent("Red", "Float"), m_R);



            EditorGUI.indentLevel++;
            m_TopX.SliderLabel(this,  "BottomWidth");//, m_Value2);//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
                                                 //        m_NonUniform = EditorGUILayout.BeginToggle("NonUniform Scale:", m_NonUniform);

            m_BottomX.SliderLabel(this, "TopWidth");//, -10, 10.0f);
            m_LeftY.SliderLabel(this, "LeftHeight");//, -10, 10.0f);
            m_RightY.SliderLabel(this, "RightHeight");//, -10, 10.0f);


            GUI.enabled = true;
            EditorGUI.indentLevel--;
            //        EditorGUILayout.EndToggleGroup();


            m_OffsetX.SliderLabel(this,"OffsetX");//, 0.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            m_OffsetY.SliderLabel(this,"OffsetY");//, 0.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);



        }

        public override bool Calculate()
        {

            TextureParam input = null;

            if (!GetInput(0, out input))
                return false;
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth, m_TexHeight);
            if (input == null)
                Debug.LogError(" input null");
        

            if (input != null && m_Param != null)
            {
//                float cos = Mathf.Cos(m_Angle * Mathf.Deg2Rad);// * m_Value2;
//                float sin = Mathf.Sin(m_Angle * Mathf.Deg2Rad);// * m_ScaleY;
                General(m_LeftY, m_RightY, 0.0f, input, m_Param, (ShaderOp)m_OpType);

            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            Outputs[0].SetValue<TextureParam>(m_Param);
            return true;
        }

        public override void SetUniqueVars(Material _mat)
        {
            _mat.SetVector("_Multiply2", new Vector4(m_OffsetX, m_OffsetY, m_TopX, m_BottomX));

        }
    }
}
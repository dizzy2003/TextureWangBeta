using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "OneInput/Transform")]
    public class Texture1Transform : TextureMathOp
    {
        private const string m_Help = "Rotate, scale and transform input texture";
        public override string GetHelp() { return m_Help; }
        public FloatRemap m_OffsetX;
        public FloatRemap m_OffsetY;
        public FloatRemap m_ScaleX;
        public FloatRemap m_ScaleY;
        public FloatRemap m_Angle;

        public bool m_Uniform = true;

        public const string ID = "Texture1Transform";
        public override string GetID { get { return ID; } }

        public override Node Create (Vector2 pos) 
        {

            Texture1Transform node = CreateInstance<Texture1Transform> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Transform";
            node.CreateInputOutputs();
            node.m_OpType=MathOp.Transform;
            node.m_Angle = new FloatRemap(0.0f,0,360);
            node.m_ScaleX = new FloatRemap(1.0f,-10,10);
            node.m_ScaleY = new FloatRemap(1.0f,-10,10);

            node.m_OffsetX = new FloatRemap(0.0f, -10, 10);
            node.m_OffsetY = new FloatRemap(0.0f, -10, 10);

            return node;
        }
    
        public override void DrawNodePropertyEditor()
        {

            base.DrawNodePropertyEditor();
            m_Angle.SliderLabel(this,"Angle");//,m_Value1, -180.0f, 180.0f) ;//,new GUIContent("Red", "Float"), m_R);

            m_Uniform = GUILayout.Toggle(m_Uniform, "Uniform Scale:");

            EditorGUI.indentLevel++;
            m_ScaleX.SliderLabel(this, m_Uniform ? "Scale": "ScaleX");//, m_Value2);//, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
            //        m_NonUniform = EditorGUILayout.BeginToggle("NonUniform Scale:", m_NonUniform);
            GUI.enabled = !m_Uniform;
            m_ScaleY.SliderLabel(this, "ScaleY");//, -10, 10.0f);

            GUI.enabled = true;
            EditorGUI.indentLevel--;
            //        EditorGUILayout.EndToggleGroup();
            if (m_Uniform)
            {
                m_ScaleY.Set( m_ScaleX);
            }


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
                float cos = Mathf.Cos(m_Angle * Mathf.Deg2Rad);// * m_Value2;
                float sin = Mathf.Sin(m_Angle * Mathf.Deg2Rad);// * m_ScaleY;
                General(cos, sin, 0.0f, input, m_Param, (ShaderOp)m_OpType);

            }
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            Outputs[0].SetValue<TextureParam>(m_Param);
            return true;
        }

        public override void SetUniqueVars(Material _mat)
        {
            if (m_Uniform)
            {

                _mat.SetVector("_Multiply2", new Vector4(m_OffsetX, m_OffsetY, m_ScaleX, m_ScaleX));
            }
            else
            {
                _mat.SetVector("_Multiply2", new Vector4(m_OffsetX, m_OffsetY, m_ScaleX, m_ScaleY));
            }

        }
    }
}
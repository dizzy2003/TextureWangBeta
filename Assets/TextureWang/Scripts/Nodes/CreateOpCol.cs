using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/Colour")]
    public class CreateOpCol : CreateOp
    {


        public const string ID = "CreateOpCol";
        public override string GetID { get { return ID; } }

        public bool m_UseHSV;
        public bool m_UseUnityColPicker;
        public Color m_UnityCol;

        //public Texture m_Cached;



        public override Node Create(Vector2 pos)
        {

            CreateOpCol node = CreateInstance<CreateOpCol>();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "CreateOpCol";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            node.m_Value1 = new FloatRemap(0.5f,0,1);
            node.m_Value2 = new FloatRemap(0.5f,0,1);
            node.m_Value3 = new FloatRemap(0.5f, 0, 1);


            node.m_ShaderOp = ShaderOp.SetCol;
            node.m_TexMode = TexMode.ColorRGB;
            return node;
        }
        protected internal override void InspectorNodeGUI()
        {

        }
        public override void SetUniqueVars(Material _mat)
        {
            if (m_UseHSV)
            {
                Color temp = Color.HSVToRGB(m_Value1, m_Value2, m_Value3, false);
                _mat.SetVector("_Multiply", new Vector4(temp.r, temp.g, temp.b, m_Value4));
            }
        }
        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            if (m_TexMode == TexMode.ColorRGB)
            {
                m_UseHSV = GUILayout.Toggle(m_UseHSV, "Use HSV");
                if (!m_UseHSV)
                {
                    m_Value1.SliderLabel(this, "Red");
                    m_Value2.SliderLabel(this, "Green");
                    m_Value3.SliderLabel(this, "Blue");
                }
                else
                {
                    m_Value1.SliderLabel(this, "Hue");
                    m_Value2.SliderLabel(this, "Saturation");
                    m_Value3.SliderLabel(this, "Value");
                }
                m_UseUnityColPicker = GUILayout.Toggle(m_UseUnityColPicker, "Use UnityColPicker");
                m_UnityCol = EditorGUILayout.ColorField(m_UnityCol);
                if (m_UseUnityColPicker)
                {
                
                    m_Value1.Set(m_UnityCol.r);
                    m_Value2.Set( m_UnityCol.g);
                    m_Value3.Set(m_UnityCol.b);
                }
            }
            else
            {
                m_Value1.SliderLabel(this, "Brightness");
            }

        }



    }
}

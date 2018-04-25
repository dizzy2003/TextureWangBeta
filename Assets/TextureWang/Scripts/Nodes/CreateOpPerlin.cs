using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "Source/Perlin")]
    public class CreateOpPerlin : CreateOp
    {
        private const string m_Help = "Perlin Noise, Integer scales produce wrapping texture output";
        public override string GetHelp() { return m_Help; }

        public enum TexOP {  PerlinBm, PerlinTurb, PerlinRidge, VeroniNoise }
        public TexOP m_OpType = TexOP.PerlinBm;


        public const string ID = "CreateOpPerlin";
        public override string GetID { get { return ID; } }

        //public Texture m_Cached;





        public override Node Create (Vector2 pos) 
        {

            CreateOpPerlin node = CreateInstance<CreateOpPerlin> ();
        
            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Perlin";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
            node.m_Value1 = new FloatRemap(20.0f,0,100);
            node.m_Value2 = new FloatRemap(20.0f,0,100);
            node.m_Value5 = new FloatRemap(5.0f,0,100);
            node.m_Value6 = new FloatRemap(5.0f,0,100);
            node.m_lacunarity = new FloatRemap(2.0f,1,4);
            node.m_gain = new FloatRemap(0.5f,0,1);
            node.m_TexMode = TexMode.Greyscale;
            node.m_Octaves = 4;

            if (CreateOpPerlin.m_perlin == null)
            {
                CreateOpPerlin.m_perlin = new ImprovedPerlinNoise(m_seed);

                CreateOpPerlin.m_perlin.LoadResourcesFor2DNoise();
            }

            return node;
        }
        protected internal override void InspectorNodeGUI()
        {

        }

        public bool m_Uniform=true;
        public bool m_Wrapping = false;
        public  override void DrawNodePropertyEditor()
        {
        
            base.DrawNodePropertyEditor();
        
            m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            m_Wrapping = GUILayout.Toggle(m_Wrapping, "Wrapping:");
            m_Uniform = GUILayout.Toggle(m_Uniform,"Uniform:");
            {
                //m_Value1 .SliderLabel(this, "Period", m_Value1, 0.0f, 8.0f);
                if (m_Uniform)
                {
                    m_Value5.SliderLabel(this,"Scale");//, m_Value5, 0.0f, 100.0f);
                    m_Value6 = m_Value5;
                    //,new GUIContent("Red", "Float"), m_R);

                }
                else
                {
                    m_Value5.SliderLabel(this,"ScaleX");//, m_Value5, 0.0f, 100.0f);
                    //,new GUIContent("Red", "Float"), m_R);
                    m_Value6.SliderLabel(this, "ScaleY");//, m_Value6, 0.0f, 100.0f);
                    //,new GUIContent("Red", "Float"), m_R);
                }
                if (m_Wrapping)
                {
                    m_Value6.Floor();//.m_Value = Mathf.Floor(m_Value6.m_Value);
                    m_Value5.Floor();//.m_Value = Mathf.Floor(m_Value5.m_Value);
                }
                m_Value7.SliderLabel(this,"OffsetX");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
                m_Value8.SliderLabel(this,"OffsetY");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);

                //            m_frequency.SliderLabel(this,"Frequency", m_frequency, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
                m_lacunarity.SliderLabel(this,"FreqScalePerOctave");//, 0.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
                m_gain.SliderLabel(this,"AmpGainPerOctave");//, 0.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);
                if (m_OpType == TexOP.VeroniNoise)
                {
                    m_jitter.SliderLabel(this,"Jitter");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
                    //m_amp = RTEditorGUI.Slider(m_amp, -10.0f, 10.0f);//,new GUIContent("Red", "Float"), m_R);

                }
                m_Octaves = RTEditorGUI.IntSlider(new GUIContent("Octaves"),m_Octaves, 1, 10);//,new GUIContent("Red", "Float"), m_R);
            }




        }
 
  

        public override bool Calculate()
        {
            if (!allInputsReady())
            {
                //Debug.LogError(" input no ready");
                return false;
            }
            if (Outputs == null || Outputs.Count == 0)
            {
                Debug.LogError(" null Outputs in " + this);
                return false;
            }
            if (m_Wrapping)
            {
                m_Value6.Floor();// = (FloatRemap)Mathf.Floor(m_Value6);
                m_Value5.Floor();// = (FloatRemap)Mathf.Floor(m_Value5);
            }
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
            if (m_perlin == null||m_perlin.GetPermutationTable1D()==null || m_perlin.GetGradient2D() == null)
            {
                m_perlin = new ImprovedPerlinNoise(m_seed);
                m_perlin.LoadResourcesFor2DNoise();
            }


            if ( m_Param != null)
            {
                switch (m_OpType)
                {
                    case TexOP.PerlinBm:
                        General(m_Value1, m_Value2, m_Value3,  m_Param, ShaderOp.PerlinBm);
                        break;
                    case TexOP.PerlinTurb:
                        General(m_Value1, m_Value2, m_Value3,  m_Param, ShaderOp.PerlinTurb);
                        break;
                    case TexOP.PerlinRidge:
                        General(m_Value1, m_Value2, m_Value3,  m_Param, ShaderOp.PerlinRidge);
                        break;
                    case TexOP.VeroniNoise:
                        General(m_Value1, m_Value2, m_Value3,  m_Param, ShaderOp.VeroniNoise);
                        break;
                }
            }
            //m_Cached = m_Param.GetHWSourceTexture();
            CreateCachedTextureIcon();
            Outputs[0].SetValue<TextureParam> (m_Param);
            CheckDiskIcon(name, m_Param);
            return true;
        }
    }
}

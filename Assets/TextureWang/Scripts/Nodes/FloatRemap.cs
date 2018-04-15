using System;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Serializable]
    public struct FloatRemap 
    {
        public bool m_ReplaceWithInput;
        public NodeKnob m_Replacement;

        public float m_Value;

        public float m_Min,m_Max;
        public float m_Add;
        public float m_Mult;

        public bool m_ReplaceWithInputNext;
        public bool m_OpenMinMax;


        public FloatRemap(float _val,float _min=0.0f,float _max=1.0f)
        {
            m_Value = _val;
            m_Replacement = null;
            m_ReplaceWithInput = false;
            m_ReplaceWithInputNext = m_ReplaceWithInput;
            m_OpenMinMax = false;
            m_Min = _min;
            m_Max = _max;
            m_Mult = 1;
            m_Add = 0;
        }
        public static implicit operator float (FloatRemap _fr)
        {
            if (_fr.m_Replacement != null)
            {
                NodeInput ni=_fr.m_Replacement as NodeInput;
                if (ni != null && ni.connection != null)
                {
                    _fr.m_Value = ni.connection.GetValue<float>();
                    return _fr.m_Value*_fr.m_Mult + _fr.m_Add;
                }
            }
            return _fr.m_Value;
        }

        public void Floor()
        {
            m_Value = Mathf.Floor(m_Value);
        }
        /*
        public static explicit operator FloatRemap(float b) 
        {
            FloatRemap d = new FloatRemap(b);  
            return d;
        }
    */

        public float SliderLabelInt(Node _node, string _label)
        {
            m_Value = (int) m_Value;
            m_Min = (int) m_Min;
            m_Max = (int)m_Max;
            return SliderLabel(_node, _label);

        }
        //Proper way of using this class
        public  float SliderLabel(Node _node, string _label)
        {
        
            if (!m_ReplaceWithInput)
            {
                if (m_Min == 0.0f && m_Max == 0.0f)
                {
                    m_Min = -10f;
                    m_Max = 10.0f;
                }
                m_Min = Mathf.Min(m_Value, m_Min);
                m_Max = Mathf.Max(m_Value, m_Max);
            }
            //using (new EditorGUI.DisabledScope(m_ReplaceWithInput != false))


            //        if (!m_ReplaceWithInput)
            {
        
                EditorGUILayout.LabelField(_label);
            
                GUILayout.BeginHorizontal();
            
                m_ReplaceWithInput = GUILayout.Toggle(m_ReplaceWithInput,m_ReplaceWithInput ? ">>":"<<");
//            if(GUI.enabled)
//                GUI.enabled = !m_ReplaceWithInput;
                m_OpenMinMax = EditorGUILayout.Foldout(m_OpenMinMax, "");
                m_Value =  UnityEditor.EditorGUILayout.Slider(m_Value, m_Min, m_Max);//, sliderOptions);
                
                GUI.enabled = true;
                GUILayout.EndHorizontal();
          
                if (m_OpenMinMax)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Slider: Min,Max", GUILayout.Width(100));
                    m_Min = EditorGUILayout.FloatField(m_Min);
                    m_Max = EditorGUILayout.FloatField(m_Max);
                    GUILayout.EndHorizontal();
                    if (GUI.enabled)
                        GUI.enabled = m_ReplaceWithInput;

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Input Mult/Add", GUILayout.Width(100));
                    m_Mult = EditorGUILayout.FloatField(m_Mult);
                    m_Add = EditorGUILayout.FloatField(m_Add);
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }


                //            m_Value = RTEditorGUI.SliderLabel(_label, m_Value, _min, _max); //,new GUIContent("Red", "Float"), m_R);
            }

            if (m_ReplaceWithInput && m_Replacement == null)
            {
                m_Replacement = _node.FindExistingInputByName(_label);
                if(m_Replacement == null)
                    m_Replacement = _node.CreateInput(_label, "Float");
//miked            (m_Replacement as NodeInput).Optional = true;
            }
            if (!m_ReplaceWithInput && m_Replacement != null)
            {
                _node.RemoveInput((m_Replacement as NodeInput));
                m_Replacement = null;
            }
            //        GUI.enabled = m_ReplaceWithInput;
 
            if (m_ReplaceWithInput && m_Replacement!=null)
            {
                GUILayout.BeginHorizontal();
                m_Replacement.name = (string) GUILayout.TextField(m_Replacement.name);
                GUILayout.EndHorizontal();
            }

            if (m_ReplaceWithInput)
            {

                if ((m_Replacement as NodeInput).connection != null)
                    m_Value = (m_Replacement as NodeInput).connection.GetValue<float>();
            }
//        GUI.enabled = true;
            return m_Value;

        }

        public void Set(float _val)
        {
            if (!m_ReplaceWithInput)
                m_Value = _val;
        }




    }
}


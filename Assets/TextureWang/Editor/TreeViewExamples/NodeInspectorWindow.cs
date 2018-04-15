#if POO
using TextureWang;
using UnityEditor;
using UnityEngine;


public class NodeInspectorWindow : EditorWindow
{
    public NodeEditorTWWindow m_Source;
    private Vector2 m_ScrollPos;
    public float rnd;
    void OnDestroy()
    {

    }

    void Awake()
    {
        rnd = Random.value;
        Debug.Log("awake "+rnd);
    }
 
    public static NodeInspectorWindow Init(NodeEditorTWWindow _src)
    {

        NodeInspectorWindow window = GetWindow<NodeInspectorWindow>();

        window.m_Source = _src;
        if (_src == null)
            Debug.LogError("init Node Inspector Window with null source");
        window.titleContent=new GUIContent("TextureWang");
        window.Show();
        window.Repaint();
        
        return window;

    }

    void OnEnable()
    {
//        Debug.Log("InspectorWindow enabled");
    }

    void OnGUI()
    {
//        GUILayout.BeginArea(new Rect(0, 0, 256, 600));
        GUILayout.BeginVertical();

        m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos, false, true);//, GUILayout.Width(256), GUILayout.MinHeight(200), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));
        GUI.changed = false;
        if(m_Source!=null)  
            m_Source.DrawSideWindow();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        if (GUI.changed)
        {
            GUI.changed = false;
            if (m_Source != null)
                m_Source.Repaint();
        }
    }
}
#endif
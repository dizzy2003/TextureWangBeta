using System;
using System.IO;
using System.Security.Permissions;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Version = System.Version;

namespace TextureWang
{

    public class StartTextureWangPopup : EditorWindow
    {
        const int m_Width = 600;
        const int m_Height = 600;
        private NodeEditorTWWindow m_Parent;
        private WWW www;
        private WWW wwwImage = null;

        

        private DownloadHandlerFile m_Down;
        private UnityWebRequest uwr;

        private bool m_DidFocus = false;

        public static void Init(NodeEditorTWWindow _inst)
        {

            StartTextureWangPopup window = ScriptableObject.CreateInstance<StartTextureWangPopup>();
            ms_StartPopup = window;


                window.m_Parent = _inst;
            window.position = new Rect(_inst.canvasWindowRect.x + _inst.canvasWindowRect.width*0.5f,
                _inst.canvasWindowRect.y + _inst.canvasWindowRect.height*0.5f, m_Width, m_Height);
            window.titleContent = new GUIContent("Welcome To TextureWang");
            window.www = new WWW("http://54.237.244.93/");

            window.ShowUtility();

            

        }

        public static StartTextureWangPopup ms_StartPopup;
        private void OnDestroy()
        {
            ms_StartPopup = null;
        }

        private int m_Count;
        private Texture2D m_Background;
        void OnGUI()
        {
            
            if (!m_DidFocus)
            {
             
                Focus();
                m_DidFocus = true;
            }
            if (www == null)
                return;
            //            string str ="\n Welcome to TextureWang \n \n If you find it useful please consider becoming a patreon \nto help support future features \n ";
            string str = "";
            string pathTitle = "Assets/TextureWang/TWtitle2.jpg";
            pathTitle = pathTitle.Replace("/", "" + Path.DirectorySeparatorChar);

            if (wwwImage!=null && wwwImage.isDone)
            {

                File.WriteAllBytes(pathTitle, wwwImage.bytes);
                AssetDatabase.ImportAsset(pathTitle, ImportAssetOptions.ForceSynchronousImport);
                wwwImage = null;

            }

            if (m_Background == null)
            {
                m_Background = (Texture2D) AssetDatabase.LoadAssetAtPath(pathTitle, typeof (Texture2D));
                if (m_Background != null)
                {
                    DateTime createdTime = File.GetCreationTime(pathTitle);
                    DateTime now = DateTime.Now;
                    TimeSpan age = now - createdTime;
                    Debug.Log(" Title age days:"+age.Days+" "+age.Seconds);
                    if (age.Days > 1||age.Seconds>10)
                    {
                        StartLoadImage();
                    }
                }
                else
                {
                    StartLoadImage();
                }
            }

//            if (m_Down != null )
//                Debug.Log(" download hangler " + m_Down.isDone+ " wwwImage "+ wwwImage.downloadProgress+" err "+wwwImage.error);

            Rect buttonArea = new Rect(0, 500, 600, 100);
            

            if (www.isDone )
            {
                try
                {


                    Version v = new Version(www.text);
                    
                    str += "\n\nLatest version available " + v + " your version: " + NodeEditorTWWindow.m_Version;
                    if(m_Background!=null)
                        GUI.DrawTexture(new Rect((m_Width-512)*0.5f, 0, 512, 512), m_Background, ScaleMode.ScaleAndCrop); //ScaleMode.StretchToFill);
                    GUILayout.BeginArea(buttonArea);
                    if (v.CompareTo(NodeEditorTWWindow.m_Version) > 0)
                    {
                        //str += "New version available " + v + " yours: " + NodeEditorTWWindow.m_Version;
                        EditorGUILayout.LabelField(str, EditorStyles.wordWrappedLabel);
                        if (GUILayout.Button("Go get New version "))
                        {
                            this.Close();
                            Application.OpenURL("https://github.com/dizzy2003/TextureWang");
                        }

                        if (GUILayout.Button("Ignore new version"))
                        {

                            this.Close();
                        }
                        return;

                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception in version number code "+ex);

                }
                GUILayout.EndArea();
            }
            else
            {
                if (m_Background != null)
                    GUI.DrawTexture(new Rect((m_Width - 512) * 0.5f, 0, 512, 512), m_Background, ScaleMode.ScaleAndCrop); //ScaleMode.StretchToFill);

                str += "\n\nConnecting to Server (to check for newer versions)";
                m_Count++;

                for (int i = 0; i < (m_Count >> 4)%10; i++)
                    str += ".";
            }


            GUILayout.BeginArea(buttonArea);
            EditorGUILayout.LabelField(str, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("OK"))
            {

                this.Close();
            }
            GUILayout.EndArea();

        }

        private void StartLoadImage()
        {
            string titlename = "http://54.237.244.93/TW" + NodeEditorTWWindow.m_Version + ".jpg";
            wwwImage = new WWW(titlename);
        }
    }
}
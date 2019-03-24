using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor.Graphs;
using UnityEditor.TreeViewExamples;
using Node = NodeEditorFramework.Node;


namespace TextureWang
{


  
    public class NodeEditorTWWindow : EditorWindow , ITreeDataProvider
    {
        public static Version m_Version = new Version(0,6,5,0);
        private string m_Name;
        private string m_LastLoadedName;
        // Information about current instance
        private static NodeEditorTWWindow _editor;
        private static NodeEditorTWWindow _editor2;

        public int m_DefaultTextureWidth;
        public int m_DefaultTextureHeight;
        
        public static NodeEditorTWWindow editor { get { AssureEditor (); return _editor; } }
		public static void AssureEditor () { if (_editor == null) OpenNodeEditor(); }

		
		public static string openedCanvasPath;
		public string tempSessionPath;

		// GUI
//		public static int sideWindowWidth = 400;
		private static Texture iconTexture;
//		public Rect sideWindowRect { get { return new Rect (position.width - sideWindowWidth, 0, sideWindowWidth, position.height); } }
		public Rect canvasWindowRect { get { return new Rect (0, 0, position.width , position.height); } }


        public MultiColumnWindow m_NodeSelectionWindow;
        public NodeInspectorWindow m_InspectorWindow;
        public bool m_Docked = false;
        public int m_DockedRetry = 0;
        // Opened Canvas
        public static NodeEditorUserCache canvasCache;


        private WWW wwwShader1 = null;
        public bool m_OpenAppearance;

        #region General 

        [MenuItem ("Window/TextureWang")]
		public static NodeEditorTWWindow OpenNodeEditor()
        {

            _editor = GetWindow<NodeEditorTWWindow>();//new Rect(0,0,1280,768),false,"TextureWang");
            _editor.minSize = new Vector2 (800, 600);
            


            NodeEditor.ClientRepaints += _editor.Repaint;

            NodeEditorInputControls.m_FinishedDupe += _editor.RecalcAll;

            //miked		NodeEditor.initiated = NodeEditor.InitiationError = false;

            iconTexture = ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_editor.titleContent = new GUIContent ("Texture Wang Nodes", iconTexture);
            return _editor;
        }

        void RecalcAll()
        {
            NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
        }
        /// <summary>
        /// Handle opening canvas when double-clicking asset
        /// </summary>

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool AutoOpenCanvas(int instanceID, int line)
        {
            if (Selection.activeObject != null && Selection.activeObject is NodeCanvas)
            {
                string NodeCanvasPath = AssetDatabase.GetAssetPath(instanceID);
                NodeEditorTWWindow.OpenNodeEditor();
                canvasCache.LoadNodeCanvas(NodeCanvasPath);
                return true;
            }
            return false;
        }
        int IDCounter = 0;
        MyTreeElement FindAndAddToParent(MyTreeElement _root, string[] _subContents,int _index,string _nodeID)
	    {
	        if (_root.children == null)
                _root.AddChild(new MyTreeElement(_subContents[_index],_root.depth+1,++IDCounter, _nodeID));
	            
	        foreach (var x in _root.children)
	        {
	            if (x.name == _subContents[_index])
	            {
	                if (_index == _subContents.Length - 1)
	                    return x as MyTreeElement;
                    
	                return FindAndAddToParent(x as MyTreeElement, _subContents, _index + 1, _nodeID);
	                
	            }
	        }
            var folder = new MyTreeElement(_subContents[_index], _root.depth+1, ++IDCounter, _nodeID);
            _root.AddChild(folder);
            if (_index == _subContents.Length - 1)
                return null;
            return FindAndAddToParent(folder, _subContents, _index + 1, _nodeID);

            
	    }
	    public IList<MyTreeElement> GetData()
	    {
            
            var treeElements = new List<MyTreeElement>();

            var root = new MyTreeElement("Root", -1, ++IDCounter,"root");
            treeElements.Add(root);
            //var child = new MyTreeElement("Element " + IDCounter, root.depth + 1, ++IDCounter);
            //treeElements.Add(child);

            
            foreach (Node node in NodeTypes.nodes.Keys)
            {

                string path = NodeTypes.nodes[node].adress;
                if (path.Contains("/"))
                {
                    // is inside a group
                    string[] subContents = path.Split('/');
                    string folderPath = subContents[0];
                    FindAndAddToParent(root, subContents, 0, node.GetID);
                }
                else
                {
                    var ele = new MyTreeElement(path, root.depth+1, ++IDCounter,node.GetID);
                    treeElements.Add(ele);

                }


            }
            var sub = new MyTreeElement("Subroutines", 0, ++IDCounter, "folder");
            root.AddChild(sub);
            var subs = GetAtPath<NodeCanvas>("TextureWang/Subroutines");//Resources.LoadAll<NodeCanvas>(NodeEditor.editorPath + "Resources/Saves/");
	        foreach (var x in subs)
	        {
	            var s = new MyTreeElement(x.name, sub.depth+1, ++IDCounter, "");
                sub.AddChild(s);
	            s.m_Canvas = x;
	        }
            sub = new MyTreeElement("UserSubroutines", 0, ++IDCounter, "folder");
            root.AddChild(sub);
            subs = GetAtPath<NodeCanvas>("TextureWang/UserSubroutines");//Resources.LoadAll<NodeCanvas>(NodeEditor.editorPath + "Resources/Saves/");
            foreach (var x in subs)
            {
                var s = new MyTreeElement(x.name, sub.depth + 1, ++IDCounter, "");
                sub.AddChild(s);
                s.m_Canvas = x;
            }

            var res = new List<MyTreeElement>();
            TreeElementUtility.TreeToList(root,res);
            return res;
	    }


        // Following section is all about caching the last editor session


        #endregion

        #region GUI

        [HotkeyAttribute(KeyCode.Delete, EventType.KeyDown)]
        private static void KeyDelete(NodeEditorInputInfo inputInfo)
        {
            inputInfo.SetAsCurrentEnvironment();
//            if (inputInfo.editorState.focusedNode != null)
            {
                NodeEditorInputControls.DeleteNodeOrNodes(inputInfo);
                //inputInfo.editorState.focusedNode.Delete();
                inputInfo.inputEvent.Use();
            }
        }
        [HotkeyAttribute(KeyCode.D, EventType.KeyDown)]
        private static void KeyDupe(NodeEditorInputInfo inputInfo)
        {
            inputInfo.SetAsCurrentEnvironment();
            NodeEditorState state = inputInfo.editorState;
            if (state.selectedNode != null||state.selectedNodes.Count>0)
            { // Create new node of same type
                
                NodeEditorInputControls.DuplicateNodeOrNodes(inputInfo);
                inputInfo.inputEvent.Use();
            }

            /*
                        NodeEditorState state = inputInfo.editorState;
                        if (state.focusedNode != null)
                        { // Create new node of same type
                            Node duplicatedNode = Node.Create(state.focusedNode.GetID, NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos), state.connectOutput);
                            duplicatedNode.CloneFieldsFrom(state.focusedNode);
                            //Node duplicatedNode = Node.Duplicate(state.focusedNode, NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                            state.selectedNode = state.focusedNode = duplicatedNode;
                            state.connectOutput = null;
                            inputInfo.inputEvent.Use();
                        }
            */
        }

        public static NodeEditorInputInfo ms_InputInfo;
        [HotkeyAttribute(KeyCode.R, EventType.KeyDown)]
        [ContextEntryAttribute(ContextType.Node, "[R]eplace Node")]
        private static void ReplaceNode(NodeEditorInputInfo inputInfo)
        {
            if (inputInfo.editorState.focusedNode != null && inputInfo.editorState.focusedNode is TextureNode)
            {
                m_ReplaceNode = inputInfo.editorState.focusedNode;
                ms_InputInfo = inputInfo;
            }
        }

        [HotkeyAttribute(KeyCode.H, EventType.KeyDown)]
        [ContextEntryAttribute(ContextType.Node, "Preview Node [H]istogram")]
        private static void InspectNode(NodeEditorInputInfo inputInfo)
        {
            inputInfo.SetAsCurrentEnvironment();
            if (inputInfo.editorState.focusedNode != null && inputInfo.editorState.focusedNode is TextureNode)
            {
                (inputInfo.editorState.focusedNode as TextureNode).OpenPreview();
                inputInfo.inputEvent.Use();
            }
        }


        public static void RequestLoad(NodeCanvas _canvas)
        {
            
            
            string assetPath = AssetDatabase.GetAssetPath(_canvas);
            
            NodeEditor.curEditorState = null;
            canvasCache.LoadNodeCanvas(assetPath);
            canvasCache.NewEditorState();

        }

        [EventHandlerAttribute(EventType.MouseDrag)]
        private static void HandleWindowPanning(NodeEditorInputInfo inputInfo)
        {
            NodeEditorState state = inputInfo.editorState;
            if (state.dragSelection)
            { // Calculate change in panOffset
//                Debug.Log("Mouse Drag " + inputInfo.inputPos+" diff "+(state.dragStart - inputInfo.inputPos)+" dragOff "+ state.dragOffset+" panOff "+ state.panOffset);

                
//                Vector2 panOffsetChange = state.dragOffset;
                state.dragOffset = inputInfo.inputPos - state.dragStart;
//                panOffsetChange = (state.dragOffset - panOffsetChange) * state.zoom;
                // Apply panOffsetChange to panOffset
                //state.panOffset += panOffsetChange;
                NodeEditor.RepaintClients();
            }
        }

        [EventHandlerAttribute(EventType.MouseDown, 120)] // Priority over hundred to make it call after the GUI
        private static void HandleNodeDraggingStart(NodeEditorInputInfo inputInfo)
        {
            if (GUIUtility.hotControl > 0)
                return; // GUI has control

            NodeEditorState state = inputInfo.editorState;
            if (inputInfo.inputEvent.button == 0 && state.focusedNode == null  && state.focusedNodeKnob == null)
            { // Clicked inside the selected Node, so start dragging it
//                Debug.Log("Start Mouse Drag "+ inputInfo.inputPos);
                
                state.dragSelection = true;
                state.selectedNodes.Clear();

                state.dragStart = inputInfo.inputPos;

                state.dragOffset = Vector2.zero;
                inputInfo.inputEvent.delta = Vector2.zero;

            }
        }
        [EventHandlerAttribute(EventType.MouseUp, 120)] // Priority over hundred to make it call after the GUI
        private static void HandleNodeDraggingEnd(NodeEditorInputInfo inputInfo)
        {
            if (GUIUtility.hotControl > 0)
                return; // GUI has control

            NodeEditorState state = inputInfo.editorState;
            if (inputInfo.inputEvent.button == 0 && state.dragSelection)// && state.focusedNode == null && state.focusedNodeKnob == null)
            { // Clicked inside the selected Node, so start dragging it
//                Debug.Log("End Mouse Drag " + inputInfo.inputPos);
                state.dragSelection = false;
                Vector2 tl = NodeEditor.ScreenToCanvasSpace(state.dragStart);
                Vector2 br = NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos);
                Rect canvasrect = new Rect(tl , (br - tl));

                NodeEditor.SelectNodes(canvasrect);
                NodeEditor.RepaintClients();
                /*
                                state.dragNode = true;
                                state.dragStart = inputInfo.inputPos;
                                state.dragPos = state.focusedNode.rect.position; // Need this here because of snapping
                                state.dragOffset = Vector2.zero;
                                inputInfo.inputEvent.delta = Vector2.zero;
                */
            }
        }

        [EventHandlerAttribute(EventType.DragUpdated, 90)] // Priority over hundred to make it call after the GUI
        [EventHandlerAttribute(EventType.DragPerform, 90)] // Priority over hundred to make it call after the GUI
        private static void HandleDragAndDrop(NodeEditorInputInfo inputInfo)
        {
            if (inputInfo.inputEvent.type == EventType.DragUpdated || (inputInfo.inputEvent.type == EventType.DragPerform))
            {

                //Debug.LogError("handle drag " + inputInfo.inputEvent.type);
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (inputInfo.inputEvent.type == EventType.DragPerform && DragAndDrop.objectReferences.Length > 0 &&
                    (DragAndDrop.objectReferences[0] is Texture2D))
                {
                    Debug.Log(" drag texture " + DragAndDrop.objectReferences[0]);
                    UnityTextureInput node =
                        (UnityTextureInput)
                            Node.Create("UnityTextureInput", NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                    node.m_Input = DragAndDrop.objectReferences[0] as Texture2D;
                    inputInfo.inputEvent.Use();

                }
                else if (inputInfo.inputEvent.type == EventType.DragPerform &&
                         DragAndDrop.GetGenericData("GenericDragColumnDragging") != null)
                {
                    DragAndDrop.AcceptDrag();
                    //Debug.Log("dragged generic data "+ DragAndDrop.GetGenericData("GenericDragColumnDragging"));
                    List<UnityEditor.IMGUI.Controls.TreeViewItem> _data =
                        DragAndDrop.GetGenericData("GenericDragColumnDragging") as
                            List<UnityEditor.IMGUI.Controls.TreeViewItem>;
                    var draggedElements = new List<TreeElement>();
                    foreach (var x in _data)
                        draggedElements.Add(((TreeViewItem<MyTreeElement>) x).data);

                    var srcItem = draggedElements[0] as MyTreeElement;
                    if (srcItem.m_Canvas)
                    {
                        SubTreeNode node =
                            (SubTreeNode)
                                Node.Create("SubTreeNode", NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                        node.SubCanvas = srcItem.m_Canvas;
                        string assetPath = AssetDatabase.GetAssetPath(node.SubCanvas);
                        //                    Debug.Log("drag and drop asset canvas path >" + assetPath + "<");
                        if (assetPath.Length > 0)
                        {
                            node.m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
//                            Debug.LogError(" set canvasGuid from asset >" + node.m_CanvasGuid + "<");
                            string fname = Path.GetFileName(assetPath);
                            fname = Path.ChangeExtension(fname, "");
                            node.name = "Sub:" + fname;
                        }


                        node.OnLoadCanvas();
                        m_PostOnLoadCanvasFixup = node;
                        NodeEditor.RecalculateFrom(node);

                    }
                    else
                    {
                        Node.Create(srcItem.m_NodeID, NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                    }
                    inputInfo.inputEvent.Use();

                    
                }
                else if (inputInfo.inputEvent.type == EventType.DragPerform &&
                         DragAndDrop.objectReferences.Length > 0 && (DragAndDrop.objectReferences[0] is NodeCanvas))
                {
                    DragAndDrop.AcceptDrag();
                    //                    Debug.Log(" drag and drop " + DragAndDrop.objectReferences[0] + " co ord " + Event.current.mousePosition);
                    SubTreeNode node =
                        (SubTreeNode)
                            Node.Create("SubTreeNode", NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                    node.SubCanvas = (NodeCanvas) DragAndDrop.objectReferences[0];
                    string assetPath = AssetDatabase.GetAssetPath(node.SubCanvas);
                    //                    Debug.Log("drag and drop asset canvas path >" + assetPath + "<");
                    if (assetPath.Length > 0)
                    {
                        node.m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
                        Debug.LogError(" set canvasGuid from asset >" + node.m_CanvasGuid + "<");
                        string fname = Path.GetFileName(assetPath);
                        fname = Path.ChangeExtension(fname, "");
                        node.name = "Sub:" + fname;
                    }
                    inputInfo.inputEvent.Use();

                    node.OnLoadCanvas();
                    m_PostOnLoadCanvasFixup = node;

                }
            }
        }

        private static SubTreeNode m_PostOnLoadCanvasFixup;
        private void OnGUI () 
		{
            if (wwwShader1 != null)
            {

                if (wwwShader1.isDone)
                {
                    string pathShader = "Assets/TextureWang/Shaders/TextureOps.shader";
                    pathShader = pathShader.Replace("/", "" + Path.DirectorySeparatorChar);

                    File.WriteAllBytes(pathShader, wwwShader1.bytes);
                    AssetDatabase.ImportAsset(pathShader, ImportAssetOptions.ForceSynchronousImport);
                    wwwShader1 = null;
                }
                Repaint();
            }


            if (m_ReplaceNode != null && !OverlayGUI.HasPopupControl() && ms_InputInfo!=null)
            {
                NodeEditorInputSystem.ShowContextMenu(ms_InputInfo);
                ms_InputInfo = null;
            }

/*
            if (NodeEditor.curEditorState == null)
            {
                Debug.Log("OnGUI::TWWindow has no editor state " + NodeEditor.curEditorState+"actual editor state "+ canvasCache.editorState);
            }
            else if (NodeEditor.curEditorState.selectedNode == null)
            {
                Debug.Log("OnGUI::TWWindow has no Selected Node " + NodeEditor.curEditorState);
            }
            else
            {
                Debug.Log("OnGUI:: Selected Node " + NodeEditor.curEditorState.selectedNode);
            }
*/
            // Initiation
            NodeEditor.checkInit(true);
            if (NodeEditor.InitiationError)
            {
                GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
                return;
            }
            AssureEditor();
            canvasCache.AssureCanvas();

            // Specify the Canvas rect in the EditorState
            canvasCache.editorState.canvasRect = canvasWindowRect;
            // If you want to use GetRect:
            //			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
            //			if (Event.current.type != EventType.Layout)
            //				mainEditorState.canvasRect = canvasRect;
            NodeEditorGUI.StartNodeGUI();

            // Perform drawing with error-handling
            try
            {

                NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);


                if (canvasCache.editorState.selectedNode != null)
                {
                    if (canvasCache.editorState.selectedNode is TextureNode)
                    {
                        var tn = canvasCache.editorState.selectedNode as TextureNode;
                        if (tn.m_RequestRepaint)
                        {
                            tn.m_RequestRepaint = false;
                            Repaint();
                            m_InspectorWindow.Repaint();
                        }
                    }
                }
                if (wwwShader1 != null)
                    GUI.Label(new Rect(100, 100, 500, 200), "One Time Shader Download in progress...");


            }
            catch (UnityException e)
            { // on exceptions in drawing flush the canvas to avoid locking the ui.
                canvasCache.NewNodeCanvas();
                NodeEditor.ReInit(true);
                Debug.LogError("Unloaded Canvas due to an exception during the drawing phase!");
                Debug.LogException(e);
            }
            if (m_PostOnLoadCanvasFixup != null)
            {
//                m_PostOnLoadCanvasFixup.PostOnLoadCanvasFixup();
                m_PostOnLoadCanvasFixup = null;
            }


            // Draw Side Window
            //sideWindowWidth = Math.Min(600, Math.Max(200, (int)(position.width / 5)));
            //GUILayout.BeginArea(sideWindowRect, GUI.skin.box);
            //DrawSideWindow();
            //GUILayout.EndArea();


            NodeEditorGUI.EndNodeGUI();
//            if (Event.current.type == EventType.Repaint)
//                m_InspectorWindow.Repaint();
/*
            //if (Event.current.type == EventType.Repaint)
            {
                if (mainEditorState.selectedNode != mainEditorState.wantselectedNode)
                {
                    mainEditorState.selectedNode = mainEditorState.wantselectedNode;
                    NodeEditor.RepaintClients();
                    Repaint();
                }

            }
*/
                
		    if (!m_Docked && m_DockedRetry++<100 && m_InspectorWindow!=null && m_NodeSelectionWindow!=null)
		    {
		        try
		        {
		            m_DockedRetry++;
                    Docker.Dock(this, m_InspectorWindow, Docker.DockPosition.Right);
                    Docker.Dock(this, m_NodeSelectionWindow, Docker.DockPosition.Left);

                }
                catch (Exception ex)
		        {
                    Debug.LogError(" Dock failed "+ex);		            
		            
		        }
		        m_Docked = true;

		    }
        }

        void OnLoadCanvas(NodeCanvas _canvas)
        {
            foreach (var n in _canvas.nodes)
            {
                if (n is TextureNode)
                {
                    (n as TextureNode).OnLoadCanvas();
                }
            }
        }

        private void OnEnable()
        {
            Debug.Log("NodeEditorTWWindow enabled");
            _editor = this;
            NodeEditor.checkInit(false);

            NodeEditorCallbacks.OnLoadCanvas -= OnLoadCanvas;
            NodeEditorCallbacks.OnLoadCanvas += OnLoadCanvas;

            NodeEditorInputControls.m_FinishedDupe -= _editor.RecalcAll;
            NodeEditorInputControls.m_FinishedDupe += _editor.RecalcAll;


            NodeEditor.ClientRepaints -= Repaint;
            NodeEditor.ClientRepaints += Repaint;

            EditorLoadingControl.justLeftPlayMode -= NormalReInit;
            EditorLoadingControl.justLeftPlayMode += NormalReInit;
            // Here, both justLeftPlayMode and justOpenedNewScene have to act because of timing
            EditorLoadingControl.justOpenedNewScene -= NormalReInit;
            EditorLoadingControl.justOpenedNewScene += NormalReInit;

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            string assetPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            if (assetPath.Length > 1)
            {
//                Debug.LogError("asset path " + assetPath);
                string path = Path.GetDirectoryName(assetPath);
                // Setup Cache
                canvasCache = new NodeEditorUserCache(path);
            }
            else
            {
                Debug.LogError("UNKNOWN asset path " + assetPath);
                canvasCache = new NodeEditorUserCache(); //path);
            }
            bool loadCache = true;
            if (!Shader.Find("Hidden/TextureOps"))
            {
                string shadername1 = "http://54.237.244.93/TextureOps" + NodeEditorTWWindow.m_Version + ".txt";
                wwwShader1 = new WWW(shadername1);
                loadCache = false;
            }
            
            canvasCache.SetupCacheEvents(loadCache);

            m_NodeSelectionWindow = MultiColumnWindow.GetWindow(this);

            m_InspectorWindow = NodeInspectorWindow.Init(this);
            NodeEditor.ClientRepaints += m_InspectorWindow.Repaint;
            StartTextureWangPopup.Init(this);

            m_InspectorWindow.m_Source = this;
            NodeEditorCallbacks.OnAddNode -= NewNodeCallback;
            NodeEditorCallbacks.OnAddNode += NewNodeCallback;

        }

        private static Node m_ReplaceNode;
        private void NewNodeCallback(Node node)
        {
            if (m_ReplaceNode != null)
            {
                for (int index = 0; index < m_ReplaceNode.Inputs.Count; index++)
                {
                    var oldInput = m_ReplaceNode.Inputs[index];
                    if (oldInput.connection != null)
                    {
                        for (int indexNewNodesInputs = 0; indexNewNodesInputs < node.Inputs.Count; indexNewNodesInputs++)
                        {
                            var newInput = node.Inputs[indexNewNodesInputs];
                            if (newInput.connection == null && newInput.typeID==oldInput.typeID)
                            {
                                newInput.ApplyConnection(oldInput.connection);
                                //newInput.connection = oldInput.connection;
                                break;
                            }
                        }


                    }
                 }
                for (int index = 0; index < m_ReplaceNode.Outputs.Count; index++)
                {
                    var oldOutput = m_ReplaceNode.Outputs[index];
                    if (oldOutput.connections != null)
                    {
                        for (int indexNewNodesOutputs = 0; indexNewNodesOutputs < node.Outputs.Count; indexNewNodesOutputs++)
                        {
                            var newOutput = node.Outputs[indexNewNodesOutputs];
                            if ( newOutput.typeID == oldOutput.typeID)
                            {
                                for (int i = oldOutput.connections.Count-1;i >= 0; i--)
                                {
                                    
                                    var x = oldOutput.connections[i];
                                    x.ApplyConnection(newOutput);
                                    //newOutput.connections = oldOutput.connections;
                                }
                                break;
                            }
                        }


                    }
                }
                node.rect = m_ReplaceNode.rect;
                m_ReplaceNode.Delete();
                m_ReplaceNode = null;
                NodeEditor.RecalculateFrom(node);
            }


        }
        private void NormalReInit()
        {
            NodeEditor.ReInit(false);
        }

        private void OnDestroy()
        {
            m_Closed = true;
            EditorUtility.SetDirty(canvasCache.nodeCanvas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            NodeEditorInputControls.m_FinishedDupe -= _editor.RecalcAll;


            NodeEditor.ClientRepaints -= Repaint;
            if(m_InspectorWindow!=null)
                NodeEditor.ClientRepaints -= m_InspectorWindow.Repaint;

            EditorLoadingControl.justLeftPlayMode -= NormalReInit;
            EditorLoadingControl.justOpenedNewScene -= NormalReInit;

            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            // Clear Cache
            canvasCache.ClearCacheEvents();
        }

        private bool m_Closed;
        private void OnSceneGUI(SceneView sceneview)
        {
            if(!m_Closed)
                DrawSceneGUI();
        }

        private void DrawSceneGUI()
        {
            if (canvasCache != null)
            {
                AssureEditor();
                canvasCache.AssureCanvas();
                if (canvasCache.editorState.selectedNode != null)
                    canvasCache.editorState.selectedNode.OnSceneGUI();
                SceneView.lastActiveSceneView.Repaint();
            }
        }
        public static T[] GetAtPath<T>(string path)
        {

            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

            foreach (string fileName in fileEntries)
            {
                int assetPathIndex = fileName.IndexOf("Assets");
                string localPath = fileName.Substring(assetPathIndex);

                UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }
            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }

        Node PriorLoop(Node n)
        {
            List<Node> nodes=new List<Node>();
            nodes.Add(n);
            for (int index = 0; index < nodes.Count; index++)
            {
                var node = nodes[index];
                foreach (var i in node.Outputs)
                {
                    if(i!=null)// && i.connection!=null)
                    foreach (var c in i.connections)
                    {
                        Node body = c.connection.body;
                        if (!nodes.Contains(body))
                            nodes.Add(body);
                        if (body is LoopBasic)
                            return body;
                    }
                }
            }
            return n;
        }

        

        public void DrawSideWindow ()
        {
            string canvasName= "Node Canvas";
            if (canvasCache != null && !string.IsNullOrEmpty(canvasCache.openedCanvasPath))
            {
                canvasName = canvasCache.openedCanvasPath.Substring(canvasCache.openedCanvasPath.LastIndexOf(@"/") + 1);
                canvasName = canvasName.Replace(".asset", "");
            }

            GUILayout.Label (new GUIContent ( canvasName));

			if (GUILayout.Button (new GUIContent ("Save Canvas", "Saves the Canvas to a Canvas Save File in the Assets Folder")))
			{
			    string lastpath = NodeEditor.editorPathLoadSave + "Saves/";
                lastpath = GetLastUsedPath(lastpath);
                string path = EditorUtility.SaveFilePanelInProject("Save Node Canvas", canvasName, "asset", "", lastpath);
                if (!string.IsNullOrEmpty(path))
                    canvasCache.SaveNodeCanvas(path);
/*
                string path = EditorUtility.SaveFilePanelInProject ("Save Node Canvas", m_LastLoadedName, "asset", "", NodeEditor.editorPath + "Resources/Saves/");
			    if (!string.IsNullOrEmpty(path))
			    {
			        SaveNodeCanvas(path); 

                }
*/
                if(m_NodeSelectionWindow!=null)
			        m_NodeSelectionWindow.ReInit();
			}
            /*
                        if (GUILayout.Button(new GUIContent("New Canvas",
                                "Create a copy")))
                        {
                            CreateEditorCopy();
                        }
            */

            if (GUILayout.Button(new GUIContent("Load Canvas", "Loads the Canvas from a Canvas Save File in the Assets Folder")))
            {
                string lastpath = NodeEditor.editorPathLoadSave + "Saves/";
                lastpath = GetLastUsedPath(lastpath);
                Debug.Log(" last path for load canvas"+lastpath);
                string path = EditorUtility.OpenFilePanel("Load Node Canvas", lastpath, "asset");

                if (!path.Contains(Application.dataPath))
                {
                    if (!string.IsNullOrEmpty(path))
                        ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
                }
                else
                {
                    NodeEditor.curEditorState = null;
                    canvasCache.LoadNodeCanvas(path);
                    canvasCache.NewEditorState();
                    
                }
                NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
            }

            if (GUILayout.Button(new GUIContent("New TextureWang", "Create a new TextureWang Canvas")))
            {
                NewTextureWangPopup.Init(this);
            }


            if (GUILayout.Button(new GUIContent("Recalculate All","Initiates complete recalculate. Usually does not need to be triggered manually.")))
            {
                NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
                GUI.changed = false;
            }
            if (GUILayout.Button(new GUIContent("Recalculate All Export PNG'S", "Initiates complete recalculate. Usually does not need to be triggered manually.")))
            {
                UnityTextureOutput.ms_ExportPNG = true;
                NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
                GUI.changed = false;
                UnityTextureOutput.ms_ExportPNG = false;
            }

//            if (GUILayout.Button ("Force Re-Init"))
//				NodeEditor.ReInit (true);
            if (GUILayout.Button(new GUIContent("Double All texture Sizes", "")))
            {
                foreach (var n in canvasCache.nodeCanvas.nodes)
                {
                    if (n is TextureNode)
                    {
                        var tn = n as TextureNode;
                        tn.m_TexWidth *= 2;
                        tn.m_TexHeight *= 2;
                    }
                }
                NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
            }
            if (GUILayout.Button(new GUIContent("Set All Selected to Default Size", "")))
            {
                foreach (var n in canvasCache.editorState.selectedNodes)
                {
                    if (n is TextureNode)
                    {
                        var tn = n as TextureNode;
                        tn.m_TexWidth = canvasCache.nodeCanvas.m_DefaultTextureWidth;
                        tn.m_TexHeight = canvasCache.nodeCanvas.m_DefaultTextureHeight;
                    }
                }

                NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
            }

            if (GUILayout.Button(new GUIContent("Halve All texture Sizes", "")))
            {
                foreach (var n in canvasCache.nodeCanvas.nodes)
                {
                    if (n is TextureNode)
                    {
                        var tn = n as TextureNode;
                        tn.m_TexWidth >>= 1;
                        tn.m_TexHeight >>= 1;
                    }
                }
                NodeEditor.RecalculateAll(canvasCache.nodeCanvas);
            }

            bool changed = GUI.changed;
            GUI.changed = false;
            NodeEditorGUI.knobSize = EditorGUILayout.IntSlider (new GUIContent ("Handle Size", "The size of the Node Input/Output handles"), NodeEditorGUI.knobSize, 12, 20);
            canvasCache.editorState.zoom = EditorGUILayout.Slider(new GUIContent("Zoom", "Use the Mousewheel. Seriously."), canvasCache.editorState.zoom, 0.2f, 4);
            m_OpenAppearance = EditorGUILayout.Foldout(m_OpenAppearance, "");
            if (m_OpenAppearance)
            {
                Node.m_DropShadowMult = EditorGUILayout.Slider(new GUIContent("DropShadow", ""), Node.m_DropShadowMult,
                    0.0f, 1);
                Node.m_DropShadowMult2 = EditorGUILayout.Slider(new GUIContent("DropShadow Wires", ""),
                    Node.m_DropShadowMult2, 0.0f, 1);
                Node.m_WireSize = EditorGUILayout.Slider(new GUIContent("m_WireSize", ""), Node.m_WireSize, 0.0f, 30);
                Node.m_WireSize2 = EditorGUILayout.Slider(new GUIContent("WireSize Shadow", ""), Node.m_WireSize2, 0.0f,
                    50);
                Node.m_Saturation = EditorGUILayout.Slider(new GUIContent("Saturation", ""), Node.m_Saturation, 0.0f, 1);
                Node.m_WireColbright = EditorGUILayout.Slider(new GUIContent("m_WireColbright", ""),
                    Node.m_WireColbright, 0.0f, 1);

                Node.m_DropShadowOffset = EditorGUILayout.Slider(new GUIContent("DropShadowOffset", ""),
                    Node.m_DropShadowOffset, -50.0f, 50);
                Node.m_DropShadowExpand = EditorGUILayout.Slider(new GUIContent("m_DropShadowExpand", ""),
                    Node.m_DropShadowExpand, 0f, 50);
            }
            bool changeView = GUI.changed;
            GUI.changed = changed;
            if (canvasCache.nodeCanvas != null)
            {
                canvasCache.nodeCanvas.m_DefaultTextureWidth =
                    EditorGUILayout.IntSlider(new GUIContent("Default tex Width", ""),
                        canvasCache.nodeCanvas.m_DefaultTextureWidth, 1, 4096);
                canvasCache.nodeCanvas.m_DefaultTextureHeight =
                    EditorGUILayout.IntSlider(new GUIContent("Default tex Height", ""),
                        canvasCache.nodeCanvas.m_DefaultTextureHeight, 1, 4096);

                canvasCache.nodeCanvas.m_DefaultChannelType = (ChannelType)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("pixel Depth", ""), canvasCache.nodeCanvas.m_DefaultChannelType);

                
            }


            //miked            mainNodeCanvas.scaleMode = (ScaleMode)EditorGUILayout.EnumPopup(new GUIContent("ScaleMode", ""), mainNodeCanvas.scaleMode, GUILayout.MaxWidth(200));



            //        m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            //            if (mainNodeCanvas != null)
            {
                //                EditorGUILayout.LabelField("width: " + mainNodeCanvas.m_TexWidth);
                //                EditorGUILayout.LabelField("height: " + mainNodeCanvas.m_TexHeight);
            }
/*
            if (NodeEditor.curEditorState == null)
            {
                Debug.Log("TWWindow has no editor state " + NodeEditor.curEditorState);
            }
            else if (NodeEditor.curEditorState.selectedNode == null)
            {
                Debug.Log("TWWindow has no Selected Node " + NodeEditor.curEditorState);
            }
            else
            {
                Debug.Log(" Selected Node " + NodeEditor.curEditorState.selectedNode);
            }
*/            
            if (canvasCache.editorState != null && canvasCache.editorState.selectedNode != null)
                // if (Event.current.type != EventType.Ignore)
            {
                RTEditorGUI.Seperator();
                GUILayout.Label(canvasCache.editorState.selectedNode.name);
                RTEditorGUI.Seperator();
                if (canvasCache.editorState.selectedNode is SubTreeNode)
                {
                    if (GUILayout.Button("Edit Sub Canvas"))
                    {
                        string NodeCanvasPath = AssetDatabase.GUIDToAssetPath((canvasCache.editorState.selectedNode as SubTreeNode).m_CanvasGuid);

                        NodeEditor.curEditorState = null;
                        canvasCache.LoadNodeCanvas(NodeCanvasPath);
                        canvasCache.NewEditorState();
                    }
                }
                if(canvasCache.editorState.selectedNode)
                    canvasCache.editorState.selectedNode.DrawNodePropertyEditor();
                if (GUI.changed && canvasCache.editorState.selectedNode)
                    NodeEditor.RecalculateFrom(PriorLoop(canvasCache.editorState.selectedNode));
                else
                if(changeView)
                {
                    Repaint();
                }

            }

            

            //            var assets = UnityEditor.AssetDatabase.FindAssets("NodeCanvas"); 
            //            foreach(var x in assets)
            //                GUILayout.Label(new GUIContent("Node Editor (" + x + ")", "Opened Canvas path: " ), NodeEditorGUI.nodeLabelBold);
            /*
                        if (m_All == null)
                            m_All = GetAtPath<NodeCanvas>("Node_Editor-master/Node_Editor/Resources/Saves");//Resources.LoadAll<NodeCanvas>(NodeEditor.editorPath + "Resources/Saves/");

                        scrollPos =EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Height(600));
                        guiStyle.fontSize = 20;
                        guiStyle.fixedHeight = 20;
                        foreach (var x in m_All)
                             EditorGUILayout.SelectableLabel("(" + x.name + ")", guiStyle);
                        EditorGUILayout.EndScrollView();
            */
        }

        private static string GetLastUsedPath(string lastpath)
        {
            if (canvasCache != null && !string.IsNullOrEmpty(canvasCache.openedCanvasPath))
            {
                int indexOfAssetSubString = canvasCache.openedCanvasPath.LastIndexOf(@"/Assets");
                if (indexOfAssetSubString >= 0)
                {
                    lastpath = canvasCache.openedCanvasPath.Substring(indexOfAssetSubString);
                    lastpath = lastpath.Replace(".asset", "");
                }
                else
                {
                    Debug.LogError("Failed to find .asset in path " + canvasCache.openedCanvasPath);
                }
            }
            return lastpath;
        }

        Vector2 scrollPos;
        private NodeCanvas[] m_All;
        //private GUIStyle guiStyle = new GUIStyle();
        #endregion




        // Opened Canvas
        





        /// <summary>
        /// Creates and opens a new empty node canvas
        /// </summary>
        public void NewNodeCanvas (int _texWidth=1024,int _texHeight=1024,ChannelType _pixelDepth=ChannelType.Float)
        {

            canvasCache.NewNodeCanvas(null,_texWidth, _texHeight, _pixelDepth);
/*
            // New NodeCanvas
            mainNodeCanvas = CreateInstance<NodeCanvas> ();
			mainNodeCanvas.name = "New Canvas";
//miked		    mainNodeCanvas.m_TexWidth = _texWidth;
//miked            mainNodeCanvas.m_TexHeight = _texHeight;
            // New NodeEditorState
            mainEditorState = CreateInstance<NodeEditorState> ();
			mainEditorState.canvas = mainNodeCanvas;
			mainEditorState.name = "MainEditorState";
		    NodeEditor.curNodeCanvas = mainNodeCanvas;
            openedCanvasPath = "";
			SaveCache ();
*/
		}
        public void LoadSceneCanvasCallback(object canvas)
        {
            canvasCache.LoadSceneNodeCanvas((string)canvas);
        }

        /*
				    if (e.alt)
				    {
                        Dictionary<Node, int> d = new Dictionary<Node, int>();
                        d[curEditorState.selectedNode] = 1;

				        MoveChildren(ref d, curEditorState.selectedNode, delta);
				    }
        */
        static void MoveChildren(ref Dictionary<Node, int> _dic, Node _n, Vector2 _delta)
        {
            foreach (var input in _n.Inputs)
            {
                Node cn = input.connection.body;
                if (!_dic.ContainsKey(cn))
                {

                    cn.rect.position += _delta;
                    NodeEditorCallbacks.IssueOnMoveNode(cn);
                    MoveChildren(ref _dic, cn, _delta);
                    _dic[cn] = 1;
                }
            }

        }

    }
}
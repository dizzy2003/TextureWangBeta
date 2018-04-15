using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework 
{
	/// <summary>
	/// Collection of default Node Editor controls for the NodeEditorInputSystem
	/// </summary>
	public static class NodeEditorInputControls
	{
		#region Canvas Context Entries

		[ContextFillerAttribute (ContextType.Canvas)]
		private static void FillAddNodes (NodeEditorInputInfo inputInfo, GenericMenu canvasContextMenu) 
		{ // Show all nodes, and if a connection is drawn, only compatible nodes to auto-connect
			NodeEditorState state = inputInfo.editorState;
			List<Node> displayedNodes = state.connectOutput != null? NodeTypes.getCompatibleNodes (state.connectOutput) : NodeTypes.nodes.Keys.ToList ();
			foreach (Node compatibleNode in displayedNodes)
			{
				if (NodeCanvasManager.CheckCanvasCompability (compatibleNode, inputInfo.editorState.canvas.GetType ()))
					canvasContextMenu.AddItem (new GUIContent ("Add " + NodeTypes.nodes[compatibleNode].adress), false, CreateNodeCallback, new NodeEditorInputInfo (compatibleNode.GetID, state));
			}
		}

		private static void CreateNodeCallback (object infoObj)
		{
			NodeEditorInputInfo callback = infoObj as NodeEditorInputInfo;
			if (callback == null)
				throw new UnityException ("Callback Object passed by context is not of type NodeEditorInputInfo!");

			callback.SetAsCurrentEnvironment ();
			Node.Create (callback.message, NodeEditor.ScreenToCanvasSpace (callback.inputPos), callback.editorState.connectOutput);
			callback.editorState.connectOutput = null;
			NodeEditor.RepaintClients ();
		}

		#endregion

		#region Node Context Entries

		[ContextEntryAttribute (ContextType.Node, "Delete Node")]
		private static void DeleteNode (NodeEditorInputInfo inputInfo)
		{
		    DeleteNodeOrNodes(inputInfo);
		    inputInfo.inputEvent.Use();
		}

	    public static void DeleteNodeOrNodes(NodeEditorInputInfo inputInfo)
	    {
	        if (inputInfo.editorState.selectedNode == null && NodeEditor.curEditorState.selectedNodes.Count == 0)
	            return;

	        inputInfo.SetAsCurrentEnvironment();
	        //if (inputInfo.editorState.focusedNode != null)
	        {
	            if (inputInfo.editorState.selectedNode!=null && !NodeEditor.curEditorState.selectedNodes.Contains(inputInfo.editorState.selectedNode))
                    NodeEditor.curEditorState.selectedNodes.Add(inputInfo.editorState.selectedNode);
                {
	                
	                foreach (var n in NodeEditor.curEditorState.selectedNodes)
	                    n.Delete();
	                NodeEditor.curEditorState.selectedNodes.Clear();
	            }
	            inputInfo.editorState.selectedNode = null;
	        }
	    }

	    [ContextEntryAttribute (ContextType.Node, "Duplicate Node")]
		private static void DuplicateNode (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.SetAsCurrentEnvironment ();
            NodeEditorState state = inputInfo.editorState;
            //if (state.focusedNode != null)
			{
			    // Create new node of same type
			    DuplicateNodeOrNodes(inputInfo);
			    inputInfo.inputEvent.Use ();
			}
		}

	    public delegate void CacheFunction();

	    public static Action m_CacheOff;
        public static Action m_CacheOn;
        public static Action m_FinishedDupe;
        public static void DuplicateNodeOrNodes(NodeEditorInputInfo inputInfo)
	    {
            if (inputInfo.editorState.selectedNode == null && NodeEditor.curEditorState.selectedNodes.Count == 0)
                return;
            inputInfo.SetAsCurrentEnvironment();
            NodeEditorState state = inputInfo.editorState;
            if (inputInfo.editorState.selectedNode!=null && !NodeEditor.curEditorState.selectedNodes.Contains(inputInfo.editorState.selectedNode))
                NodeEditor.curEditorState.selectedNodes.Add(inputInfo.editorState.selectedNode);

            {
//                if(m_CacheOff!=null)
//                    m_CacheOff();

                Dictionary<Node,Node>  nodeMap= new Dictionary<Node, Node>();
	            List<Node> newSelected = new List<Node>();
	            foreach (var n in NodeEditor.curEditorState.selectedNodes)
	            {
	                Node duplicated = Node.Create(n.GetID, n.rect.center, null,false);//state.connectOutput);
//                    Debug.Log("dupe n "+n);
	                duplicated.CloneFieldsFrom(n);
	                state.selectedNode = state.focusedNode = duplicated;
	                newSelected.Add(duplicated);
                    nodeMap.Add(n,duplicated );
	            }
                //handle connections
	            foreach (var n in NodeEditor.curEditorState.selectedNodes)
	            {
	                for (int index = 0; index < n.Inputs.Count; index++)
	                {
	                    NodeInput nodeInput = n.Inputs[index];
	                    if (nodeInput.connection != null)
	                    {
	                        Node connectTo = nodeInput.connection.body;
	                        if (nodeMap.ContainsKey(nodeInput.connection.body))
	                            connectTo = nodeMap[nodeInput.connection.body];

	                        foreach (NodeOutput on in connectTo.Outputs)
	                        {
	                            if (on.name == nodeInput.connection.name)
	                            {
                                    if(index< nodeMap[n].Inputs.Count)
	                                    nodeMap[n].Inputs[index].ApplyConnection(on);
	                                break;
	                            }
	                        }
	                    }
	                }
                    for (int index = 0; index < n.Outputs.Count; index++)
                    {
                        NodeOutput nodeOutput = n.Outputs[index];

                        for (int index1 = 0; index1 < nodeOutput.connections.Count; index1++)
                        {
                            NodeInput nodeInput = nodeOutput.connections[index1];
                            if (nodeInput == null)
                                continue;

                            Node connectTo = nodeInput.body;
                            if (nodeMap.ContainsKey(connectTo))
                                continue; //we did it allready

                            for (int i = 0; i < connectTo.Inputs.Count; i++)
                            {
                                NodeInput inputnode = connectTo.Inputs[i];
                                if (inputnode.name == nodeInput.name)
                                {
                                    inputnode.ApplyConnection(nodeOutput);
                                    break;
                                }
                            }
                        }
                    }
	            }

                NodeEditor.curEditorState.selectedNodes.Clear();
	            NodeEditor.curEditorState.selectedNodes = newSelected;
	            NodeEditor.curEditorState.selectedNode = newSelected[0];
//                foreach (var x in newSelected)
//                    Debug.Log(" selected node "+x);
//                if (m_CacheOn != null)
//                    m_CacheOn();
                NodeEditorCallbacks.IssueOnAddNode(NodeEditor.curEditorState.selectedNode); //just do it on one node, so it saves the canvas/repaints
            }
	      
	        state.connectOutput = null;

            if (m_FinishedDupe!=null)
                m_FinishedDupe();
	    }

	    #endregion

		#region Node Keyboard Control

		// Main Keyboard_Move method
		[HotkeyAttribute(KeyCode.UpArrow, EventType.KeyDown)]
		[HotkeyAttribute(KeyCode.LeftArrow, EventType.KeyDown)]
		[HotkeyAttribute(KeyCode.RightArrow, EventType.KeyDown)]
		[HotkeyAttribute(KeyCode.DownArrow, EventType.KeyDown)]
		private static void KB_MoveNode(NodeEditorInputInfo inputInfo)
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.selectedNode != null)
			{ 
				Vector2 pos = state.selectedNode.rect.position;
				int shiftAmount = inputInfo.inputEvent.shift? 50 : 10;

				if (inputInfo.inputEvent.keyCode == KeyCode.RightArrow)
					pos = new Vector2(pos.x + shiftAmount, pos.y);
				else if (inputInfo.inputEvent.keyCode == KeyCode.LeftArrow)
					pos = new Vector2(pos.x - shiftAmount, pos.y);
				else if (inputInfo.inputEvent.keyCode == KeyCode.DownArrow)
					pos = new Vector2(pos.x, pos.y + shiftAmount);
				else if (inputInfo.inputEvent.keyCode == KeyCode.UpArrow)
					pos = new Vector2(pos.x, pos.y - shiftAmount);

				state.selectedNode.rect.position = pos;
				inputInfo.inputEvent.Use();
			}
			NodeEditor.RepaintClients();

		}


		#endregion

		#region Node Dragging

		[EventHandlerAttribute (EventType.MouseDown, 110)] // Priority over hundred to make it call after the GUI
		private static void HandleNodeDraggingStart (NodeEditorInputInfo inputInfo) 
		{
			if (GUIUtility.hotControl > 0)
				return; // GUI has control

			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && state.focusedNode != null && state.focusedNode == state.selectedNode && state.focusedNodeKnob == null) 
			{ // Clicked inside the selected Node, so start dragging it
				state.dragNode = true;
                state.didDragNode = false;
                state.dragStart = inputInfo.inputPos;
				state.dragPos = state.focusedNode.rect.position; // Need this here because of snapping
				state.dragOffset = Vector2.zero;
				inputInfo.inputEvent.delta = Vector2.zero;
			}
		}
        static void MoveChildren(ref Dictionary<Node, int> _dic, Node _n, Vector2 _delta)
        {
            foreach (var input in _n.Inputs)
            {
                if (input == null||input.connection==null)
                    continue;
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
        [EventHandlerAttribute (EventType.MouseDrag)]
		private static void HandleNodeDragging (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.dragNode) 
			{ // If conditions apply, drag the selected node, else disable dragging
				if (state.selectedNode != null && GUIUtility.hotControl == 0)
				{ // Calculate new position for the dragged object
					state.dragOffset = inputInfo.inputPos-state.dragStart;
                    Vector2 delta= ( state.dragPos + state.dragOffset * state.zoom)- state.selectedNode.rect.position;
				    if (delta.magnitude > 1.0f)
				        state.didDragNode = true;
                    if(state.selectedNode!=null && !state.selectedNodes.Contains(state.selectedNode))
                        state.selectedNode.rect.position = state.dragPos + state.dragOffset*state.zoom;
                    Vector2 deltaAll = (  state.dragOffset * state.zoom) ;
                    foreach (var n in state.selectedNodes)
				    {
                        n.rect.position += delta;
                    }

                    if (inputInfo.inputEvent.alt)
                    {
                        Dictionary<Node, int> d = new Dictionary<Node, int>();
                        d[state.selectedNode] = 1;

                        MoveChildren(ref d, state.selectedNode, delta);
                    }
                    NodeEditorCallbacks.IssueOnMoveNode (state.selectedNode);
					NodeEditor.RepaintClients ();
				} 
				else
					state.dragNode = false;
			}
		}

		[EventHandlerAttribute (EventType.MouseDown)]
        private static void HandleNodeDraggingEnd(NodeEditorInputInfo inputInfo)
        {

            inputInfo.editorState.dragNode = false;
        }
        [EventHandlerAttribute (EventType.MouseUp)]
		private static void HandleNodeDraggingRealEnd (NodeEditorInputInfo inputInfo) 
		{
            NodeEditorState state = inputInfo.editorState;
            if (!state.didDragNode)
		    {

                state.selectedNodes.Clear();
                state.selectedNode = state.focusedNode;
                NodeEditor.RepaintClients();
                
            }
			inputInfo.editorState.dragNode = false;
		}

		#endregion

		#region Window Panning

		[EventHandlerAttribute (EventType.MouseDown, 100)] // Priority over hundred to make it call after the GUI
		private static void HandleWindowPanningStart (NodeEditorInputInfo inputInfo) 
		{
			if (GUIUtility.hotControl > 0)
				return; // GUI has control

			NodeEditorState state = inputInfo.editorState;
			if ((/*inputInfo.inputEvent.button == 0 ||*/ inputInfo.inputEvent.button == 2) && state.focusedNode == null) 
			{ // Left- or Middle clicked on the empty canvas -> Start panning
				state.panWindow = true;
				state.dragStart = inputInfo.inputPos;
				state.dragOffset = Vector2.zero;
			}
		}

		[EventHandlerAttribute (EventType.MouseDrag)]
		private static void HandleWindowPanning (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.panWindow) 
			{ // Calculate change in panOffset
				Vector2 panOffsetChange = state.dragOffset;
				state.dragOffset = inputInfo.inputPos - state.dragStart;
				panOffsetChange = (state.dragOffset - panOffsetChange) * state.zoom;
				// Apply panOffsetChange to panOffset
				state.panOffset += panOffsetChange;
				NodeEditor.RepaintClients ();
			}
		}

		[EventHandlerAttribute (EventType.MouseDown)]
		[EventHandlerAttribute (EventType.MouseUp)]
		private static void HandleWindowPanningEnd (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.editorState.panWindow = false;
		}

		#endregion

		#region Connection

		[EventHandlerAttribute (EventType.MouseDown)]
		private static void HandleConnectionDrawing (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && state.focusedNodeKnob != null)
			{ // Left-Clicked on a NodeKnob, so check if any of them is a nodeInput or -Output
				if (state.focusedNodeKnob is NodeOutput)
				{ // Output clicked -> Draw new connection from it
					state.connectOutput = (NodeOutput)state.focusedNodeKnob;
					inputInfo.inputEvent.Use ();
				}
				else if (state.focusedNodeKnob is NodeInput)
				{ // Input clicked -> Loose and edit connection from it
					NodeInput clickedInput = (NodeInput)state.focusedNodeKnob;
					if (clickedInput.connection != null)
					{
						state.connectOutput = clickedInput.connection;
						clickedInput.RemoveConnection ();
						inputInfo.inputEvent.Use ();
					}
				}
			}
		}

		[EventHandlerAttribute (EventType.MouseUp)]
		private static void HandleApplyConnection (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (inputInfo.inputEvent.button == 0 && state.connectOutput != null && state.focusedNode != null && state.focusedNodeKnob != null && state.focusedNodeKnob is NodeInput) 
			{ // An input was clicked, it'll will now be connected
				NodeInput clickedInput = state.focusedNodeKnob as NodeInput;
				clickedInput.TryApplyConnection (state.connectOutput);
				inputInfo.inputEvent.Use ();
			}
			state.connectOutput = null;
		}

		#endregion

		#region Zoom

		[EventHandlerAttribute (EventType.ScrollWheel)]
		private static void HandleZooming (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.editorState.zoom = (float)Math.Round (Math.Min (4.0, Math.Max (0.2, inputInfo.editorState.zoom + inputInfo.inputEvent.delta.y / 15)), 2);
			NodeEditor.RepaintClients ();
		}

		#endregion

		#region Navigation

		[HotkeyAttribute (KeyCode.N, EventType.KeyDown)]
		private static void HandleStartNavigating (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.editorState.navigate = true;
		}

		[HotkeyAttribute (KeyCode.N, EventType.KeyUp)]
		private static void HandleEndNavigating (NodeEditorInputInfo inputInfo) 
		{
			inputInfo.editorState.navigate = false;
		}

		#endregion

		#region Node Snap

		[HotkeyAttribute (KeyCode.LeftControl, EventType.KeyDown, 60)] // 60 ensures it is checked after the dragging was performed before
		[HotkeyAttribute (KeyCode.LeftControl, EventType.KeyUp, 60)]
		private static void HandleNodeSnap (NodeEditorInputInfo inputInfo) 
		{
			NodeEditorState state = inputInfo.editorState;
			if (state.selectedNode != null)
			{ // Snap selected Node's position to multiples of 10
				Vector2 pos = state.selectedNode.rect.position;
				pos = new Vector2 (Mathf.RoundToInt (pos.x/10) * 10, Mathf.RoundToInt (pos.y/10) * 10);
				state.selectedNode.rect.position = pos;
				inputInfo.inputEvent.Use ();
			}
			NodeEditor.RepaintClients ();
		}

		#endregion

	}

}


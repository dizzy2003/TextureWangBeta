using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Assets.TextureWang.Scripts.Nodes;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework
{
    public abstract class Node : ScriptableObject
    {
        public Rect rect = new Rect();
        internal Vector2 contentOffset = Vector2.zero;
        [SerializeField] public List<NodeKnob> nodeKnobs = new List<NodeKnob>();

        // Calculation graph
        [SerializeField] public List<NodeInput> Inputs = new List<NodeInput>();
        [SerializeField] public List<NodeOutput> Outputs = new List<NodeOutput>();
        [HideInInspector] [NonSerialized] internal bool calculated = true;

        #region General

        /// <summary>
        /// Init the Node Base after the Node has been created. This includes adding to canvas, and to calculate for the first time
        /// </summary>
        protected virtual void InitBase()
        {
            NodeEditor.RecalculateFrom(this);
            if (!NodeEditor.curNodeCanvas.nodes.Contains(this))
                NodeEditor.curNodeCanvas.nodes.Add(this);
#if UNITY_EDITOR
            if (String.IsNullOrEmpty(name))
                name = UnityEditor.ObjectNames.NicifyVariableName(GetID);
#endif
            NodeEditor.RepaintClients();
        }

        /// <summary>
        /// Deletes this Node from curNodeCanvas and the save file
        /// </summary>
        public void Delete()
        {
            if (!NodeEditor.curNodeCanvas.nodes.Contains(this))
                throw new UnityException("The Node " + name + " does not exist on the Canvas " +
                                         NodeEditor.curNodeCanvas.name + "!");
            NodeEditorCallbacks.IssueOnDeleteNode(this);
            NodeEditor.curNodeCanvas.nodes.Remove(this);
            for (int outCnt = 0; outCnt < Outputs.Count; outCnt++)
            {
                NodeOutput output = Outputs[outCnt];
                while (output.connections.Count != 0)
                    output.connections[0].RemoveConnection();
                DestroyImmediate(output, true);
            }
            for (int inCnt = 0; inCnt < Inputs.Count; inCnt++)
            {
                NodeInput input = Inputs[inCnt];
                if (input.connection != null)
                    input.connection.connections.Remove(input);
                DestroyImmediate(input, true);
            }
            for (int knobCnt = 0; knobCnt < nodeKnobs.Count; knobCnt++)
            {
                // Inputs/Outputs need specific treatment, unfortunately
                if (nodeKnobs[knobCnt] != null)
                    DestroyImmediate(nodeKnobs[knobCnt], true);
            }
            DestroyImmediate(this, true);
        }

        /// <summary>
        /// Create the a Node of the type specified by the nodeID at position
        /// </summary>
        public static Node Create(string nodeID, Vector2 position)
        {
            return Create(nodeID, position, null);
        }
        public static Node Duplicate(Node _other, Vector2 position)
        {

            Node node = UnityEngine.Object.Instantiate(_other);
            node.rect.position+=new Vector2(10,10);
            node.InitBase();
            if (!NodeEditor.curNodeCanvas.nodes.Contains(node))
                NodeEditor.curNodeCanvas.nodes.Add(node);


            //NodeEditorCallbacks.IssueOnAddNode(node);

            return node;
        }

        public void CloneFieldsFrom(Node _old)
        {
            //        Dictionary<string, SharedVariable> m_SVLookUp=new Dictionary<string,SharedVariable>();
            Type oldType = _old.GetType();// typeof(PlayerBTTweaks);
            Type thisType = this.GetType();// typeof(PlayerBTTweaks);

            FieldInfo[] my_Fields = thisType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] oldFields = oldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in oldFields)
            {
                //TODO can probably automate this somewhat finding reference types
                if (m.MemberType == MemberTypes.Field && m.FieldType.FullName.Contains("AnimationCurve"))
                {
                    foreach (var n in my_Fields)
                    {
                        if (n.Name == m.Name)
                        {
                            AnimationCurve oldCurve = (AnimationCurve)m.GetValue(_old);
                            
                            n.SetValue(this, new AnimationCurve((Keyframe[])oldCurve.keys.Clone()));
                        }
                    }
                }
                else
                if (m.MemberType == MemberTypes.Field && m.FieldType.FullName.Contains("Gradient"))
                {
                    foreach (var n in my_Fields)
                    {
                        if (n.Name == m.Name)
                        {
                            Gradient oldGradient = (Gradient)m.GetValue(_old);
                            Gradient newGradient = new Gradient();
                            newGradient.SetKeys((GradientColorKey[])oldGradient.colorKeys.Clone(), (GradientAlphaKey[])oldGradient.alphaKeys);

                            n.SetValue(this, newGradient);
                        }
                    }
                }
                else
                if (m.MemberType == MemberTypes.Field && 
                    (m.FieldType.FullName.Contains("Boolean")||
                     m.FieldType.FullName.Contains("Int32")||
                     m.FieldType.FullName.Contains("Color") ||
                     m.FieldType.FullName.Contains("TexMode") ))
                {
                    foreach (var n in my_Fields)
                    {
                        if (n.Name == m.Name)
                        {
                            
                            n.SetValue(this, m.GetValue(_old));
                        }
                    }
                }
                else
                if (m.MemberType == MemberTypes.Field && m.FieldType.FullName.Contains("FloatRemap"))
                {
                    foreach (var n in my_Fields)
                    {
                        if (  n.Name == m.Name)// && !n.Name.StartsWith("Input") && !n.Name.StartsWith("Output") && !n.Name.StartsWith("nodeKnobs") && !n.Name.StartsWith("calculated") && !n.Name.StartsWith("rect")))
                        {
                            FloatRemap fr = (FloatRemap)m.GetValue(_old); //its a struct this makes a copy

                            //remove all replace input links                                          


                            if (fr.m_ReplaceWithInput && fr.m_Replacement != null)
                            {
                                //wont find itfr.m_Replacement = n.FindExistingInputByName(_label);
                                var oldReplacement = fr.m_Replacement;
                                    fr.m_Replacement = CreateInput(oldReplacement.name, "Float");

                            }
                            else
                            {
                                    fr.m_ReplaceWithInput=false;
                                    fr.m_Replacement =null;                                
                            }


                            n.SetValue(this, fr);
                        }
                    }
                }
            }



        }

        /// <summary>
        /// Create the a Node of the type specified by the nodeID at position
        /// Auto-connects the passed connectingOutput if not null to the first compatible input
        /// </summary>
        public static Node Create(string nodeID, Vector2 position, NodeOutput connectingOutput,bool _issueCallback=true)
        {
            Node node = NodeTypes.getDefaultNode(nodeID);
            if (node == null)
                throw new UnityException("Cannot create Node with id " + nodeID + " as no such Node type is registered!");

            
            node = node.Create(position);
            node.InitBase();

            if (connectingOutput != null)
            {
                // Handle auto-connection and link the output to the first compatible input
                foreach (NodeInput input in node.Inputs)
                {
                    if (input.TryApplyConnection(connectingOutput))
                        break;
                }
            }
            if(_issueCallback)
                NodeEditorCallbacks.IssueOnAddNode(node);

            return node;
        }

        public void FixNode()
        {
            float maxRange=6000;
            if (rect.x < -maxRange)
                rect.x = -maxRange;
            if (rect.x > maxRange)
                rect.x = maxRange;
            if (rect.y < -maxRange)
                rect.y = -maxRange;
            if (rect.y > maxRange)
                rect.y = maxRange;
        }
        protected static float ms_MinX;
        protected static float ms_MinY;
        protected static float ms_MaxX;
        protected static float ms_MaxY;
        /// <summary>
        /// Makes sure this Node has migrated from the previous save version of NodeKnobs to the current mixed and generic one
        /// </summary>
        internal void CheckNodeKnobMigration()
        {
            // TODO: Migration from previous NodeKnob system; Remove later on
            if (nodeKnobs.Count == 0 && (Inputs.Count != 0 || Outputs.Count != 0))
            {
                nodeKnobs.AddRange(Inputs.Cast<NodeKnob>());
                nodeKnobs.AddRange(Outputs.Cast<NodeKnob>());
            }
        }

        #endregion

        #region Dynamic Members

        #region Node Type Methods

        /// <summary>
        /// Get the ID of the Node
        /// </summary>
        public abstract string GetID { get; }

        /// <summary>
        /// Create an instance of this Node at the given position
        /// </summary>
        public abstract Node Create(Vector2 pos);

        /// <summary>
        /// Draw the Node immediately
        /// </summary>
        protected internal abstract void NodeGUI();

        /// <summary>
        /// Used to display a custom node property editor in the side window of the NodeEditorWindow
        /// Optionally override this to implement
        /// </summary>
        public virtual void DrawNodePropertyEditor()
        {
        }

        /// <summary>
        /// Calculate the outputs of this Node
        /// Return Success/Fail
        /// Might be dependant on previous nodes
        /// </summary>
        public virtual bool Calculate()
        {
            return true;
        }

        #endregion

        #region Node Type Properties

        /// <summary>
        /// Does this node allow recursion? Recursion is allowed if atleast a single Node in the loop allows for recursion
        /// </summary>
        public virtual bool AllowRecursion
        {
            get { return false; }
        }

        /// <summary>
        /// Should the following Nodes be calculated after finishing the Calculation function of this node?
        /// </summary>
        public virtual bool ContinueCalculation
        {
            get { return true; }
        }

        #endregion

        #region Protected Callbacks

        /// <summary>
        /// Callback when the node is deleted
        /// </summary>
        protected internal virtual void OnDelete()
        {
        }

        /// <summary>
        /// Callback when the NodeInput was assigned a new connection
        /// </summary>
        protected internal virtual void OnAddInputConnection(NodeInput input)
        {
        }

        /// <summary>
        /// Callback when the NodeOutput was assigned a new connection (the last in the list)
        /// </summary>
        protected internal virtual void OnAddOutputConnection(NodeOutput output)
        {
        }

        #endregion

        #region Additional Serialization

        /// <summary>
        /// Returns all additional ScriptableObjects this Node holds. 
        /// That means only the actual SOURCES, simple REFERENCES will not be returned
        /// This means all SciptableObjects returned here do not have it's source elsewhere
        /// </summary>
        public virtual ScriptableObject[] GetScriptableObjects()
        {
            return new ScriptableObject[0];
        }

        /// <summary>
        /// Replaces all REFERENCES aswell as SOURCES of any ScriptableObjects this Node holds with the cloned versions in the serialization process.
        /// </summary>
        protected internal virtual void CopyScriptableObjects(
            System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
        {
        }

        public void SerializeInputsAndOutputs(System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
        {
        }

        #endregion

        #endregion

        #region Drawing

#if UNITY_EDITOR
        public virtual void OnSceneGUI()
        {

        }
#endif
        

        protected virtual Color GetTitleBoxColor()
        {
            return Color.grey;
        }

        protected static Texture2D ms_SelectedBox;
        /// <summary>
        /// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
        /// </summary>
        protected internal virtual void DrawNode()
        {
            // TODO: Node Editor Feature: Custom Windowing System
            // Create a rect that is adjusted to the editor zoom
            Rect nodeRect = rect;
            nodeRect.position += NodeEditor.curEditorState.zoomPanAdjust + NodeEditor.curEditorState.panOffset;
            contentOffset = new Vector2(0, 20);
            var was = GUI.color;
            GUI.color = GetTitleBoxColor();
            bool selected = NodeEditor.curEditorState.selectedNode == this || NodeEditor.curEditorState.selectedNodes.Contains(this);

            Rect bodyRect = new Rect(nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width,
                nodeRect.height - contentOffset.y);
            Rect headerRect = new Rect(nodeRect.x, nodeRect.y, nodeRect.width, contentOffset.y);
            if (selected)
            {
                GUI.color = Color.white;
                //draw a large selection rect
                Rect bigger = bodyRect;
                bigger.x -= 3;
                bigger.y -= 3;
                bigger.y -= headerRect.height;
                bigger.size += new Vector2(6, 6 + headerRect.height);
                
                GUI.BeginGroup(bigger, GUI.skin.label);
                bigger.position = Vector2.zero;
                if (ms_SelectedBox == null)
                {
                    ms_SelectedBox = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    ms_SelectedBox.SetPixel(0, 0, Color.yellow);
                    ms_SelectedBox.wrapMode = TextureWrapMode.Repeat;
                    ms_SelectedBox.Apply();

                }
                GUI.DrawTexture(bigger, ms_SelectedBox, ScaleMode.StretchToFill); //ScaleMode.StretchToFill);

                GUI.EndGroup();
                
            }
            NodeEditorGUI.nodeBox.normal.textColor = Color.white;
            // Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold
            
            GUI.Label(headerRect, name,NodeEditorGUI.nodeBox);
            GUI.color = was;
            // Begin the body frame around the NodeGUI

           
            GUI.BeginGroup(bodyRect, GUI.skin.box);

            bodyRect.position = Vector2.zero;
            GUILayout.BeginArea(bodyRect, GUI.skin.box);
            // Call NodeGUI
            GUI.changed = false;
            NodeGUI();
            // End NodeGUI frame
            GUILayout.EndArea();
            GUI.EndGroup();
        }

        /// <summary>
        /// Draws the nodeKnobs
        /// </summary>
        protected internal virtual void DrawKnobs()
        {
            CheckNodeKnobMigration();
            for (int knobCnt = 0; knobCnt < nodeKnobs.Count; knobCnt++)
                nodeKnobs[knobCnt].DrawKnob();
        }

        /// <summary>
        /// Draws the node curves
        /// </summary>
        protected internal virtual void DrawConnections()
        {
            CheckNodeKnobMigration();
            if (Event.current.type != EventType.Repaint)
                return;
            for (int outCnt = 0; outCnt < Outputs.Count; outCnt++)
            {
                NodeOutput output = Outputs[outCnt];
                Vector2 startPos = output.GetGUIKnob().center;
                Vector2 startDir = output.GetDirection();

                for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
                {
                    NodeInput input = output.connections[conCnt];
                    NodeEditorGUI.DrawConnection(startPos,
                        startDir,
                        input.GetGUIKnob().center,
                        input.GetDirection(),
                        output.typeData.Color);
                }
            }
        }

        #endregion

        #region Calculation Utility

        /// <summary>
        /// Checks if there are no unassigned and no null-value inputs.
        /// </summary>
        protected internal bool allInputsReady()
        {
            for (int inCnt = 0; inCnt < Inputs.Count; inCnt++)
            {
                if (Inputs[inCnt]!=null && (Inputs[inCnt].connection == null || Inputs[inCnt].connection.IsValueNull))//||!Inputs[inCnt].connection.body.calculated)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if there are any unassigned inputs.
        /// </summary>
        protected internal bool hasUnassignedInputs()
        {
            for (int inCnt = 0; inCnt < Inputs.Count; inCnt++)
                if (Inputs[inCnt].connection == null)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns whether every direct dexcendant has been calculated
        /// </summary>
        protected internal bool descendantsCalculated()
        {
            for (int cnt = 0; cnt < Inputs.Count; cnt++)
            {
                if (Inputs[cnt]!=null && Inputs[cnt].connection != null && !Inputs[cnt].connection.body.calculated)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns whether the node acts as an input (no inputs or no inputs assigned)
        /// </summary>
        protected internal bool isInput()
        {
            for (int cnt = 0; cnt < Inputs.Count; cnt++)
            {
                if(Inputs[cnt] == null)
                    Debug.LogError(" node "+this+" has null input ");
                if (Inputs[cnt]!=null && Inputs[cnt].connection != null)
                    return false;
            }
            return true;
        }
        //Does this node output to anything else
        protected internal bool noOutput()
        {
            for (int cnt = 0; cnt < Outputs.Count; cnt++)
            {
                if (Outputs[cnt] != null)
                {
                    foreach (var x in Outputs[cnt].connections)
                    {
                        if (x != null && x.connection != null)
                            return false;
                    }
                }
                
            }
            return true;
        }

        #endregion

        #region Knob Utility

        // -- OUTPUTS --

        /// <summary>
        /// Creates and output on your Node of the given type.
        /// </summary>
        public NodeOutput CreateOutput(string outputName, string outputType)
        {
            return NodeOutput.Create(this, outputName, outputType);
        }

        /// <summary>
        /// Creates and output on this Node of the given type at the specified NodeSide.
        /// </summary>
        public NodeOutput CreateOutput(string outputName, string outputType, NodeSide nodeSide)
        {
            return NodeOutput.Create(this, outputName, outputType, nodeSide);
        }

        /// <summary>
        /// Creates and output on this Node of the given type at the specified NodeSide and position.
        /// </summary>
        public NodeOutput CreateOutput(string outputName, string outputType, NodeSide nodeSide, float sidePosition)
        {
            return NodeOutput.Create(this, outputName, outputType, nodeSide, sidePosition);
        }

        /// <summary>
        /// Aligns the OutputKnob on it's NodeSide with the last GUILayout control drawn.
        /// </summary>
        /// <param name="outputIdx">The index of the output in the Node's Outputs list</param>
        protected void OutputKnob(int outputIdx)
        {
            if (Event.current.type == EventType.Repaint)
                Outputs[outputIdx].SetPosition();
        }

        void RePositionInputKnobs()
        {
            float step = 130.0f / Inputs.Count;
            float pos = 30;
            foreach (var x in Inputs)
            {
                x.sidePosition = pos;
                pos += step;
            }
        }
    

    // -- INPUTS --

		/// <summary>
		/// Creates and input on your Node of the given type.
		/// </summary>
		public NodeInput CreateInput (string inputName, string inputType)
		{
			var ret= NodeInput.Create (this, inputName, inputType);
		    RePositionInputKnobs();
            return ret;
		}
		/// <summary>
		/// Creates and input on this Node of the given type at the specified NodeSide.
		/// </summary>
		public NodeInput CreateInput (string inputName, string inputType, NodeSide nodeSide)
		{
            var ret = NodeInput.Create (this, inputName, inputType, nodeSide);
            RePositionInputKnobs();
            return ret;
        }
		/// <summary>
		/// Creates and input on this Node of the given type at the specified NodeSide and position.
		/// </summary>
		public NodeInput CreateInput (string inputName, string inputType, NodeSide nodeSide, float sidePosition)
		{
            var ret = NodeInput.Create (this, inputName, inputType, nodeSide, sidePosition);
            RePositionInputKnobs();
            return ret;
        }

		/// <summary>
		/// Aligns the InputKnob on it's NodeSide with the last GUILayout control drawn.
		/// </summary>
		/// <param name="inputIdx">The index of the input in the Node's Inputs list</param>
		protected void InputKnob (int inputIdx)
		{
			if (Event.current.type == EventType.Repaint)
				Inputs[inputIdx].SetPosition ();
		}

		/// <summary>
		/// Reassigns the type of the given output. This actually recreates it
		/// </summary>
		protected static void ReassignOutputType (ref NodeOutput output, Type newOutputType) 
		{
			Node body = output.body;
			string outputName = output.name;
			// Store all valid connections that are not affected by the type change
			IEnumerable<NodeInput> validConnections = output.connections.Where ((NodeInput connection) => connection.typeData.Type.IsAssignableFrom (newOutputType));
			// Delete the output of the old type
			output.Delete ();
			// Create Output with new type
			NodeEditorCallbacks.IssueOnAddNodeKnob (NodeOutput.Create (body, outputName, newOutputType.AssemblyQualifiedName));
			output = body.Outputs[body.Outputs.Count-1];
			// Restore the valid connections
			foreach (NodeInput input in validConnections)
				input.ApplyConnection (output);
		}

		/// <summary>
		/// Reassigns the type of the given output. This actually recreates it
		/// </summary>
		protected static void ReassignInputType (ref NodeInput input, Type newInputType) 
		{
			Node body = input.body;
			string inputName = input.name;
			// Store the valid connection if it's not affected by the type change
			NodeOutput validConnection = null;
			if (input.connection != null && newInputType.IsAssignableFrom (input.connection.typeData.Type))
				validConnection = input.connection;
			// Delete the input of the old type
			input.Delete ();
			// Create Output with new type
			NodeEditorCallbacks.IssueOnAddNodeKnob (NodeInput.Create (body, inputName, newInputType.AssemblyQualifiedName));
			input = body.Inputs[body.Inputs.Count-1];
			// Restore the valid connections
			if (validConnection != null)
				input.ApplyConnection (validConnection);
		}

		#endregion

		#region Node Utility

		/// <summary>
		/// Recursively checks whether this node is a child of the other node
		/// </summary>
		public bool isChildOf (Node otherNode)
		{
			if (otherNode == null || otherNode == this)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			for (int cnt = 0; cnt < Inputs.Count; cnt++) 
			{
				NodeOutput connection = Inputs [cnt].connection;
				if (connection != null) 
				{
					if (connection.body != startRecursiveSearchNode)
					{
						if (connection.body == otherNode || connection.body.isChildOf (otherNode))
						{
							StopRecursiveSearchLoop ();
							return true;
						}
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether this node is in a loop
		/// </summary>
		internal bool isInLoop ()
		{
			if (BeginRecursiveSearchLoop ()) return this == startRecursiveSearchNode;
			for (int cnt = 0; cnt < Inputs.Count; cnt++) 
			{
				NodeOutput connection = Inputs [cnt].connection;
				if (connection != null && connection.body.isInLoop ()) 
				{
					StopRecursiveSearchLoop ();
					return true;
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether any node in the loop to be made allows recursion.
		/// Other node is the node this node needs connect to in order to fill the loop (other node being the node coming AFTER this node).
		/// That means isChildOf has to be confirmed before calling this!
		/// </summary>
		internal bool allowsLoopRecursion (Node otherNode)
		{
			if (AllowRecursion)
				return true;
			if (otherNode == null)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			for (int cnt = 0; cnt < Inputs.Count; cnt++) 
			{
				NodeOutput connection = Inputs [cnt].connection;
				if (connection != null && connection.body.allowsLoopRecursion (otherNode)) 
				{
					StopRecursiveSearchLoop ();
					return true;
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// A recursive function to clear all calculations depending on this node.
		/// Usually does not need to be called manually
		/// </summary>
		public void ClearCalculation () 
		{
			if (BeginRecursiveSearchLoop ()) return;
//            Debug.Log("CLEAR CALC "+this);
			calculated = false;
		    OnClearCalculation();
            for (int outCnt = 0; outCnt < Outputs.Count; outCnt++)
			{
				NodeOutput output = Outputs [outCnt];
				for (int conCnt = 0; conCnt < output.connections.Count; conCnt++)
					output.connections [conCnt].body.ClearCalculation ();
			}
			EndRecursiveSearchLoop ();
		}

		#region Recursive Search Helpers

		[NonSerialized] private List<Node> recursiveSearchSurpassed;
		[NonSerialized] private Node startRecursiveSearchNode; // Temporary start node for recursive searches

		/// <summary>
		/// Begins the recursive search loop and returns whether this node has already been searched
		/// </summary>
		internal bool BeginRecursiveSearchLoop ()
		{
			if (startRecursiveSearchNode == null || recursiveSearchSurpassed == null) 
			{ // Start search
				recursiveSearchSurpassed = new List<Node> ();
				startRecursiveSearchNode = this;
			}

			if (recursiveSearchSurpassed.Contains (this))
				return true;
			recursiveSearchSurpassed.Add (this);
			return false;
		}

		/// <summary>
		/// Ends the recursive search loop if this was the start node
		/// </summary>
		internal void EndRecursiveSearchLoop () 
		{
			if (startRecursiveSearchNode == this) 
			{ // End search
				recursiveSearchSurpassed = null;
				startRecursiveSearchNode = null;
			}
		}

		/// <summary>
		/// Stops the recursive search loop immediately. Call when you found what you needed.
		/// </summary>
		internal void StopRecursiveSearchLoop () 
		{
			recursiveSearchSurpassed = null;
			startRecursiveSearchNode = null;
		}

        #endregion

        #endregion

        public void RemoveInput(NodeInput _in)
        {
            if (_in.connection != null)
                _in.connection.connections.Remove(_in);

            Inputs.Remove(_in);
            nodeKnobs.Remove(_in);
//            DestroyImmediate(_in);
        }
        public int m_DirtyID;
        public static int ms_GlobalDirtyID;

        public virtual void OnClearCalculation()
	    {

        }

        public NodeInput FindExistingInputByName(string inputName)
        {
            foreach (var x in Inputs)
            {
                if (x!=null && x.name == inputName)
                {
                    Debug.Log("create input that exists use existing " + inputName );
                    return x;
                }
            }
            return null;
        }
    }
}

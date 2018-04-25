using System.Collections.Generic;
using System.Reflection;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node (false, "SubTreeNode")]
    public class SubTreeNode : TextureNode
    {
    
        public const string ID = "SubTreeNode";
        public override string GetID { get { return ID; } }

        public bool m_UseAllOutputs;

        public NodeCanvas SubCanvas
        {
            get { return m_SubCanvas; }
            set { m_SubCanvas = value; }
        }

//    [SerializeField]
        private NodeCanvas m_SubCanvas;
        public string m_CanvasGuid;
        private bool m_WasCloned;

        protected internal override void InspectorNodeGUI()
        {
        
        }

        public override Node Create (Vector2 _pos) 
        {

            SubTreeNode node = CreateInstance<SubTreeNode> ();
        
            node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "SubTreeNode";
            node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

            return node;
        }

        private void OnGUI()
        {
            NodeGUI();
        }

        public override void DrawNodePropertyEditor() 
        {
            //GUI.changed = false;
    
            // m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
#if UNITY_EDITOR
/*
        if (m_SubCanvas != null)
        {

            string assetPath = AssetDatabase.GetAssetPath(m_SubCanvas);
            Debug.Log(" canvas path >"+assetPath+"<");
            if (assetPath.Length > 0)
            {
                m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
                Debug.LogError(" set canvasGuid from asset >" + m_CanvasGuid+"<");
            }


        }
*/
            if (m_CanvasGuid != null)
            {
                m_CanvasGuid = GUILayout.TextField(m_CanvasGuid);
            }
            else
            {
                m_CanvasGuid = GUILayout.TextField("");
                m_CanvasGuid = "";

            }

            //Debug.LogError(" set canvasGuid textfiled " + m_CanvasGuid);
#endif
            //        m_SubCanvas = (NodeCanvas)EditorGUI.ObjectField(new Rect(0, 250, 250, 50), m_SubCanvas, typeof(NodeCanvas), false);


        }
        protected override Color GetTitleBoxColor()
        {
            return Color.cyan;
        }

        string GetNodeOutputName(TextureNode n)
        {
            string name = "Out:";
            if (n is UnityTextureOutputMetalicAndRoughness)
                name = (n as UnityTextureOutputMetalicAndRoughness).m_TexName;
            if (n is UnityTextureOutput)
                name = (n as UnityTextureOutput).m_TexName;
            return name;
        }
        bool FixupForSubCanvas()
        {
            if (!string.IsNullOrEmpty(m_CanvasGuid) && m_SubCanvas == null)
            {

                string nodeCanvasPath = AssetDatabase.GUIDToAssetPath(m_CanvasGuid);

                m_SubCanvas = NodeEditorSaveManager.LoadNodeCanvas(nodeCanvasPath,false); 
                m_WasCloned = false;

            }

            if (m_SubCanvas != null)
            {
                if (!m_WasCloned)
                {
                    m_SubCanvas = NodeEditorSaveManager.CreateWorkingCopy(m_SubCanvas, false); //miked remove ref
                    m_WasCloned = true;
/* test its making unique nodes
                    foreach (Node n in m_SubCanvas.nodes)
                    {
                        if (n is TextureNode)
                        {
                            var tnIn = n as TextureNode;
                            var was = tnIn.m_TexHeight;
                            tnIn.m_TexHeight = Random.Range(1000, 1500);
                            Debug.Log("Change sub routines node" + tnIn + "  tex height to  " + tnIn.m_TexHeight + " was " + was);

                        }
                    }
*/
                }

                List<NodeInput> needsInput = new List<NodeInput>();
                List<TextureNode> needsOutput = new List<TextureNode>();
                foreach (Node n in m_SubCanvas.nodes)
                {

                    if (n.Inputs.Count > 0)
                    {
                        if (n is UnityTextureOutput && n.Inputs[0].connection != null)
                        {
                            needsOutput.Add(n as TextureNode);

                        }
                        if (n is UnityTextureOutputMetalicAndRoughness && n.Inputs[0].connection != null)
                        {
                            needsOutput.Add(n as TextureNode);

                        }
                        for (int i = 0; i < n.Inputs.Count; i++)
                        {
                            if (n.Inputs[i].connection == null)
                            {
                                //this node has no input so we will wire it up to ours
                                needsInput.Add(n.Inputs[i]);
                                //                            Debug.Log(" missing input for node "+n+" name "+n.name);
                            }
                        }
                    }
                }
                if (needsOutput.Count > Outputs.Count)
                {
                
                    while (needsOutput.Count > Outputs.Count)
                    {
                        //                    Debug.Log(" create input "+Inputs.Count);

                        string nname=GetNodeOutputName(needsOutput[Outputs.Count]);
                        
                        CreateOutput("Texture"+  Outputs.Count+" "+ nname, needsOutput[Outputs.Count ].Inputs[0].connection.typeID, NodeSide.Right, 50 + Outputs.Count * 20);
                    }
                }
                if(needsOutput.Count>0)
                    Outputs[0].name = "Texture0" + " " + GetNodeOutputName(needsOutput[0]);

                if (needsInput.Count > Inputs.Count)
                {
                    int added = 0;

                

                    for (int index = Inputs.Count ; index < needsInput.Count; index++)
                    {
                        string needInputname = needsInput[index].name;
                        //                    Debug.Log(" create input "+Inputs.Count);
                        NodeInput newInput=CreateInput(needInputname, needsInput[index].typeID, NodeSide.Left, 30 + Inputs.Count*20);
                        if (newInput.typeID == "Float")
                        {
                            var n = Node.Create("inputNode", rect.position - new Vector2(100, 50 - added * 60));
                            added++;
                            InputNode inode = n as InputNode;
                            if (inode != null)
                            {

                                newInput.ApplyConnection(inode.Outputs[0], false);
                                InputNode ip = (InputNode)n;
                                Node nodeThatNeedsInput = needsInput[index].body;
                                //Use reflection to find the float remap member var that matches the input
                                FieldInfo[] myField = nodeThatNeedsInput.GetType().GetFields();
                                foreach (var x in myField)
                                {
                                    // if(x.FieldType is FloatRemap)
                                    if (x.GetValue(nodeThatNeedsInput) is FloatRemap)
                                    {
                                        FloatRemap fr = (FloatRemap)x.GetValue(nodeThatNeedsInput); //its a struct this makes a copy
                                        //                Debug.Log(x + " " + x.FieldType + " " + x.GetType() + " " + x.ReflectedType+" fr val:"+fr.m_Value);
                                        if (fr.m_ReplaceWithInput && fr.m_Replacement == null)
                                        {
                                            Debug.LogError(" wants to be replaced but isnt linked ");
                                        }
                                        else if (fr.m_Replacement != null)
                                        {
                                            NodeKnob knob = fr.m_Replacement;
                                            NodeInput replace = knob as NodeInput;
                                            if (replace != null)
                                            {
                                                if (replace == needsInput[index])
                                                {
                                                    ip.m_Value.Set(fr.m_Value);
                                                    ip.m_Value.m_Min = fr.m_Min;
                                                    ip.m_Value.m_Max = fr.m_Max;
                                                }
                                            }

                                        }
                                    }
                                }
                            }

                        }
                    }
                    m_InputsCreated = true;
                    //CreateNewFloatInputs();
                    return false;
                }

            }
            return true;
        }

        public override void OnLoadCanvas()
        {
            base.OnLoadCanvas();
            FixupForSubCanvas();
        }

        public void PostOnLoadCanvasFixup()
        {
            CreateNewFloatInputs();
        }

        private bool m_InputsCreated;
        public void CreateNewFloatInputs()
        {
            if (!m_InputsCreated)
            {
/*            
            int added = 0;
            foreach (var i in Inputs)
            {
                m_InputsCreated = true;
                if (i.typeID == "Float")
                {
                    var n = Node.Create("inputNode", this.rect.position - new Vector2(100, 50 - added*60));
                    added++;
                    InputNode inode = n as InputNode;
                    if (inode != null)
                    {

                        i.ApplyConnection(inode.Outputs[0], false);
                        //InputNode ip = (InputNode) n;
                        
                    }

                }
            }
*/
            }
        }

        protected internal override void DrawConnections()
        {
            base.DrawConnections();

            if (Event.current.type != EventType.Repaint)
                return;
            foreach (NodeOutput output in Outputs)
            {
                if (output == null)
                    continue;
                Vector2 startPos = output.GetGUIKnob().center;
            

                EditorGUI.LabelField(new Rect(startPos - new Vector2(0, 20), new Vector2(200, 50)), output.name);

            }
        }



        public override bool Calculate()
        {
        

            if (!allInputsReady())
            {
                //Debug.LogError(" input no ready");
                return false;
            }
            if (!FixupForSubCanvas())
                return false;
            if (Outputs[0] != null && Outputs[0].connections.Count > 0 && Outputs[0].connections[0].body is TextureNode)
            {
                var tnIn = Outputs[0].connections[0].body as TextureNode;
                var was = tnIn.m_TexHeight;
                tnIn.m_TexHeight = Random.Range(1000, 1500);
                Debug.Log("Cahnge sub routines "+tnIn+" last output nodes tex height to  " + tnIn.m_TexHeight+" was "+was);

            }

            if (m_SubCanvas != null)
            {
            
                List<Node> workList = new List<Node>();

                List<NodeInput> needsRemoval = new List<NodeInput>();
                //connect each of our inputs to the internal inputs of the sub canvas
                int count = 0;
                foreach (Node n in m_SubCanvas.nodes)
                {
                    n.calculated = false;
                    if (n.Inputs.Count > 0)
                    {
                        for (int i = 0; i < n.Inputs.Count; i++)
                        {
                            if (n.Inputs[i].connection == null)
                            {
                                //this node has no input so we will wire it up to ours
//                            Debug.Log(" connect input " + Inputs.Count+" count "+count);
                                if (Inputs.Count > count&& Inputs[count])
                                {
                                    workList.Add(n);
                                    n.calculated = false;
                                    n.Inputs[i].ApplyConnection(Inputs[count].connection,false);
                                    needsRemoval.Add(n.Inputs[i]);
                                }
                                count++;
                            }
                        }
                    }
                }


                NodeEditor.RecalculateAllAndWorkList(m_SubCanvas,workList);
                int countOut = 0;
                bool hasUnityOutputNodes = false;
                foreach (Node n in m_SubCanvas.nodes)
                {
                    if (n is UnityTextureOutput || n is UnityTextureOutputMetalicAndRoughness)
                    {
                        hasUnityOutputNodes = true;
                        break;
                    }
                }

                foreach (Node n in m_SubCanvas.nodes)
                {
                    if (n is UnityTextureOutput || n is UnityTextureOutputMetalicAndRoughness)
                    {
                        m_Param = n.Inputs[0].GetValue<TextureParam>();
                        Outputs[countOut++].SetValue(m_Param);
                    }
                    else
                        if ((!hasUnityOutputNodes )&& n.Outputs.Count>0 && n.Outputs[0].connections.Count == 0)
                        {
                            //this node has no output so it must be the final destination
                            m_Param = n.Outputs[0].GetValue<TextureParam>();
                            Outputs[countOut++].SetValue(m_Param);
                        }
                    if (countOut >= Outputs.Count)
                        break;

                }

                foreach (var x in needsRemoval)
                {
                    x.RemoveConnection(false);
                }

                CreateCachedTextureIcon();
                //m_Cached = m_Param.GetHWSourceTexture();

                CheckDiskIcon(m_SubCanvas.name,m_Param);
            }

            return true;
        }

    }
    
}
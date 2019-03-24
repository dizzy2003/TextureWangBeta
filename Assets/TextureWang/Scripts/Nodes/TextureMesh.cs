using System;
using System.Collections.Generic;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/MeshEdges")]
    public  class TextureMesh : TextureNode
    {
        private const string m_Help = "Draw an N sided polygon";
        public override string GetHelp() { return m_Help; }
        //    public enum TexOP { Add, ClipMin, Multiply, Power,Blur,CalcNormal,Level1,Transform,Gradient,Min,Max }

        public Mesh m_Mesh;
        //public Texture m_Cached;
        public FloatRemap m_MaterialIndex;//where all 0.5
        public FloatRemap m_EdgeAngleDif;//where all 0.5


        public const string ID = "TextureMesh";
        public override string GetID { get { return ID; } }

        public override Node Create(Vector2 pos)
        {

            TextureMesh node = CreateInstance<TextureMesh>();

            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Mesh";
            node.CreateInputOutputs();
            node.m_MaterialIndex = new FloatRemap(1, 0, 10);
            node.m_EdgeAngleDif = new FloatRemap(0.7f, 0, 1);

            return node;
        }

        protected internal override void InspectorNodeGUI()
        {
        
        }
        protected override Color GetTitleBoxColor()
        {
            return Color.green;
        }

        protected override void CreateInputOutputs()
        {
            CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
        }


        private void OnGUI()
        {
            NodeGUI();
        }

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Mesh = (Mesh)EditorGUILayout.ObjectField(m_Mesh, typeof(Mesh), false, GUILayout.MinWidth(200), GUILayout.MinHeight(50));

            m_MaterialIndex.SliderLabelInt(this, "Seed");
            m_EdgeAngleDif.SliderLabel(this, "Edge Angle Limit");
        }

        double GetEdgeKey(Vector3 a, Vector3 b)
        {
            double offset = 0.3654326789532567;
            double ret = a.x+offset;
            ret*= (a.y + offset);
            ret *= (a.z + offset);
            ret *= (b.x + offset);
            ret *= (b.y + offset);
            ret *= (b.z + offset);
            return ret;

        }
        double GetEdgeKey(int a, int b)
        {
            
            double ret  = (a+1) + (b+1)*1000000;
                   ret += (b+1) + (a+1)*1000000;

            return ret;

        }

        public void Generate( TextureParam _output)
        {
            int seed = (int) m_MaterialIndex;
            
            Random.InitState(seed);
            RenderTexture destination = CreateRenderDestination(null, _output);

            TriangleDraw.GPUStart(destination,GL.LINES);
            List<Vector2> uvs=new List<Vector2>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector3> pos = new List<Vector3>();
            if ((int)m_MaterialIndex.m_Value >= m_Mesh.subMeshCount)
                m_MaterialIndex.Set(m_Mesh.subMeshCount - 1);
            int[] indicies = m_Mesh.GetIndices((int)m_MaterialIndex.m_Value);

            m_Mesh.GetUVs(0,uvs);
            m_Mesh.GetNormals( norms);
            m_Mesh.GetVertices(pos);
            Vector3[] averageTri=new Vector3[indicies.Length/3];
            Dictionary<double,Int64> edgeToTri=new Dictionary<double, long>();
            Dictionary<Int64, int> TriTriToEdge = new Dictionary<Int64, int>();
            for (int i = 0; i < indicies.Length; i += 3)
            {
                int i0 = indicies[i];
                int i1 = indicies[i+1];
                int i2 = indicies[i+2];
                Vector3 n0 = norms[i0];
                Vector3 n1 = norms[i1];
                Vector3 n2 = norms[i2];

                Vector3 p0 = pos[i0];
                Vector3 p1 = pos[i1];
                Vector3 p2 = pos[i2];

                Vector3 average = (n0 + n1 + n2)*.3333333333f;

                averageTri[i/3] = average;
                
                double key = GetEdgeKey(i0, i1);
                long triIndex = (i/3)+1;
                long existingTri=0;
                edgeToTri.TryGetValue(key, out existingTri);
                Debug.Log(" key "+key+" ia "+i0+" ib "+i1+" tri: "+(i/3)+"existing from dict "+ existingTri);

                if (edgeToTri.ContainsKey(key))
                {
                    Int64 tris = edgeToTri[key];
                    if (tris <( 1 << 30))
                    {//allready done
                        tris |= triIndex << 32;

                        edgeToTri[key] = tris;
                        TriTriToEdge[tris] = 0;
                    }
                    else
                    {
                        Debug.LogError("e0 third attempt at tri "+(i/3));
                    }
                }
                else
                {
                    edgeToTri[key] = triIndex;
                    TriTriToEdge[triIndex] = 0;
                }

                key = GetEdgeKey(i1, i2);
                edgeToTri.TryGetValue(key, out existingTri);

                Debug.Log(" key " + key + " ia " + i1 + " ib " + i2 + " tri: " + (i / 3) + "existing from dict " + existingTri);
                if (edgeToTri.ContainsKey(key))
                {
                    Int64 tris = edgeToTri[key];
                    if (tris < (1 << 30))
                    {//allready done
                        tris |= triIndex << 32;

                        edgeToTri[key] = tris;
                        TriTriToEdge[tris] = 1;
                    }
                    else
                    {
                        Debug.LogError("e1 third attempt at tri " + (i / 3));
                    }

                }
                else
                {
                    edgeToTri[key] = triIndex;
                    TriTriToEdge[triIndex] = 1;
                }
                key = GetEdgeKey(i2, i0);
                edgeToTri.TryGetValue(key, out existingTri);

                Debug.Log(" key " + key + " ia " + i2 + " ib " + i0 + " tri: " + (i / 3) + "existing from dict " + existingTri);
                if (edgeToTri.ContainsKey(key))
                {
                    Int64 tris = edgeToTri[key];
                    if (tris < (1 << 30))
                    {//allready done
                        tris |= triIndex << 32;

                        edgeToTri[key] = tris;
                        TriTriToEdge[tris] = 2;
                    }
                    else
                    {
                        Debug.LogError("e2 third attempt at tri " + (i / 3));
                    }

                }
                else
                {
                    edgeToTri[key] = triIndex;
                    TriTriToEdge[triIndex] = 2;
                }
            }


            TriangleDraw.AddCol(Color.white);
            foreach (double key in edgeToTri.Keys)
            {
                long index = edgeToTri[key];
                int left = (int)(index & 0xfffffff)-1;
                int right = (int)((index>>32) & 0xfffffffff)-1;
                bool drawEdge=false;
                if ( right < 0)
                {
                    drawEdge = false;
                    int offset = TriTriToEdge[index];
                    int v0 = left * 3 + offset;
                    int v1 = left * 3 + ((offset + 1) % 3);
                    Vector3 uv0 = uvs[indicies[v0]];
                    Vector3 uv1 = uvs[indicies[v1]];
                    TriangleDraw.AddVert(new Vector3(uv0.x, uv0.y, 0));
                    TriangleDraw.AddVert(new Vector3(uv1.x, uv1.y, 0));

                }
                else
                {
                    float dot = Vector3.Dot(averageTri[left], averageTri[right]);
                    if (dot < m_EdgeAngleDif && right >= 0)
                        drawEdge = true;

                }
                if(drawEdge)
                {

                    int offset = TriTriToEdge[index];
                    int v0 = right*3 + offset;
                    int v1 = right * 3 + (offset+1)%3;
                    Vector3 uv0 = uvs[indicies[v0]];
                    Vector3 uv1 = uvs[indicies[v1]];
                    TriangleDraw.AddVert(new Vector3(uv0.x, uv0.y, 0));
                    TriangleDraw.AddVert(new Vector3(uv1.x, uv1.y, 0));

                }
            }
/*
            for (int i = 0; i < indicies.Length; i +=3)
            {
                Vector3 uv0 = uvs[indicies[i]];
                Vector3 uv1 = uvs[indicies[i+1]];
                Vector3 uv2 = uvs[indicies[i+2]];
                TriangleDraw.AddVert(new Vector3(uv0.x,uv0.y,0));
                TriangleDraw.AddVert(new Vector3(uv1.x, uv1.y, 0));

                TriangleDraw.AddVert(new Vector3(uv1.x, uv1.y, 0));
                TriangleDraw.AddVert(new Vector3(uv2.x, uv2.y, 0));

                TriangleDraw.AddVert(new Vector3(uv2.x, uv2.y, 0));
                TriangleDraw.AddVert(new Vector3(uv0.x, uv0.y, 0));
            }
            */
            TriangleDraw.GPUEnd();

        }
   
 
        public override bool Calculate()
        {
            if (m_Mesh == null)
                return false;
            if (m_Param == null)
                m_Param = new TextureParam(m_TexWidth,m_TexHeight);
            Generate(  m_Param);
            //Debug.Log("execute " + this);
            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            Outputs[0].SetValue<TextureParam> (m_Param);
            return true;
        }
    }
}
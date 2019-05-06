using System.Collections.Generic;
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    abstract public class MeshBase : Node
    {
        public RenderTexture m_CacheIcon;

        protected internal override void NodeGUI()
        {

            if (m_CacheIcon != null)
            {

                if (Event.current.type == EventType.Repaint)
                {

                    GUI.DrawTexture(new Rect(2, 3, 200 - 4, 200 - 14), m_CacheIcon, ScaleMode.StretchToFill);
                }
            }

        }





        abstract protected void GenVertsFromData(List<Vector3> _vertsP, List<Vector3> _vertsN, List<Color32> _vertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV);

        Vector2 ToUV(Vector3 _pos,float _radius)
        {
            Vector3 ret = _pos;
            ret.y = _pos.z;
            ret.y /= 2.0f * _radius * Mathf.Sin(60 / 180.0f * Mathf.PI);
            ret.x /= 2.0f * _radius;
            ret.y += 0.5f;
            ret.x += 0.5f;

            return ret;
        }

        protected internal override void DrawKnobs()
        {
            base.DrawKnobs();
        }

        protected void CreateTube(List<Vector3> _vertsP, List<Vector3> _vertsN, List<Color32> _vertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV, Vector3 start, float r1, float len, float segs, float vsegs,
            float bumpExtra, float bumpFreq, Vector3 up, Vector3 right, bool endCap1, bool EndCap2, int bone)
        {

            //make a tube
            float step = 360.0f / segs;
            float stepalong = 1.0f / (vsegs - 1.0f);

            float radius = r1;
            float length = len;

            Vector3 upwards = up * length;
            int row = 0;
            List<Vector3> row1 = new List<Vector3>();
            List<Vector3> row2 = new List<Vector3>();
            List<Vector2> uvrow1 = new List<Vector2>();
            List<Vector2> uvrow2 = new List<Vector2>();
            List<Vector3> temp = null;
            List<Vector2> tempUV = null;
            int around = 0;
            for (float along = 0.0f; along <= 1.0001f; along += stepalong, row++)
            {
                //float bumpy = 0.0f;//Mathf.Abs(Mathf.Cos(along* bumpFreq*Mathf.PI));
                Vector3 sideways = right * (radius); //+bumpy*bumpExtra);
                around = 0;
                row1.Clear();
                uvrow1.Clear();
                for (float angle = 0.0f; angle < 360.0f; angle += step, around++)
                {
                    Quaternion axisAngle1 = Quaternion.AngleAxis(angle, Vector3.up);
                    Vector3 v = start + axisAngle1 * sideways + upwards * along;

                    row1.Add(v);
                    uvrow1.Add(ToUV(axisAngle1 * sideways,r1));
                }
                if (along > 0.0f)
                {
                    around = 0;
                    for (float angle = 0.0f; angle < 360.0f; angle += step, around++)
                    {
                        Vector3 bl = row2[around];
                        Vector3 br = row2[(around + 1) % row2.Count];
                        Vector3 tl = row1[around];
                        Vector3 tr = row1[(around + 1) % row1.Count];
                        Vector2 blUV = uvrow2[around];
                        Vector2 brUV = uvrow2[(around + 1) % row2.Count];
                        Vector2 tlUV = uvrow1[around];
                        Vector2 trUV = uvrow1[(around + 1) % row1.Count];
                        AddQuadComputeNorm(_vertsP, _vertsN, _vertsC, _bw, _vertsUV, tl, tr, br, bl, tlUV, trUV, brUV,
                            blUV,
                            Color.white, row); //bone);
                    }

                }
                else if (endCap1)
                {
                    around = 0;
                    for (float angle = 0.0f; angle < 360.0f; angle += step, around++)
                    {
                        Vector3 v = start;
                        Vector3 tl = row1[around];
                        Vector3 tr = row1[(around + 1) % row1.Count];
                        Vector2 vUV = ToUV(start,r1);
                        Vector2 tlUV = uvrow1[around];
                        Vector2 trUV = uvrow1[(around + 1) % row1.Count];
                        AddTriComputeNorm(_vertsP, _vertsN, _vertsC, _bw, _vertsUV, tl, tr, v, tlUV, trUV, vUV,
                            Color.white, row); //bone);
                    }
                }

                temp = row1;
                row1 = row2;
                row2 = temp;
                temp = null;

                tempUV = uvrow1;
                uvrow1 = uvrow2;
                uvrow2 = tempUV;
                tempUV = null;

            }
            if (EndCap2)
            {
                around = 0;

                Vector3 v = start + upwards * 1.0f;


                around = 0;
                for (float angle = 0.0f; angle < 360.0f; angle += step, around++)
                {
                    Vector3 bl = row2[around];
                    Vector3 br = row2[(around + 1) % row2.Count];
                    Vector3 tr = v;

                    Vector2 blUV = uvrow2[around];
                    Vector2 brUV = uvrow2[(around + 1) % row2.Count];
                    Vector2 trUV = ToUV(Vector3.zero,r1);
                    AddTriComputeNorm(_vertsP, _vertsN, _vertsC, _bw, _vertsUV, br, bl, tr, brUV, blUV, trUV,
                        Color.white, row); //bone);
                }
            }
        }

        private void AddQuad(List<Vector3> ms_VertsP, List<Vector3> ms_VertsN, List<Color32> ms_VertsC,
            List<BoneWeight> _bw, List<Vector2> ms_VertsUV,
            Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector2 tlUV, Vector2 trUV, Vector2 brUV, Vector2 blUV,
            Vector3 norm, Color _col, int bone)
        {
            Color32 col = _col;

            ms_VertsP.Add(bl);
            ms_VertsP.Add(tr);
            ms_VertsP.Add(tl);
            ms_VertsP.Add(bl);
            ms_VertsP.Add(br);
            ms_VertsP.Add(tr);

            ms_VertsC.Add(col);
            ms_VertsC.Add(col);
            ms_VertsC.Add(col);
            ms_VertsC.Add(col);
            ms_VertsC.Add(col);
            ms_VertsC.Add(col);

            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);

            ms_VertsUV.Add(blUV);
            ms_VertsUV.Add(trUV);
            ms_VertsUV.Add(tlUV);
            ms_VertsUV.Add(blUV);
            ms_VertsUV.Add(brUV);
            ms_VertsUV.Add(trUV);

            /*
                BoneWeight bw=new BoneWeight();
                bw.boneIndex0 = bone;
                bw.weight0 = 1.0f;
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
        */



        }

        private void AddTri(List<Vector3> ms_VertsP, List<Vector3> ms_VertsN, List<Color32> ms_VertsC,
            List<BoneWeight> _bw, List<Vector2> ms_VertsUV,
            Vector3 tl, Vector3 tr, Vector3 bl, Vector2 tlUV, Vector2 trUV, Vector2 blUV, Vector3 norm, Color _col,
            int bone)
        {
            Color32 col = _col;

            ms_VertsP.Add(bl);
            ms_VertsP.Add(tr);
            ms_VertsP.Add(tl);

            ms_VertsC.Add(col);
            ms_VertsC.Add(col);
            ms_VertsC.Add(col);

            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);
            ms_VertsN.Add(norm);

            ms_VertsUV.Add(blUV);
            ms_VertsUV.Add(trUV);
            ms_VertsUV.Add(tlUV);
            /*
                BoneWeight bw=new BoneWeight();
                bw.boneIndex0 = bone;
                bw.weight0 = 1.0f;
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
                _bw.Add(bw);
        */



        }

        private void AddQuadComputeNorm(List<Vector3> ms_VertsP, List<Vector3> ms_VertsN, List<Color32> ms_VertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV,
            Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector2 tlUV, Vector2 trUV, Vector2 brUV, Vector2 blUV,
            Color col, int bone)
        {

            Vector3 norm = Vector3.Cross(tl - tr, bl - tr);
            norm.Normalize();

            AddQuad(ms_VertsP, ms_VertsN, ms_VertsC, _bw, _vertsUV, tl, tr, br, bl, tlUV, trUV, brUV, blUV,
                norm, col, bone);


        }

        private void AddTriComputeNorm(List<Vector3> ms_VertsP, List<Vector3> ms_VertsN, List<Color32> ms_VertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV,
            Vector3 tl, Vector3 tr, Vector3 bl, Vector2 tlUV, Vector2 trUV, Vector2 blUV, Color col, int bone)
        {

            Vector3 norm = Vector3.Cross(tl - tr, bl - tr);
            norm.Normalize();

            AddTri(ms_VertsP, ms_VertsN, ms_VertsC, _bw, _vertsUV, tl, tr, bl, tlUV, trUV, blUV, norm, col, bone);


        }

        public void Build()
        {

            List<Vector3> vertsP = new List<Vector3>();
            List<Vector3> vertsN = new List<Vector3>();
            List<Color32> vertsC = new List<Color32>();
            List<BoneWeight> bw = new List<BoneWeight>();
            List<Vector2> vertsUV = new List<Vector2>();
            BuildPass1(vertsP, vertsN, vertsC, bw, vertsUV);
            BuildPass2(vertsP, vertsN, vertsC, bw, vertsUV);

            SnapShot();
        }

        public bool BuildPass1(List<Vector3> ms_VertsP, List<Vector3> ms_VertsN, List<Color32> ms_VertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV)
        {



            //        Color32 col = Color.red;

            ms_VertsN.Clear();
            ms_VertsP.Clear();
            ms_VertsC.Clear();
            _bw.Clear();




            //we clear the dirty flag here so that if it changes while its being built it will get rebuilt again
            GenVertsFromData(ms_VertsP, ms_VertsN, ms_VertsC, _bw, _vertsUV);




            return true;
        }



        public void BuildPass2(List<Vector3> ms_VertsP, List<Vector3> ms_VertsN, List<Color32> ms_VertsC, List<BoneWeight> _bw, List<Vector2> _vertsUV)
        {
            if (ms_VertsP.Count == 0)
                return;
            if (ms_VertsP.Count != ms_VertsC.Count || ms_VertsP.Count != ms_VertsN.Count)
            {
                Debug.LogError(" vertex errorStart  " + ms_VertsP.Count + " " + ms_VertsC.Count + " " + ms_VertsN.Count);
                return;
            }
            //        Profiler.BeginSample("Make Mesh");

            //        m_SourceRenderer.sharedMesh = mesh;
            if (m_Param.m_Mesh == null)
                m_Param.m_Mesh = new Mesh();

            Mesh mesh = m_Param.m_Mesh;
            mesh.Clear();
/*
            if (m_MeshFilter == null)
            {
                GameObject go = new GameObject("hexagon");
                var mf = go.AddComponent<MeshFilter>();
                var mr = go.AddComponent<MeshRenderer>();
                mr.material = new Material(Shader.Find("Diffuse"));
                m_MeshFilter = mf;


            }
            if (m_MeshFilter)
                m_MeshFilter.sharedMesh = mesh;
*/


            int vertexCount = ms_VertsP.Count;
            if (ms_VertsP.Count == 0 || ms_VertsP.Count != ms_VertsC.Count || ms_VertsP.Count != ms_VertsN.Count ||
                vertexCount != ms_VertsN.Count)
            {
                Debug.LogError(" vertex error AtSet  " + ms_VertsP.Count + " " + ms_VertsC.Count + " " + ms_VertsN.Count);
            }


            //        mesh.boneWeights = _bw.ToArray();
            mesh.SetVertices(ms_VertsP);
            mesh.SetNormals(ms_VertsN);
            mesh.SetUVs(0, _vertsUV);
            if (ms_VertsP.Count == 0 || ms_VertsP.Count != ms_VertsC.Count || ms_VertsP.Count != ms_VertsN.Count ||
                vertexCount != ms_VertsN.Count)
            {
                Debug.LogError(" vertex error AtSet  " + ms_VertsP.Count + " " + ms_VertsC.Count + " " + ms_VertsN.Count);
                return;
            }

            //        mesh.SetColors(ms_VertsC);

            int[] newTriangles = new int[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                newTriangles[i] = i;

            mesh.triangles = newTriangles;


            if (ms_VertsP.Count == 0 || ms_VertsP.Count != ms_VertsC.Count || ms_VertsP.Count != ms_VertsN.Count ||
                vertexCount != ms_VertsN.Count)
            {
                Debug.LogError(" vertex error At exit  " + ms_VertsP.Count + " " + ms_VertsC.Count + " " +
                               ms_VertsN.Count);
                return;
            }


        }

        private Camera m_Cam;
        protected void SnapShot()
        {
            if (m_Cam == null)
            {
                var cam = new GameObject("Camera");
                m_Cam = cam.AddComponent<Camera>();
                m_Cam.clearFlags=CameraClearFlags.Color;
                m_Cam.backgroundColor = Color.black;
                m_Cam.cullingMask = 1 << 30;
                m_Cam.enabled = false;
            }
            m_Cam.transform.position = Vector3.zero + Vector3.forward * ( 10) + Vector3.up * ( 10);

            m_Cam.transform.rotation = Quaternion.LookRotation(Vector3.zero + m_Param.m_Mesh.bounds.center - m_Cam.transform.position);

            if (m_CacheIcon == null)
                m_CacheIcon = new RenderTexture(256, 256, 32, RenderTextureFormat.ARGB32);
            
            m_Cam.targetTexture = m_CacheIcon;
            Graphics.DrawMesh(m_Param.m_Mesh, Vector3.zero, Quaternion.identity, new Material(Shader.Find("Diffuse")),30, m_Cam);
            
            m_Cam.Render();


        }


//        public MeshFilter m_MeshFilter;
        protected MeshParam m_Param;

        public bool GetInput(int _input, out MeshParam _out)
        {
            _out = null;

            if (Inputs == null || _input >= Inputs.Count)
            {
                Debug.LogError("not enough inputs for " + this);
                return false;
            }


            if (Inputs[_input].connection != null)
                _out = Inputs[_input].connection.GetValue<MeshParam>();
            else
                return false;
                //_out = TextureParam.GetWhite();


            return true;
        }

    }
}
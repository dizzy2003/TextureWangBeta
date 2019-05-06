using System.Collections.Generic;
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Mesh/TwistMesh")]
    public class MeshTwist : MeshBase
    {
        public const string ID = "TwistMesh";
        public override string GetID { get { return ID; } }

        

        public FloatRemap m_ScaleTwist;


        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            
            m_ScaleTwist.SliderLabel(this, "ScaleTwist");

        }

        public override
            Node Create(Vector2 pos)
        {

            MeshTwist node = CreateInstance<MeshTwist>();

            node.rect = new Rect(pos.x, pos.y, 200, 200);
            node.name = "TwistMesh";
            node.CreateOutput("mesh", "MeshParam", NodeSide.Right, 50);
            node.CreateInput("Mesh", "MeshParam", NodeSide.Left, 50);

            node.m_ScaleTwist = new FloatRemap(0.5f, -100, 100);


            return node;
        }

        protected override  void GenVertsFromData(List<Vector3> _vertsP, List<Vector3> _vertsN, List<Color32> _vertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV)
        {


        }




        public override bool Calculate()
        {
            if (m_Param == null)
                m_Param = new MeshParam();


            MeshParam input = null;

            if (!GetInput(0, out input))
                return false;

            if (m_Param != null)
            {
                m_Param.m_Mesh = new Mesh();
                var vertices = input.m_Mesh.vertices;
                var ns = input.m_Mesh.normals;
                for (var i = 0; i < vertices.Length; i++)
                {
                    Quaternion q=Quaternion.Euler(0, vertices[i].y* m_ScaleTwist, 0);
                    vertices[i] = q*vertices[i];
                    ns[i] = q * ns[i];
                }
                m_Param.m_Mesh.vertices = vertices;
                m_Param.m_Mesh.normals = ns;
                m_Param.m_Mesh.uv = input.m_Mesh.uv;
                m_Param.m_Mesh.triangles = input.m_Mesh.triangles;

                SnapShot();





                //Build();
            }
//            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture(); 
            if (Outputs.Count > 0)
                Outputs[0].SetValue<MeshParam>(m_Param);

//            CheckDiskIcon(name, m_Param);
            return true;
        }
    }
}
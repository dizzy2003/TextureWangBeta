using System.Collections.Generic;
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Mesh/ScaleFromPoint")]
    public class MeshScaleFromPoint : MeshBase
    {
        public const string ID = "ScaleFromPoint";
        public override string GetID { get { return ID; } }

        
        public FloatRemap m_ScaleFromX;
        public FloatRemap m_ScaleFromY;
        public FloatRemap m_ScaleFromZ;
        public FloatRemap m_Scale;


        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();

            m_Scale.SliderLabel(this, "Scale");
            m_ScaleFromX.SliderLabel(this, "ScaleFromX");
            m_ScaleFromY.SliderLabel(this, "ScaleFromY");
            m_ScaleFromZ.SliderLabel(this, "ScaleFromZ");

        }

        public override
            Node Create(Vector2 pos)
        {

            MeshScaleFromPoint node = CreateInstance<MeshScaleFromPoint>();

            node.rect = new Rect(pos.x, pos.y, 200, 200);
            node.name = "ScaleFromPos";
            node.CreateOutput("mesh", "MeshParam", NodeSide.Right, 50);
            node.CreateInput("Mesh", "MeshParam", NodeSide.Left, 50);

            node.m_Scale = new FloatRemap(1.0f, -100, 100);
            node.m_ScaleFromX = new FloatRemap(0.0f, -100, 100);
            node.m_ScaleFromY = new FloatRemap(0.0f, -100, 100);
            node.m_ScaleFromZ = new FloatRemap(0.0f, -100, 100);
            


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
                Vector3 from = new Vector3(m_ScaleFromX, m_ScaleFromY, m_ScaleFromZ);
                for (var i = 0; i < vertices.Length; i++)
                {
                    Vector3 delta = vertices[i]-from;
                    delta *= m_Scale;
                    
                    vertices[i] = delta+from;
//                    ns[i] = q * ns[i];
                }
                m_Param.m_Mesh.vertices = vertices;
                //m_Param.m_Mesh.normals = ns;
                m_Param.m_Mesh.uv = input.m_Mesh.uv;
                m_Param.m_Mesh.triangles = input.m_Mesh.triangles;
                m_Param.m_Mesh.RecalculateNormals();

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
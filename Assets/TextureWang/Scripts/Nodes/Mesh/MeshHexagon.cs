using System.Collections.Generic;
using NodeEditorFramework;
using UnityEngine;


namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Mesh/Hexagon")]
    public class MeshHexagon : MeshBase
    {
        public const string ID = "CreateHexagon";
        public override string GetID { get { return ID; } }

        
        

        

        public FloatRemap m_Radius;
        public FloatRemap m_Length;
        public FloatRemap m_Sides;
        public FloatRemap m_SubDivide;

        public override void DrawNodePropertyEditor()
        {
            base.DrawNodePropertyEditor();
            m_Radius.SliderLabel(this, "Radius");
            m_Length.SliderLabel(this, "Height");
            m_Sides.SliderLabelInt(this, "Sides");
            m_SubDivide.SliderLabelInt(this, "vertical SubDivide");

        }

            public override
            Node Create(Vector2 pos)
        {

            MeshHexagon node = CreateInstance<MeshHexagon>();

            node.rect = new Rect(pos.x, pos.y, 200, 200);
            node.name = "MeshHexagon";
            node.CreateOutput("Hexagon", "MeshParam", NodeSide.Right, 50);

            node.m_Radius = new FloatRemap(0.5f, 0, 10);
            node.m_Length = new FloatRemap(0.5f, 0, 10);
            node.m_Sides = new FloatRemap(6, 3, 100);
            node.m_SubDivide = new FloatRemap(2, 2, 100);


            return node;
        }

        protected override  void GenVertsFromData(List<Vector3> _vertsP, List<Vector3> _vertsN, List<Color32> _vertsC,
            List<BoneWeight> _bw, List<Vector2> _vertsUV)
        {
            CreateTube(_vertsP, _vertsN, _vertsC, _bw, _vertsUV,
                -Vector3.right * 4.0f, m_Radius, m_Length, (int)m_Sides, (int)m_SubDivide, 0.0f, 0.0f, Vector3.up, Vector3.right, true, true,1);


        }




        public override bool Calculate()
        {

            if (m_Param == null)
                m_Param = new MeshParam();


            if (m_Param != null)
            {

                Build();
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
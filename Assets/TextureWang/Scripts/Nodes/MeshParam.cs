using System;
using System.IO;
using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{

    public class MeshParamType : IConnectionTypeDeclaration
    {
        public string Identifier { get { return "MeshParam"; } }
        public Type Type { get { return typeof(MeshParam); } }
        public Color Color { get { return Color.white; } }
        public string InKnobTex { get { return "Textures/In_Knob.png"; } }
        public string OutKnobTex { get { return "Textures/Out_Knob.png"; } }
    }
    public class MeshParam
    {

        static MeshParam ms_White;



        public Mesh m_Mesh;
    }
}
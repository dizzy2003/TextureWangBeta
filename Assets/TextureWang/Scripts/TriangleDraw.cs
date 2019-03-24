using UnityEngine;

namespace Assets.TextureWang.Scripts
{
    public class TriangleDraw
    {
        static Material m_LineMaterial;
        static void CreateLineMaterial()
        {


            if (m_LineMaterial == null)
            {
                m_LineMaterial = new Material(Shader.Find("GUI/Text Shader"));
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");

                m_LineMaterial = new Material(shader);
                m_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn off alpha blending
                m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                // Turn backface culling off
                m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                m_LineMaterial.SetInt("_ZWrite", 0);
            }
        }

        static void ClearRenderTarget()
        {


            GL.Clear(true, true, Color.black);

        }

        public static void GPUStart(RenderTexture _out)
        {

            Graphics.SetRenderTarget(_out);
            CreateLineMaterial();
            ClearRenderTarget();



            GL.PushMatrix();
            //ProcHeight.Get().GetGPUMaskMat().SetColor("Color", Color.white);
            m_LineMaterial.SetPass(0);
            GL.LoadOrtho();
            /*
                float ox = (ProcHeight.Get().m_OffsetX)*1024;
                float oz = (ProcHeight.Get().m_OffsetZ)*1024;
                GL.LoadPixelMatrix(ox,oz,ox+128,oz+128);
        */
            GL.Begin(GL.TRIANGLES);
        }
        public static void GPUStart(RenderTexture _out,int mode)
        {

            Graphics.SetRenderTarget(_out);
            CreateLineMaterial();
            ClearRenderTarget();



            GL.PushMatrix();
            //ProcHeight.Get().GetGPUMaskMat().SetColor("Color", Color.white);
            m_LineMaterial.SetPass(0);
            GL.LoadOrtho();
            /*
                float ox = (ProcHeight.Get().m_OffsetX)*1024;
                float oz = (ProcHeight.Get().m_OffsetZ)*1024;
                GL.LoadPixelMatrix(ox,oz,ox+128,oz+128);
        */
            GL.Begin(mode);
        }
        public static void GPUEnd()
        {
            GL.End();
            GL.PopMatrix();
            Graphics.SetRenderTarget(null);
            GL.RenderTargetBarrier(); // Is this needed?

        }
        public static void AddVertsForTri(Vector3 v0, Vector3 v1, Vector3 v2, Color c0, Color c1, Color c2)
        {
            GL.Color(c0);
        
            GL.Vertex(v0);
            GL.Color(c1);
            GL.Vertex(v1);
            GL.Color(c2);
            GL.Vertex(v2);
        }

        public static void AddVert(Vector3 v0)
        {
            

            GL.Vertex(v0);
        }
        public static void AddCol( Color c0)
        {
            GL.Color(c0);

            
        }

        public static void Test(RenderTexture _out)
        {
            GPUStart(_out);
            AddVertsForTri(
                new Vector3(.3f, .3f, 0),
                new Vector3(.6f, .3f, 0),
                new Vector3(.6f, .6f, 0),
                Color.red, Color.green, Color.blue);
            GPUEnd();
        }




    }
}

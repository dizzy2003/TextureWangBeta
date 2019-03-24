using NodeEditorFramework;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{
    [Node(false, "Source/Polygon")]
    public  class TexturePolygon : TextureNode
    {
        private const string m_Help = "Draw an N sided polygon";
        public override string GetHelp() { return m_Help; }
        //    public enum TexOP { Add, ClipMin, Multiply, Power,Blur,CalcNormal,Level1,Transform,Gradient,Min,Max }


        //public Texture m_Cached;
        public FloatRemap m_Seed;//where all 0.5
        public FloatRemap m_Sides  ;//where all 0.5
        public FloatRemap m_X ;
        public FloatRemap m_Y;
        public FloatRemap m_Radius  ;
        public FloatRemap m_Angle;

        public FloatRemap m_RandomizeVertPos;
        public FloatRemap m_RandomizeRadius;

        public FloatRemap m_OuterBrightness;
        public FloatRemap m_InnerBrightness;
    
        public FloatRemap m_RandomizeOuterBrightness;
        public FloatRemap m_RandomizeInnerBrightness;

        public FloatRemap m_IncrementBrightnessPerVert;
        public FloatRemap m_IncrementBrightnessPerVertMid;

        public const string ID = "TexturePolygon";
        public override string GetID { get { return ID; } }

        public override Node Create(Vector2 pos)
        {

            TexturePolygon node = CreateInstance<TexturePolygon>();

            node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
            node.name = "Polygon";
            node.CreateInputOutputs();
            node.m_Seed = new FloatRemap(1, 0, 10000000);
            node.m_Sides = new FloatRemap(5, 3, 360);
            node.m_X = new FloatRemap(0.5f, 0, 1);
            node.m_Y = new FloatRemap(0.5f, 0, 1);
            node.m_Radius = new FloatRemap(0.3f, 0, 1);
            node.m_RandomizeVertPos = new FloatRemap(0.0f, 0, 1);
            node.m_RandomizeRadius = new FloatRemap(0.0f, 0, 1);

            node.m_OuterBrightness = new FloatRemap(0.5f, 0, 1);
            node.m_InnerBrightness = new FloatRemap(0.5f, 0, 1);

            node.m_RandomizeOuterBrightness = new FloatRemap(0.0f, 0, 10);
            node.m_RandomizeInnerBrightness = new FloatRemap(0.0f, 0, 10);
            node.m_Angle = new FloatRemap(0.0f, 0, 360);

            node.m_IncrementBrightnessPerVert = new FloatRemap(0.0f, -.5f, .5f);
            node.m_IncrementBrightnessPerVertMid = new FloatRemap(0.0f, -.5f, .5f);
            node.m_Seed.m_Mult = 9999999;


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

            m_Seed.SliderLabelInt(this, "Seed");
            m_Sides.SliderLabelInt(this, "SideCount");
            m_Radius.SliderLabel(this, "Radius");
            m_Angle.SliderLabel(this, "Angle");
            m_X.SliderLabel(this, "Mid X");
            m_Y.SliderLabel(this, "Mid Y");

            m_RandomizeVertPos.SliderLabel(this, "Randomize vert Positions");
            m_RandomizeRadius.SliderLabel(this, "Randomize radius per vert");

            m_OuterBrightness.SliderLabel(this, "Outer Brightness");
            m_InnerBrightness.SliderLabel(this, "Inner Brightness");
        

            m_RandomizeOuterBrightness.SliderLabel(this, "Randomize Outer Brightness (add)");
            m_RandomizeInnerBrightness.SliderLabel(this, "Randomize Inner Brightness (add)");
        
            m_IncrementBrightnessPerVert.SliderLabel(this, "Increment Brightness Per Vert)");
            m_IncrementBrightnessPerVertMid.SliderLabel(this, "Increment Brightness Per Vert mid)");
        }

        float Radius
        {
            get { return m_Radius + Random.value*m_RandomizeRadius; }
        }


        public void Generate( TextureParam _output)
        {
            int seed = (int) m_Seed;
            
            Random.InitState(seed);
            RenderTexture destination = CreateRenderDestination(null, _output);

            TriangleDraw.GPUStart(destination);
            float angle = 0;;

            Color prevCol=Color.white*(m_OuterBrightness+m_RandomizeOuterBrightness*Random.value);
            prevCol.a = 1;

            Vector3 prev=new Vector3(m_X+Mathf.Cos(angle+ m_Angle * Mathf.Deg2Rad) *Radius, m_Y + Mathf.Sin(angle + m_Angle * Mathf.Deg2Rad) * Radius,0);
            prev += new Vector3((Random.value-0.5f) * m_RandomizeVertPos, (Random.value - 0.5f) * m_RandomizeVertPos, 0);
        
            Color midCol = Color.white * (m_InnerBrightness + m_RandomizeInnerBrightness * Random.value);
            midCol.a = 1;

            Vector3 mid = new Vector3(m_X , m_Y , 0);
            mid += new Vector3(Random.value * m_RandomizeVertPos, Random.value * m_RandomizeVertPos, 0);


            Vector3 firstPos = prev;
            Color firstCol = prevCol;

            int sides = (int) m_Sides;

            float step = Mathf.PI*2/sides;
            float count = 1;
            for ( angle = step; angle < Mathf.PI*2-step*0.5f; angle += step,count+=1.0f)
            {
                Vector3 pos = new Vector3(m_X + Mathf.Cos(angle + m_Angle * Mathf.Deg2Rad) * Radius, m_Y + Mathf.Sin(angle + m_Angle * Mathf.Deg2Rad) * Radius, 0);
                pos+= new Vector3((Random.value - 0.5f) * m_RandomizeVertPos, (Random.value - 0.5f) * m_RandomizeVertPos, 0);

                Color col = Color.white * (m_OuterBrightness + m_RandomizeOuterBrightness * Random.value+m_IncrementBrightnessPerVert*count);
                Color midColUse = midCol+ Color.white * ( m_IncrementBrightnessPerVertMid*count);
                col.a = 1;

                TriangleDraw.AddVertsForTri(
                    prev,
                    mid,
                    pos,
                    prevCol, midColUse, col);
                prev = pos;
                prevCol = col;
            }

            TriangleDraw.AddVertsForTri(
                prev,
                mid,
                firstPos,
                prevCol, midCol, firstCol);

            TriangleDraw.GPUEnd();

        }
   
 
        public override bool Calculate()
        {

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
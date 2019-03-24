using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Assets.TextureWang.Scripts.Nodes
{ //namespace TextureWang
//{

    public enum ChannelType
    {
        EightBit,
        HalfFloat,
        Float
    }

    public enum ShaderOp
    {
        MultRGB=0,
        Gradient=1,
        Distort=2,
        Normal=3,
        AddRGB=4,
        Add2=5,
        Mult2=6,
        Pow2=7,
        Min=8,
        Max=9,
        clipMin=10, //if < input colour set to 0
        Blend2=11,
        Sub2=12,
        Level1=13,
        Transform=14,
        DirectionWarp = 15,
        Power=16,
        Min1=17,
        Max1=18,
        CopyGrey=19,
        CopyColor = 20, 
        Step=21,
        Invert=22,
        SrcBlend=23,
        Stepify=24,
        EdgeDist=25,
        Smooth=26,
        BlackEdge=27,
        CopyColorAndAlpha = 28, 
        EdgeDistDir = 29,
        Splatter=30,
        SplatterGrid=31,
        Sobel=32,
        GenCurve = 33,
        AbsDistort=34,
        MapCylinder=35,
        CopyRandA = 36, //used for unity metalic and roughness
        Histogram = 37,
        ProbabilityBlend = 38,
        RandomEdge=39,
        CopyRGBA = 40,
        CopyNormalMap=41,
        MaskedBlend = 42,
        Sharpen =43,
        Emboss=44,
        Divide=45,
        AngleWarp = 46,
        Posterize=47,
        Dot=48,
        UVOffset=49,
        Cos = 50,
        AnimCurveDraw=51,
        EdgeDistFixedDir = 52,
        AO=53,
        SplatterGridProb=54,
        SmoothedMask=55,
        SmoothedDirection=56,
        CopyRGBAChannels = 57,
        Quadify=58,
        MapSphere=59,
        Symetry=60,


        SetCol = 0,
        PerlinBm=1,
        PerlinTurb = 2,
        PerlinRidge = 3,
        VeroniNoise = 4,
        Pattern     = 5,
        Grid        = 6,
        Weave       = 7,
        Circle      = 8,
        Ripples     = 9,
        Square      = 10,
        Hexagon     = 11,
        WangNoise   = 12,
        Distance    = 13


    }


    public class TextureParamType : IConnectionTypeDeclaration
    {
        public string Identifier { get { return "TextureParam"; } }
        public Type Type { get { return typeof(TextureParam); } }
        public Color Color { get { return Color.white; } }
        public string InKnobTex { get { return "Textures/In_Knob.png"; } }
        public string OutKnobTex { get { return "Textures/Out_Knob.png"; } }
    }


    public class TextureParam
    {
        public static TextureFormat ms_TexFormat = TextureFormat.RGBAFloat;
        public static RenderTextureFormat ms_RTexFormat = RenderTextureFormat.ARGBFloat;

        static TextureParam ms_White;

        static public TextureParam GetWhite()
        {
            if (ms_White == null)
            {
                ms_White=new TextureParam(1,1);
                ms_White.m_Tex=new Texture2D(1,1,TextureFormat.ARGB32, false);
                ms_White.m_Tex.SetPixel(0,0,Color.white);
                ms_White.m_Tex.Apply();
                ms_White.m_Channels = 4;
                

            }
            return ms_White;

        }
    

        public static RenderTextureFormat GetRTFormat(bool _grey, ChannelType _pixeltype)
        {
            if (_grey)
            {
                switch (_pixeltype)
                {
                    case ChannelType.Float:
                        return RenderTextureFormat.RFloat;
                    case ChannelType.HalfFloat:
                        return RenderTextureFormat.RHalf;
                    case ChannelType.EightBit:
                        return RenderTextureFormat.R8;
                }
                return RenderTextureFormat.RFloat;
            }
            else
            {
                switch (_pixeltype)
                {
                    case ChannelType.Float:
                        return RenderTextureFormat.ARGBFloat;
                    case ChannelType.HalfFloat:
                        return RenderTextureFormat.ARGBHalf;
                    case ChannelType.EightBit:
                        return RenderTextureFormat.ARGB32;
                }
                return RenderTextureFormat.ARGBFloat;
            }
        }
        public static TextureFormat GetTexFormat(bool _grey)
        {
            if (_grey)
            {
                return TextureFormat.RGBAFloat;
            }
            else
            {
                return TextureFormat.RFloat;
            }
        }



        public Texture2D m_Tex; //only used by TextureInput
        public RenderTexture m_Destination;
        public int m_Width = 1024;
        public int m_Height = 1024;
//        public float[] data;

//        public bool m_DataValid = false;
        public int m_Channels;


        public void SetTex(Texture2D _tex)
        {
            m_Tex = _tex;
//            m_DataValid = false;
        }
/*
        public float[] GetChannel(int channel)
        {
            float[] target = new float[data.Length / 4];
            int j = 0;
            for (int i = channel; i < data.Length; i += 4)
                target[j++] = data[i] * 256.0f;
            return target;
        }
        public void SetChannel(float[] src, int channel)
        {
            int j = 0;
            for (int i = channel; i < data.Length; i += 4)
                data[i] = src[j++] / 256.0f;

        }
*/
        public Texture GetHWSourceTexture()
        {
            if (m_Tex != null)
                return m_Tex;
            /*

                        if(m_Tex==null && m_Destination==null)
                        {
                            m_Tex=CreateTexture(ms_TexFormat);
                            return m_Tex;
                        }
            */
            return m_Destination;
        }

        public Texture2D CreateTexture(Color[] data,TextureFormat format= TextureFormat.ARGB32)
        {
            if (m_Tex == null)
            {
//                Debug.LogError(" have to make Texture");
                m_Tex = new Texture2D(m_Width, m_Height, format, false);
            }

            m_Tex.filterMode = FilterMode.Bilinear;
            m_Tex.wrapMode = TextureWrapMode.Repeat;
            m_Tex.SetPixels(data);
            m_Tex.Apply();
            return m_Tex;
        }

        public bool IsGrey()
        {
            return m_Channels == 1;
        }

        void SetChannelCount(RenderTextureFormat _texFormat)
        {
            switch (_texFormat)
            {
                case RenderTextureFormat.R8:
                case RenderTextureFormat.RHalf:
                case RenderTextureFormat.RFloat:
                    m_Channels = 1;
                    break;
                default:
                    m_Channels = 4;
                    break;
            }
        }

        public RenderTexture CreateRenderDestination(int _width, int _height,  RenderTextureFormat _texFormat,bool _filter=true)
        {
//            _width = m_Width;
//            _height = m_Height;
            if (m_Destination == null||m_Destination.format!=_texFormat||_width!=m_Destination.width ||_height!=m_Destination.height)
            {
                m_Destination = new RenderTexture(_width, _height,0, _texFormat,RenderTextureReadWrite.Linear);
                m_Destination.wrapMode = TextureWrapMode.Repeat;
                
                SetChannelCount(_texFormat);
                m_Width = _width;
                m_Height = _height;

            }
            m_Destination.filterMode = _filter ? FilterMode.Bilinear : FilterMode.Point;
        
            return m_Destination;
        }

 

        public TextureParam()
        {
            
        }
        public TextureParam(int w,int h)
        {
            m_Width = w;
            m_Height = h;
            
        }
        public TextureParam(TextureParam _other)
        {
            m_Width = _other.m_Width;
            m_Height = _other.m_Height;
            if (_other.m_Destination != null)
            {
                var rt = _other.m_Destination;
                m_Destination= CreateRenderDestination(rt.width,rt.height,rt.format, rt.filterMode==FilterMode.Bilinear);

            

            }

        }

        //if we need the data extracted from a renderTexture (for eaxmple to get CPU access to pixles
        public Texture2D GetTex2D()
        {
            if (m_Tex != null)
                return m_Tex;
            var tex = new Texture2D(m_Width, m_Height, TextureParam.ms_TexFormat, false);

            if (IsGrey())
            {
                //for grey scale we have to create a new texture with alpha channel ==1

                RenderTexture rt = new RenderTexture(m_Width, m_Height, 0, RenderTextureFormat.ARGB32);

                Material m = TextureNode.GetMaterial("TextureOps");
                m.SetInt("_MainIsGrey", IsGrey() ? 1 : 0);
                m.SetInt("_TextureBIsGrey", 1);
                m.SetTexture("_GradientTex", GetWhite().m_Destination);

                Graphics.Blit(GetHWSourceTexture(), rt, m, (int)ShaderOp.CopyColorAndAlpha);

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                //input.DestinationToTexture(m_Output);
                tex.Apply();
                RenderTexture.active = null;
                rt.DiscardContents();
                rt.Release();
                rt = null;

            }
            else
            {

                RenderTexture.active = m_Destination;
                tex.ReadPixels(new Rect(0, 0, m_Width, m_Height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

            }
            return tex;
        }

        public void SavePNG(string path,int _width,int _height)
        {
            var tex = new Texture2D(_width, _height, TextureParam.ms_TexFormat, false);

            if (IsGrey())
            {
                //for grey scale we have to create a new texture with alpha channel ==1

                RenderTexture rt = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);

                Material m = TextureNode.GetMaterial("TextureOps");
                m.SetInt("_MainIsGrey", IsGrey() ? 1 : 0);
                m.SetInt("_TextureBIsGrey", 1);
                m.SetTexture("_GradientTex", GetWhite().m_Destination);

                Graphics.Blit(GetHWSourceTexture(), rt, m, (int) ShaderOp.CopyColorAndAlpha);

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                //input.DestinationToTexture(m_Output);
                tex.Apply();
                RenderTexture.active = null;
                rt.DiscardContents();
                rt.Release();
                rt = null;

            }
            else
            {

                RenderTexture.active = m_Destination;
                tex.ReadPixels(new Rect(0, 0, m_Width, m_Height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

            }
            byte[] bytes = tex.EncodeToPNG();

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllBytes(path, bytes);
            }
        }

        public Color[] AllocData()
        {
            return new Color[m_Width * m_Height * 4];
        }
    }
//}
}
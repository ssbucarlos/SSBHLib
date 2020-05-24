﻿using System;
using CrossMod.Rendering;
using CrossMod.Tools;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using CrossMod.Rendering.Resources;

// Classes ported from StudioSB
// https://github.com/Ploaj/StudioSB/blob/master/LICENSE
namespace CrossMod.Nodes
{
    [FileTypeAttribute(".nutexb")]
    public class NutexNode : FileNode, IRenderableNode, IExportableTextureNode
    {
        public string TexName { get; private set; }

        private RTexture renderableTexture;
        private SBSurface surface;

        public NutexNode(string path) : base(path)
        {
            ImageKey = "texture";
            SelectedImageKey = "texture";
        }

        public override string ToString()
        {
            return Text.Contains(".") ? Text.Substring(0, Text.IndexOf(".")) : Text;
        }

        public override void Open()
        {
            surface = Open(AbsolutePath);
            // HACK: Empty streams?
            if (surface == null)
            {
                using (var image = Image.FromFile("DefaultTextures/default_black.png"))
                {
                    surface = SBSurface.FromBitmap((Bitmap)image);
                    surface.Name = Path.GetFileNameWithoutExtension(AbsolutePath);
                }
            }

            TexName = surface.Name;
        }

        public static SBSurface Open(string FilePath)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
            {
                // TODO: Why are there empty streams?
                if (reader.BaseStream.Length == 0)
                    return null;

                SBSurface surface = new SBSurface();

                reader.BaseStream.Position = reader.BaseStream.Length - 0xB0;

                int[] mipmapSizes = new int[16];
                for (int i = 0; i < mipmapSizes.Length; i++)
                    mipmapSizes[i] = reader.ReadInt32();

                reader.ReadChars(4); // TNX magic

                string texName = ReadTexName(reader);
                surface.Name = texName;

                surface.Width = reader.ReadInt32();
                surface.Height = reader.ReadInt32();
                surface.Depth = reader.ReadInt32();

                var Format = (NUTEX_FORMAT)reader.ReadByte();

                reader.ReadByte();

                ushort Padding = reader.ReadUInt16();
                reader.ReadUInt32();

                int MipCount = reader.ReadInt32();
                int Alignment = reader.ReadInt32();
                surface.ArrayCount = reader.ReadInt32();
                int ImageSize = reader.ReadInt32();
                char[] Magic = reader.ReadChars(4);
                int MajorVersion = reader.ReadInt16();
                int MinorVersion = reader.ReadInt16();

                if (pixelFormatByNuTexFormat.ContainsKey(Format))
                    surface.PixelFormat = pixelFormatByNuTexFormat[Format];

                if (internalFormatByNuTexFormat.ContainsKey(Format))
                    surface.InternalFormat = internalFormatByNuTexFormat[Format];

                surface.PixelType = GetPixelType(Format);

                reader.BaseStream.Position = 0;
                byte[] ImageData = reader.ReadBytes(ImageSize);

                for (int array = 0; array < surface.ArrayCount; array++)
                {
                    MipArray arr = new MipArray();
                    for (int i = 0; i < MipCount; i++)
                    {
                        byte[] deswiz = SwitchSwizzler.GetImageData(surface, ImageData, array, i, MipCount);
                        arr.Mipmaps.Add(deswiz);
                    }
                    surface.Arrays.Add(arr);
                }

                return surface;
            }
        }

        public IRenderable GetRenderableNode()
        {
            // Don't initialize more than once.
            // We'll assume the context isn't destroyed.
            if (renderableTexture == null)
            {
                renderableTexture = new RTexture
                {
                    IsSrgb = surface.IsSRGB
                };

                // HACK: Nutex loading doesn't always work.
                try
                {
                    renderableTexture.renderTexture = surface.GetRenderTexture();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return renderableTexture;
        }

        public static void Export(string FileName, SBSurface surface)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(FileName, FileMode.Create)))
            {
                writer.Write(SwitchSwizzler.CreateBuffer(surface));

                uint ImageSize = (uint)writer.BaseStream.Position;

                foreach (var mip in surface.Arrays)
                {
                    foreach (var m in mip.Mipmaps)
                        writer.Write(m.Length);
                    for (int i = mip.Mipmaps.Count; i < 0x10; i++)
                        writer.Write(0);
                }

                writer.Write(new char[] { ' ', 'X', 'N', 'T' });
                writer.Write(surface.Name.ToCharArray());
                writer.Write(new byte[0x40 - surface.Name.Length]);
                writer.Write(surface.Width);
                writer.Write(surface.Height);
                writer.Write(surface.Depth);
                writer.Write((byte)TexFormatByInternalFormat(surface.InternalFormat)); // format
                writer.Write((byte)4); // unknown usually 4
                writer.Write((short)0); // pad
                writer.Write(surface.IsCubeMap ? 9 : 4); // unknown usually 4 9 for cubemap
                writer.Write(surface.Arrays[0].Mipmaps.Count);
                writer.Write(0x1000); // alignment
                writer.Write(surface.Arrays.Count); // array count
                writer.Write(ImageSize);

                writer.Write(new char[] { ' ', 'X', 'E', 'T' });
                writer.Write((short)1); // version major
                writer.Write((short)2); // version minor
            }
        }

        public void SaveTexturePNG(string fileName)
        {
            System.Drawing.Bitmap texture = ((RTexture)GetRenderableNode()).renderTexture.GetBitmap(0);
            texture.Save(fileName);
            texture.Dispose();
        }

        private static string ReadTexName(BinaryReader reader)
        {
            var result = "";
            for (int i = 0; i < 0x40; i++)
            {
                byte b = reader.ReadByte();
                if (b != 0)
                    result += (char)b;
            }

            return result;
        }

        private static NUTEX_FORMAT TexFormatByInternalFormat(InternalFormat format)
        {
            foreach (var v in internalFormatByNuTexFormat)
                if (v.Value == format)
                    return v.Key;
            return NUTEX_FORMAT.BC1_SRGB;
        }

        public static readonly Dictionary<NUTEX_FORMAT, InternalFormat> internalFormatByNuTexFormat = new Dictionary<NUTEX_FORMAT, InternalFormat>()
        {
            { NUTEX_FORMAT.R8G8B8A8_SRGB, InternalFormat.SrgbAlpha },
            { NUTEX_FORMAT.R8G8B8A8_UNORM, InternalFormat.Rgba8 },
            { NUTEX_FORMAT.R32G32B32A32_FLOAT, InternalFormat.Rgba32f },
            { NUTEX_FORMAT.B8G8R8A8_UNORM, InternalFormat.Rgba8 },
            { NUTEX_FORMAT.B8G8R8A8_SRGB, InternalFormat.Rgba8Snorm },
            { NUTEX_FORMAT.BC1_UNORM, InternalFormat.CompressedRgbaS3tcDxt1Ext },
            { NUTEX_FORMAT.BC1_SRGB, InternalFormat.CompressedSrgbAlphaS3tcDxt1Ext },
            { NUTEX_FORMAT.BC2_UNORM, InternalFormat.CompressedRgbaS3tcDxt3Ext },
            { NUTEX_FORMAT.BC2_SRGB, InternalFormat.CompressedSrgbAlphaS3tcDxt3Ext },
            { NUTEX_FORMAT.BC3_UNORM, InternalFormat.CompressedRgbaS3tcDxt5Ext },
            { NUTEX_FORMAT.BC3_SRGB, InternalFormat.CompressedSrgbAlphaS3tcDxt5Ext },
            { NUTEX_FORMAT.BC4_UNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC4_SNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC5_UNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC5_SNORM, InternalFormat.Rgba },
            { NUTEX_FORMAT.BC6_UFLOAT, InternalFormat.CompressedRgbBptcUnsignedFloat },
            { NUTEX_FORMAT.BC7_UNORM, InternalFormat.CompressedRgbaBptcUnorm },
            { NUTEX_FORMAT.BC7_SRGB, InternalFormat.CompressedSrgbAlphaBptcUnorm }
        };


        private static PixelType GetPixelType(NUTEX_FORMAT format)
        {
            switch (format)
            {
                case NUTEX_FORMAT.R32G32B32A32_FLOAT:
                    return PixelType.Float;
                default:
                    return PixelType.Byte;
            }
        }
        /// <summary>
        /// Channel information for uncompressed formats.
        /// </summary>
        public static readonly Dictionary<NUTEX_FORMAT, PixelFormat> pixelFormatByNuTexFormat = new Dictionary<NUTEX_FORMAT, PixelFormat>()
        {
            { NUTEX_FORMAT.R32G32B32A32_FLOAT, PixelFormat.Rgba },
            { NUTEX_FORMAT.R8G8B8A8_SRGB, PixelFormat.Rgba },
            { NUTEX_FORMAT.R8G8B8A8_UNORM, PixelFormat.Rgba },
            { NUTEX_FORMAT.B8G8R8A8_UNORM, PixelFormat.Bgra },
            { NUTEX_FORMAT.B8G8R8A8_SRGB, PixelFormat.Bgra },
        };
    }

    public enum NUTEX_FORMAT
    {
        R8G8B8A8_UNORM = 0,
        R8G8B8A8_SRGB = 0x05,
        R32G32B32A32_FLOAT = 0x34,
        B8G8R8A8_UNORM = 0x50,
        //53
        B8G8R8A8_SRGB = 0x55,
        BC1_UNORM = 0x80,
        BC1_SRGB = 0x85,
        BC2_UNORM = 0x90,
        BC2_SRGB = 0x95,
        BC3_UNORM = 0xa0,
        BC3_SRGB = 0xa5,
        BC4_UNORM = 0xb0,
        BC4_SNORM = 0xb5,
        BC5_UNORM = 0xc0,
        BC5_SNORM = 0xc5,
        BC6_UFLOAT = 0xd7,
        BC7_UNORM = 0xe0,
        BC7_SRGB = 0xe5
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityLib.Utils
{
    public static class TGAUtils
    {
        public static Texture2D LoadTGA(byte[] buffer)
        {
            try
            {
                MemoryStream stream = new MemoryStream(buffer);
                BinaryReader reader = new BinaryReader(stream);
                reader.BaseStream.Seek(12, SeekOrigin.Begin);
                short width = reader.ReadInt16();
                short height = reader.ReadInt16();
                reader.BaseStream.Seek(2, SeekOrigin.Current);
                byte[] source = reader.ReadBytes(width * height * 4);
                reader.Close();
                Texture2D texture = new Texture2D(width, height, TextureFormat.BGRA32, false);
                texture.LoadRawTextureData(source);
                texture.Apply(false, true);
                return texture;
            }
            catch (Exception)
            {
                return Texture2D.blackTexture;
            }
        }
    }
}

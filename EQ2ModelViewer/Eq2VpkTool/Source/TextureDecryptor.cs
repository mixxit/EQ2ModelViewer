#region Using directives

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using Eq2FileInfo   = Everquest2.IO.FileInfo;
using Eq2FileStream = Everquest2.IO.FileStream;

#endregion

namespace Eq2VpkTool
{
    public class TextureDecryptor
    {
        public static byte[] Decrypt(Eq2FileInfo textureFile)
        {
            if (!Configuration.Instance.IsLoaded) return null;
            if (!CanDecrypt(textureFile))         return null;

            using (Eq2FileStream stream = textureFile.OpenRead())
            {
                // Read encrypted data
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);

                string key = GetDecryptionKey(textureFile);

                Decrypt(data, key);
                return data;
            }
        }


        public static bool CanDecrypt(Eq2FileInfo textureFile)
        {
            return GetDecryptionKey(textureFile) != null;
        }


        private static string GetDecryptionKey(Eq2FileInfo textureFile)
        {
            return Configuration.Instance.GetValue("/configuration/encrypted-maps/map[name=\"" + textureFile.FullName + "\"]/decryption-key");
        }


        private static void Decrypt(byte[] data, string key)
        {
            string[] bytes = key.Split(' ');
            if (bytes.Length != 8) return;

            byte[] baseKey = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                baseKey[i] = byte.Parse(bytes[i], System.Globalization.NumberStyles.HexNumber);
            }

            byte[] decryptionKey = CreateDecryptionKey(baseKey);
            Decrypt(data, decryptionKey);
        }


        private static byte[] CreateDecryptionKey(byte[] baseKey)
        {
            byte[] key = new byte[256];
            for (int i = 0; i < 256; ++i) key[i] = (byte)i;
            
            byte b = 0;
            for (int i = 0; i < 256; ++i)
            {
                byte k = key[(byte)i];

                b += (byte)(baseKey[i & 7] + k);

                key[i] = key[b];
                key[b] = k;
            }

            return key;
        }


        private static void Decrypt(byte[] data, byte[] key)
        {
            byte[] localKey = key.Clone() as byte[];

            byte b = 0;            
            for (int i = 0; i < data.Length; ++i)
            {
                byte j = (byte)(i + 1);
                byte k = localKey[j];

                b += k;

                localKey[j] = localKey[b];
                localKey[b] = k;
                
                data[i] ^= localKey[(byte)(localKey[j] + k)];
            }
        }
    }
}

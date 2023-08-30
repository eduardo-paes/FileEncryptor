using System.Security.Cryptography;
using CLI.Interfaces;

namespace CLI.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key;
        public CryptoService(byte[] key)
        {
            _key = AdjustKeySize(key);
        }

        public void EncryptFile(string inputFile, string outputFile)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = _key;
            aesAlg.BlockSize = 128;
            aesAlg.GenerateIV();
            aesAlg.Mode = CipherMode.CFB;
            aesAlg.Padding = PaddingMode.PKCS7;

            using FileStream fsInput = new(inputFile, FileMode.Open, FileAccess.Read);
            using FileStream fsOutput = new(outputFile, FileMode.Create, FileAccess.Write);
            using ICryptoTransform encryptor = aesAlg.CreateEncryptor();
            using CryptoStream csEncrypt = new(fsOutput, encryptor, CryptoStreamMode.Write);
            fsOutput.Write(aesAlg.IV, 0, aesAlg.IV.Length);

            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0)
            {
                csEncrypt.Write(buffer, 0, bytesRead);
            }
        }

        public void DecryptFile(string inputFile, string outputFile)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = _key;
            aesAlg.BlockSize = 128;
            aesAlg.Mode = CipherMode.CFB;
            aesAlg.Padding = PaddingMode.PKCS7;

            using FileStream fsInput = new(inputFile, FileMode.Open, FileAccess.Read);
            using FileStream fsOutput = new(outputFile, FileMode.Create, FileAccess.Write);
            byte[] iv = new byte[aesAlg.IV.Length];
            fsInput.Read(iv, 0, iv.Length);
            aesAlg.IV = iv;

            using ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            using CryptoStream csDecrypt = new CryptoStream(fsOutput, decryptor, CryptoStreamMode.Write);
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0)
            {
                csDecrypt.Write(buffer, 0, bytesRead);
            }
        }

        private static byte[] AdjustKeySize(byte[] key)
        {
            using Aes aesAlg = Aes.Create();
            int validKeySize = aesAlg.LegalKeySizes[0].MinSize;

            if (key.Length * 8 < validKeySize)
            {
                int bytesToAdd = validKeySize / 8 - key.Length;
                byte[] adjustedKey = new byte[validKeySize / 8];
                Buffer.BlockCopy(key, 0, adjustedKey, bytesToAdd, key.Length);
                return adjustedKey;
            }
            else if (key.Length * 8 > validKeySize)
            {
                byte[] adjustedKey = new byte[validKeySize / 8];
                Buffer.BlockCopy(key, 0, adjustedKey, 0, adjustedKey.Length);
                return adjustedKey;
            }

            return key;
        }
    }
}
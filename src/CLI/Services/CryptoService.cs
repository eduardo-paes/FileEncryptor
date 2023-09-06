using System.Security.Cryptography;
using System.Text;
using CLI.Interfaces;

namespace CLI.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _key;
        public CryptoService(string key)
        {
            // The key must be 16, 24 or 32 bytes long.
            _key = AdjustKeySize(Encoding.UTF8.GetBytes(key));
        }

        /// <summary>
        /// Encrypts a file using the key provided in the constructor.
        /// </summary>
        /// <param name="inputFile">The file to be encrypted.</param>
        /// <param name="outputFile">The file to be created.</param>
        /// <exception cref="CryptographicException">Thrown when the key is invalid.</exception>
        /// <exception cref="IOException">Thrown when the file is corrupted.</exception>
        public void EncryptFile(string inputFile, string outputFile)
        {
            using Aes aesAlg = CreateAes();
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

        /// <summary>
        /// Decrypts a file using the key provided in the constructor.
        /// </summary>
        /// <param name="inputFile">The file to be decrypted.</param>
        /// <param name="outputFile">The file to be created.</param>
        /// <exception cref="CryptographicException">Thrown when the key is invalid.</exception>
        /// <exception cref="IOException">Thrown when the file is corrupted.</exception>
        public void DecryptFile(string inputFile, string outputFile)
        {
            using Aes aesAlg = CreateAes();
            using FileStream fsInput = new(inputFile, FileMode.Open);
            using FileStream fsOutput = new(outputFile, FileMode.OpenOrCreate, FileAccess.Write);

            byte[] iv = new byte[aesAlg.IV.Length];
            fsInput.Read(iv, 0, iv.Length);
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            using CryptoStream csDecrypt = new(fsOutput, decryptor, CryptoStreamMode.Write);
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0)
                {
                    csDecrypt.Write(buffer, 0, bytesRead);
                }
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="Aes"/> with the key provided in the constructor.
        /// </summary>
        /// <param name="generateIV">If true, generates a new IV for the instance.</param>
        /// <returns>An instance of <see cref="Aes"/>.</returns>
        private Aes CreateAes()
        {
            Aes aesAlg = Aes.Create();
            aesAlg.Key = _key;
            aesAlg.BlockSize = 128;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.GenerateIV();
            return aesAlg;
        }

        /// <summary>
        /// Adjusts the key size to the minimum size required by the <see cref="Aes"/> algorithm.
        /// </summary>
        /// <param name="key">The key to be adjusted.</param>
        /// <returns>The adjusted key.</returns> 
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
using FileEncryptor.Interfaces;
using FileEncryptor.Services;

namespace FileEncryptor.Factories
{
    public class CryptoServiceFactory
    {
        public static ICryptoService Create(string key)
        {
            return new CryptoService(key);
        }
    }
}
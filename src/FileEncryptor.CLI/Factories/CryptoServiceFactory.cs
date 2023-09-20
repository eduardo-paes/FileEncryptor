using FileEncryptor.CLI.Interfaces;
using FileEncryptor.CLI.Services;

namespace FileEncryptor.CLI.Factories
{
    public class CryptoServiceFactory
    {
        public static ICryptoService Create(string key)
        {
            return new CryptoService(key);
        }
    }
}
using CLI.Interfaces;
using CLI.Services;

namespace CLI.Factories
{
    public class CryptoServiceFactory
    {
        public static ICryptoService Create(byte[] key)
        {
            return new CryptoService(key);
        }
    }
}
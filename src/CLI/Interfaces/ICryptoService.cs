namespace CLI.Interfaces
{
    public interface ICryptoService
    {
        void EncryptFile(string inputFile, string outputFile);
        void DecryptFile(string inputFile, string outputFile);
    }
}
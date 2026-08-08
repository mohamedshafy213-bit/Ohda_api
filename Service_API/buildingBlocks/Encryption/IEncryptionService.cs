namespace Service_API.Services.Encryption
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText, out string iv);
        string Decrypt(string cipherText, string iv);
    }
}

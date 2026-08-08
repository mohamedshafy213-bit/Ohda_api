using System.Security.Cryptography;

namespace Service_API.Services.Encryption
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public AesEncryptionService(string base64Key)
        {
            if (string.IsNullOrEmpty(base64Key))
                throw new ArgumentNullException(nameof(base64Key), "Base64 key cannot be null or empty.");

            try
            {
                _key = Convert.FromBase64String(base64Key);
                if (_key.Length != 32) // AES-256 requires a 32-byte key
                    throw new ArgumentException("Key must be 256 bits (32 bytes).", nameof(base64Key));
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid Base64 key format.", nameof(base64Key));
            }
        }
        public string Encrypt(string plainText, out string iv)
        {
           
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.GenerateIV();
                iv = Convert.ToBase64String(aesAlg.IV);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }
        public string Decrypt(string cipherText, string iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = Convert.FromBase64String(iv);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}

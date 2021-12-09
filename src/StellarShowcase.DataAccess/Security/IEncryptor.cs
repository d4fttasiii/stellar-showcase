namespace StellarShowcase.DataAccess.Security
{
    public interface IEncryptor
    {
        string EncryptString(string toEncrypt, string password);
        string DecryptToString(string encryptedText, string password);
    }
}

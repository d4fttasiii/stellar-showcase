using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace StellarShowcase.DataAccess.Security
{
    internal class Encryptor : IEncryptor
    {
        private const int _aesBlockByteSize = 128 / 8;
        private const int _passwordSaltByteSize = 128 / 8;
        private const int _passwordByteSize = 256 / 8;
        private const int _passwordIterationCount = 100_000;
        private const int _signatureByteSize = 256 / 8;
        private const int _minimumEncryptedMessageByteSize =
            _passwordSaltByteSize + // auth salt
            _passwordSaltByteSize + // key salt
            _aesBlockByteSize + // IV
            _aesBlockByteSize + // cipher text min length
            _signatureByteSize; // signature tag

        private static readonly Encoding _stringEncoding = Encoding.UTF8;
        private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

        public Encryptor() { }

        public string EncryptString(string toEncrypt, string password)
        {
            // encrypt
            var keySalt = GenerateRandomBytes(_passwordSaltByteSize);
            var key = GetKey(password, keySalt);
            var iv = GenerateRandomBytes(_aesBlockByteSize);

            byte[] cipherText;
            using (var aes = CreateAes())
            using (var encryptor = aes.CreateEncryptor(key, iv))
            {
                var plainText = _stringEncoding.GetBytes(toEncrypt);
                cipherText = encryptor
                    .TransformFinalBlock(plainText, 0, plainText.Length);
            }

            // sign
            var authKeySalt = GenerateRandomBytes(_passwordSaltByteSize);
            var authKey = GetKey(password, authKeySalt);

            var result = MergeArrays(
                additionalCapacity: _signatureByteSize,
                authKeySalt, keySalt, iv, cipherText);

            using (var hmac = new HMACSHA256(authKey))
            {
                var payloadToSignLength = result.Length - _signatureByteSize;
                var signatureTag = hmac.ComputeHash(result, 0, payloadToSignLength);
                signatureTag.CopyTo(result, payloadToSignLength);
            }

            return Convert.ToBase64String(result);
        }

        public string DecryptToString(string encryptedText, string password)
        {
            var encryptedData = Convert.FromBase64String(encryptedText);

            if (encryptedData is null
                || encryptedData.Length < _minimumEncryptedMessageByteSize)
            {
                throw new ArgumentException("Invalid length of encrypted data");
            }

            var authKeySalt = encryptedData
                .AsSpan(0, _passwordSaltByteSize).ToArray();
            var keySalt = encryptedData
                .AsSpan(_passwordSaltByteSize, _passwordSaltByteSize).ToArray();
            var iv = encryptedData
                .AsSpan(2 * _passwordSaltByteSize, _aesBlockByteSize).ToArray();
            var signatureTag = encryptedData
                .AsSpan(encryptedData.Length - _signatureByteSize, _signatureByteSize).ToArray();

            var cipherTextIndex = authKeySalt.Length + keySalt.Length + iv.Length;
            var cipherTextLength =
                encryptedData.Length - cipherTextIndex - signatureTag.Length;

            var authKey = GetKey(password, authKeySalt);
            var key = GetKey(password, keySalt);

            // verify signature
            using (var hmac = new HMACSHA256(authKey))
            {
                var payloadToSignLength = encryptedData.Length - _signatureByteSize;
                var signatureTagExpected = hmac
                    .ComputeHash(encryptedData, 0, payloadToSignLength);

                // constant time checking to prevent timing attacks
                var signatureVerificationResult = 0;
                for (int i = 0; i < signatureTag.Length; i++)
                {
                    signatureVerificationResult |= signatureTag[i] ^ signatureTagExpected[i];
                }

                if (signatureVerificationResult != 0)
                {
                    throw new UnauthorizedAccessException("Invalid signature");
                }
            }

            // decrypt
            using var aes = CreateAes();
            using var encryptor = aes.CreateDecryptor(key, iv);

            try
            {
                var decryptedBytes = encryptor
                    .TransformFinalBlock(encryptedData, cipherTextIndex, cipherTextLength);

                return _stringEncoding.GetString(decryptedBytes);
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Invalid passphrase");
            }
        }

        private static byte[] GetKey(string password, byte[] passwordSalt)
        {
            var keyBytes = _stringEncoding.GetBytes(password);

            using var derivator = new Rfc2898DeriveBytes(
                keyBytes, passwordSalt,
                _passwordIterationCount, HashAlgorithmName.SHA256);

            return derivator.GetBytes(_passwordByteSize);
        }

        private static byte[] GenerateRandomBytes(int numberOfBytes)
        {
            var randomBytes = new byte[numberOfBytes];
            _random.GetBytes(randomBytes);
            return randomBytes;
        }

        private static byte[] MergeArrays(int additionalCapacity = 0, params byte[][] arrays)
        {
            var merged = new byte[arrays.Sum(a => a.Length) + additionalCapacity];
            var mergeIndex = 0;
            for (int i = 0; i < arrays.GetLength(0); i++)
            {
                arrays[i].CopyTo(merged, mergeIndex);
                mergeIndex += arrays[i].Length;
            }

            return merged;
        }

        private static Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
    }
}

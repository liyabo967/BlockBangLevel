// AesEncryptor.cs

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UnityGameFramework.Runtime
{
    public sealed class AesEncryptor
    {
        private const int SALT_SIZE = 16;
        private const int IV_SIZE = 16;
        private const int KEY_SIZE = 32; // AES-256
        private const int PBKDF2_ITERATIONS = 10000;

        // 缓存的密钥和 salt，构造时派生一次后复用
        private readonly byte[] _key;
        private readonly byte[] _salt;

        /// <summary>
        /// 用密码初始化加密器。密钥派生只发生在这里。
        /// </summary>
        /// <param name="password">加密密码</param>
        /// <param name="salt">固定盐值（16 字节）。必须固定，否则解不开旧数据。</param>
        public AesEncryptor(string password, byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password 不能为空");
            if (salt == null || salt.Length != SALT_SIZE)
                throw new ArgumentException($"salt 必须是 {SALT_SIZE} 字节");

            _salt = (byte[])salt.Clone();

            // ★ 密钥派生只发生一次
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password, _salt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256);
            _key = pbkdf2.GetBytes(KEY_SIZE);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create();
            aes.Key = _key; // 直接用缓存的密钥
            aes.GenerateIV(); // IV 每次都要新的

            using var ms = new MemoryStream();
            // 把 salt 和 IV 写在密文前面（salt 写入是为了兼容性，方便未来升级）
            ms.Write(_salt, 0, _salt.Length);
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] fullCipher = Convert.FromBase64String(cipherText);
            if (fullCipher.Length < SALT_SIZE + IV_SIZE)
                throw new CryptographicException("密文长度不合法");

            // 取出前置的 IV（salt 跳过，因为我们用的是构造时的 salt）
            byte[] iv = new byte[IV_SIZE];
            Buffer.BlockCopy(fullCipher, SALT_SIZE, iv, 0, IV_SIZE);

            using var aes = Aes.Create();
            aes.Key = _key; // 直接用缓存的密钥
            aes.IV = iv;

            using var ms = new MemoryStream(
                fullCipher, SALT_SIZE + IV_SIZE, fullCipher.Length - SALT_SIZE - IV_SIZE);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
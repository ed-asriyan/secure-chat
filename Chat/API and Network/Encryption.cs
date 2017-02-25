using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Numerics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;

namespace Chat
{
    /// <summary>
    /// Обеспечивает шифрование
    /// </summary>
    class Encryption
    {
        #region RSA

        static RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
        static RSACryptoServiceProvider rsaRecieve = new RSACryptoServiceProvider(2048);
        static byte[] rsaPubicKeys = Utility.Text.Encoding.GetBytes(rsa.ToXmlString(false));

        #region Keys

        /// <summary>
        /// Возврщает публичный ключ
        /// </summary>
        /// <returns>Публичный ключ</returns>
        static public byte[] RSAGetPublicKey()
        {
            return rsaPubicKeys;
        }

        /// <summary>
        /// Расшифровывает данные
        /// </summary>
        /// <param name="chiperText">Данные для расшифровки</param>
        /// <returns>Расшифрованные данные</returns>
        static public byte[] RSADecrypt(byte[] chiperText)
        {
            lock (rsa)
            {
                return rsa.Decrypt(chiperText, false);
            }
        }

        /// <summary>
        /// Зашифровывает данные
        /// </summary>
        /// <param name="plainText">Данные, которые надо зашифровать</param>
        /// <param name="publicKey">Публичный ключ</param>
        /// <returns>Зашифрованные данные</returns>
        static public byte[] RSAEncrypt(byte[] plainText, byte[] publicKey)
        {
            lock (rsaRecieve)
            {
                rsaRecieve.FromXmlString(Utility.Text.Encoding.GetString(publicKey));
                return rsaRecieve.Encrypt(plainText, false);
            }
        }

        #endregion

        #endregion

        #region AES

        static Random random = new Random();

        /// <summary>
        /// Генеринует симметричный ключ
        /// </summary>
        /// <returns>Симметричный ключ</returns>
        static public byte[] GenerateSymmetricPassword()
        {
            byte[] result = new byte[90];
            random.NextBytes(result);
            return result;
        }

        /// <summary>
        /// Генерирует вектор инициализации
        /// </summary>
        /// <returns>Вектор инициализации</returns>
        static public byte[] GenerateSalt()
        {
            byte[] result = new byte[8];
            random.NextBytes(result);
            return result;
        }


        /// <summary>
        /// Расшифровывает данные
        /// </summary>
        /// <param name="bytesToBeDecrypted">Данные для расшифровки</param>
        /// <param name="passwordBytes">Ключ</param>
        /// <param name="salt">Вектор инициализации</param>
        /// <returns></returns>
        static public byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes, byte[] salt)
        {
            MemoryStream ms = new MemoryStream();
            RijndaelManaged AES = new RijndaelManaged();

            AES.KeySize = 256;
            AES.BlockSize = 128;

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                cs.Close();
            }

            return ms.ToArray();
        }


        /// <summary>
        /// Шифрует данные
        /// </summary>
        /// <param name="bytesToBeEncrypted">Данные для зашифровки</param>
        /// <param name="passwordBytes">Ключ</param>
        /// <param name="salt">Вектор инициализации</param>
        /// <returns>Зашифрованные данные</returns>
        static public byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes, out byte[] salt)
        {
            salt = GenerateSalt();

            MemoryStream ms = new MemoryStream();
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;

            var key = new Rfc2898DeriveBytes(passwordBytes, salt, 1000);
            AES.Key = key.GetBytes(AES.KeySize / 8);
            AES.IV = key.GetBytes(AES.BlockSize / 8);

            AES.Mode = CipherMode.CBC;

            using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                cs.Close();
            }

            return ms.ToArray();
        }

        #endregion
    }
}

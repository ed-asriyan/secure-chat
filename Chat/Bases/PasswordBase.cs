using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    /// <summary>
    /// База секретных слов
    /// </summary>
    [Serializable]
    public class PasswordsBase
    {
        static public PasswordsBase Main = new PasswordsBase();

        List<string> allowNoEncryption = new List<string>();
        Dictionary<string, byte[]> secrets = new Dictionary<string, byte[]>();

        #region Encryption need

        /// <summary>
        /// Устанавливает необходимость шифрования с данным IP
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="needed">Необходимо ли шифрование. TRUE - да, FALSE - нет</param>
        public void SetEncryptionNeed(IPAddress ipAddress, bool needed)
        {
            SetEncryptionNeed(ipAddress.ToString(), needed);
        }

        /// <summary>
        /// Устанавливает необходимость шифрования с данным IP
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="needed">Необходимо ли шифрование. TRUE - да, FALSE - нет</param
        public void SetEncryptionNeed(string ipAddress, bool needed)
        {
            lock (this.allowNoEncryption)
            {
                if (!this.allowNoEncryption.Contains(ipAddress))
                {
                    if (!needed)
                    {
                        this.allowNoEncryption.Add(ipAddress);
                    }
                }
                else
                {
                    if (needed)
                    {
                        this.allowNoEncryption.Remove(ipAddress);
                    }
                }
            }
        }

        /// <summary>
        /// Определяет необходимо ли шифрование с данным IP
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Булево значение</returns>
        public bool EncryptionIsNeed(IPAddress ipAddress)
        {
            return EncryptionIsNeed(ipAddress.ToString());
        }

        /// <summary>
        /// Определяет необходимо ли шифрование с данным IP
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Булево значение</returns>
        public bool EncryptionIsNeed(string ipAddress)
        {
            lock (this.allowNoEncryption)
            {
                return !this.allowNoEncryption.Contains(ipAddress);
            }
        }

        #endregion

        #region Secret

        /// <summary>
        /// Устанавливает секретное слово для данного IP
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="secret">Секретное слово в виде байт</param>
        public void SetSecret(IPAddress ipAddress, byte[] secret)
        {
            var ipStr = ipAddress.ToString();

            lock (secrets)
            {
                if (secrets.ContainsKey(ipStr))
                {
                    secrets[ipStr] = secret;
                }
                else
                {
                    secrets.Add(ipStr, secret);
                }
            }
        }

        /// <summary>
        /// Удаляет секретное слово из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void RemoveSecret(IPAddress ipAddress)
        {
            var ipStr = ipAddress.ToString();

            lock (secrets)
            {
                if (secrets.ContainsKey(ipStr))
                {
                    secrets.Remove(ipStr);
                }
            }
        }

        /// <summary>
        /// Получает секрутное слово
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Секретное слово в виде байт</returns>
        public byte[] GetSecret(IPAddress ipAddress)
        {
            var ipStr = ipAddress.ToString();

            lock (secrets)
            {
                if (secrets.ContainsKey(ipStr))
                {
                    return secrets[ipStr];
                }
                else return Settings.Global.DefaultSecretKey == null ? null : Utility.Text.Encoding.GetBytes(Settings.Global.DefaultSecretKey);
            }
        }

        #endregion
    }
}

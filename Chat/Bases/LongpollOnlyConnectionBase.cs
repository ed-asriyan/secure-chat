using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    /// <summary>
    /// База IP-адресов, к которым надо подключаться только по longpoll
    /// </summary>
    [Serializable]
    class LongpollOnlyConnectionBase
    {
        static public LongpollOnlyConnectionBase Global = new LongpollOnlyConnectionBase();

        List<string> list = new List<string>();

        #region Add methods

        /// <summary>
        /// Добавляет IP-адрес в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Add(string ipAddress)
        {
            lock (list)
            {
                if (!list.Contains(ipAddress)) list.Add(ipAddress);
            }
        }


        /// <summary>
        /// Добавляет IP-адрес в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Add(IPAddress ipAddress)
        {
            Add(ipAddress.ToString());
        }

        #endregion

        #region Remove methods

        /// <summary>
        /// Удаляет IP-адрес из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Remove(string ipAddress)
        {
            lock (list)
            {
                if (list.Contains(ipAddress)) list.Remove(ipAddress);
            }
        }

        /// <summary>
        /// Удаляет IP-адрес из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Remove(IPAddress ipAddress)
        {
            Remove(ipAddress.ToString());
        }

        #endregion

        #region Check methods

        /// <summary>
        /// Проверяет наличие IP-адреса в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Булево значение</returns>
        public bool Contains(string ipAddress)
        {
            lock (list)
            {
                return list.Contains(ipAddress);
            }
        }

        /// <summary>
        /// Проверяет наличие IP-адреса в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Булево значение</returns>
        public bool Contains(IPAddress ipAddress)
        {
            return Contains(ipAddress.ToString());
        }

        #endregion
    }
}

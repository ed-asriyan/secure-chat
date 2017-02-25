using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    /// <summary>
    /// База соединений
    /// </summary>
    [Serializable]
    public class ConnectionBase
    {
        static public ConnectionBase Main = new ConnectionBase();

        List<string> list = new List<string>();

        #region Add/remove methods

        /// <summary>
        /// Добавляет соединение в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Add(IPAddress ipAddress)
        {
            lock (list)
            {
                if (!list.Contains(ipAddress.ToString()))
                {
                    list.Add(ipAddress.ToString());
                }
            }
        }

        /// <summary>
        /// Удаляет соединение в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Результат удаления</returns>
        public bool Remove(IPAddress ipAddress)
        {
            lock (list)
            {
                if (list.Contains(ipAddress.ToString()))
                {
                    list.Remove(ipAddress.ToString());
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Check methods

        /// <summary>
        /// Разрешено ли соединение с данным IP-адресом
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>Булево значение</returns>
        public bool AllowConnection(IPAddress ipAddress)
        {
            lock (list)
            {
                return Settings.Global.recieveConnectionsFromOthers || list.Contains(ipAddress.ToString());
            }
        }

        /// <summary>
        /// Содержет ли база данный IP
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Булево значение</returns>
        public bool Contains(IPAddress ipAddress)
        {
            lock (list)
            {
                return list.Contains(ipAddress.ToString());
            }
        }

        #endregion

        #region Get methods

        /// <summary>
        /// Список всех соединений
        /// </summary>
        /// <returns>Массив</returns>
        public string[] AllConnections()
        {
            return this.list.ToArray();
        }

        #endregion
    }
}

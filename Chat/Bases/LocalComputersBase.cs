using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    /// <summary>
    /// База компьютеров в локаьной сети
    /// </summary>
    class LocalComputersBase
    {
        static public LocalComputersBase Global = new LocalComputersBase();

        List<string> list = new List<string>();

        #region Add methods

        /// <summary>
        /// Добавляет компьютер в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Add(string ipAddress)
        {
            lock (this.list)
            {
                if (!this.Contains(ipAddress))
                {
                    this.list.Add(ipAddress);
                }
            }
        }

        /// <summary>
        /// Добавляет компьютер в базу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Add(IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");

            this.Add(ipAddress.ToString());
        }

        /// <summary>
        /// Добавляет компьютеры в базу
        /// </summary>
        /// <param name="ipAddresses">IP-адресы</param>
        public void Add(IEnumerable<string> ipAddresses)
        {
            if (ipAddresses == null) throw new ArgumentNullException("ipAddresses");

            foreach (var ipAddress in ipAddresses)
            {
                this.Add(ipAddress);
            }
        }

        /// <summary>
        /// Добавляет компьютеры в базу
        /// </summary>
        /// <param name="ipAddresses">IP-адресы</param>
        public void Add(IEnumerable<IPAddress> ipAddresses)
        {
            if (ipAddresses == null) throw new ArgumentNullException("ipAddresses");

            foreach (var ipAddress in ipAddresses)
            {
                this.Add(ipAddress);
            }
        }

        #endregion

        #region Remove methods

        /// <summary>
        /// Удаляет компьютер из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Remove(string ipAddress)
        {
            lock (this.list)
            {
                if (this.Contains(ipAddress))
                {
                    this.list.Remove(ipAddress);
                }
            }
        }

        /// <summary>
        /// Удаляет компьютер из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void Remove(IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            this.Remove(ipAddress.ToString());
        }

        #endregion

        #region Check methods

        /// <summary>
        /// Проверяет наличие компьютера в базе
        /// </summary>
        /// <param name="ipAddress">IP-адрес компьютера</param>
        /// <returns>Булево значение</returns>
        public bool Contains(string ipAddress)
        {
            lock (this.list)
            {
                return this.list.Contains(ipAddress);
            }
        }

        /// <summary>
        /// Проверяет наличие компьютера в базе
        /// </summary>
        /// <param name="ipAddress">IP-адрес компьютера</param>
        /// <returns>Булево значение</returns>
        public bool Contains(IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            return this.Contains(ipAddress.ToString());
        }

        #endregion

        #region Update

        /// <summary>
        /// Обновляет список компьютеров
        /// </summary>
        /// <returns>Массив из IP-адресов</returns>
        public IPAddress[] Update()
        {
            var ips = LocalNetwork.GetLocalIPs();

            if (ips != null)
            {
                this.Add(ips);
            }

            return ips;
        }

        #endregion

        #region Get methods

        /// <summary>
        /// Список IP-адресов
        /// </summary>
        /// <returns>Список IP-адресов</returns>
        public string[] GetList()
        {
            lock (this.list)
            {
                return this.list.ToArray();
            }
        }

        #endregion
    }
}

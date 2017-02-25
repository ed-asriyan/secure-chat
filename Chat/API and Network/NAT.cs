using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Nat;

using System.Net;

namespace Chat
{
    /// <summary>
    /// Позволяет работать с сетевым экраном
    /// </summary>
    static class NAT
    {
        const string PROTOCOL = "TCP";

        static INatDevice device = null; // найденное устройство

        /// <summary>
        /// Найдено ли устройство
        /// </summary>
        static public bool Ready { get { return device != null; } }

        /// <summary>
        /// Поиск устройства
        /// </summary>
        /// <param name="onEnd"></param>
        static public void Discover(Action onEnd = null)
        {
            device = null;
            try
            {
                NatUtility.StartDiscovery();
                NatUtility.DeviceFound += delegate(object sender, DeviceEventArgs e)
                {
                    try
                    {
                        device = e.Device;

                        NatUtility.StopDiscovery();

                    }
                    catch { }
                    if (onEnd != null) onEnd();
                };
            }
            catch { }
        }

        /// <summary>
        /// Список всех пробросов
        /// </summary>
        /// <returns>Список все пробросов</returns>
        static Mapping[] GetAllMappings()
        {
            if (!Ready) return null;

            return device.GetAllMappings();
        }

        /// <summary>
        /// Проверяет, проброшен ли прот
        /// </summary>
        /// <param name="externalPort">Внешний порт</param>
        /// <param name="internalPort">Локальный порт</param>
        /// <returns>Результат проверки</returns>
        static public bool IsMapped(UInt16 externalPort, UInt16 internalPort)
        {
            try
            {
                return GetAllMappings().Where(x => x.PrivatePort == internalPort && x.PublicPort == externalPort).Count() != 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// Пробрасывает порт
        /// </summary>
        /// <param name="externalPort">Внешний порт</param>
        /// <param name="internalPort">Локальный порт</param>
        /// <returns>Результат проброса</returns>
        static public bool ForwardPort(UInt16 externalPort, UInt16 internalPort)
        {
            if (!Ready) return false;

            Mapping[] mappings = GetAllMappings();
            foreach (var mapping in mappings)
            {
                if (mapping.PublicPort == externalPort) return false;
            }

            device.CreatePortMap(new Mapping(Protocol.Tcp, internalPort, externalPort, "Secure chat"));

            return true;
        }

        /// <summary>
        /// Удаляет проброс порта
        /// </summary>
        /// <param name="externalPort">Внешний порт</param>
        /// <param name="internalPort">Локальный порт</param>
        /// <returns>Результат удаления</returns>
        static public bool DeleteForwardingRule(UInt16 externalPort, UInt16 internalPort)
        {
            if (!Ready) return false;

            Mapping[] mappings = GetAllMappings();
            bool result = false;
            foreach (var mapping in mappings)
            {
                if (mapping.PublicPort == externalPort && mapping.PrivatePort == internalPort) result = true;
            }

            if (!result) return false;

            device.DeletePortMap(new Mapping(Protocol.Tcp, internalPort, externalPort));
            
            return true;
        }

        /// <summary>
        /// Получает внешний IP
        /// </summary>
        /// <returns>Внешний IP</returns>
        static public IPAddress GetExternalIP()
        {
            return Ready ? device.GetExternalIP() : null;
        }

        /// <summary>
        /// Получает локальнй IP
        /// </summary>
        /// <returns>Локальный IP</returns>
        static public IPAddress GetInternalIP()
        {
            var ip = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
            return ip.Length > 0 ? ip[0] : null;
        }
    }
}

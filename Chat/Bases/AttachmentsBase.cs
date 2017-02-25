using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
//using System.

namespace Chat
{
    /// <summary>
    /// База отправленных (прикреплённых) файлов
    /// </summary>
    [Serializable]
    class AttachmentsBase
    {
        static public AttachmentsBase Main = new AttachmentsBase();

        static readonly string AttachmentPath = System.IO.Directory.GetCurrentDirectory() + @"\Загрузки";

        readonly Dictionary<string, Dictionary<string, Attachment>> attachments = new Dictionary<string, Dictionary<string, Attachment>>();

        #region Check

        /// <summary>
        /// Проверяет наличие IP-адреса в базе
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>Булево значение</returns>
        public bool ContainsIP(IPAddress ipAddress)
        {
            return ContainsIP(ipAddress.ToString());
        }

        /// <summary>
        /// Проверяет наличие IP-адреса в базе
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>Булево значение</returns>
        public bool ContainsIP(string ipAddress)
        {
            lock (attachments)
            {
                return attachments.ContainsKey(ipAddress);
            }
        }

        #endregion

        #region Add IP

        /// <summary>
        /// Добавляет IP-адрес в словарь
        /// </summary>
        /// <param name="ipAdress"></param>
        /// <returns></returns>
        Dictionary<string, Attachment> AddIPDictionary(string ipAdress)
        {
            lock (attachments)
            {
                if (!ContainsIP(ipAdress)) attachments.Add(ipAdress, new Dictionary<string, Attachment>());
                return attachments[ipAdress];
            }
        }

        /// <summary>
        /// Добавляет IP-адрес в словарь
        /// </summary>
        /// <param name="ipAdress"></param>
        /// <returns></returns>
        Dictionary<string, Attachment> AddIPDictionary(IPAddress ipAdress)
        {
            return AddIPDictionary(ipAdress.ToString());
        }

        #endregion

        #region Register attachment

        /// <summary>
        /// Добавляет файл в базу
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="path"></param>
        /// <returns>Объект файла</returns>
        public Attachment RegisterAttachment(IPAddress ipAddress, string path)
        {
            lock (attachments)
            {
                var dictionary = AddIPDictionary(ipAddress);

                Attachment ANew = new Attachment();
                System.IO.FileInfo info = new System.IO.FileInfo(path);
                ANew.size = info.Length;
                ANew.Name = System.IO.Path.GetFileName(path);

                ANew.IP = ipAddress.ToString();
                ANew.Path = path;

                byte[] idByte = Utility.RandomBytes(100);

                ANew.Id = System.Text.Encoding.UTF8.GetString(idByte);

                dictionary.Add(ANew.Id, ANew);

                return ANew;
            }
        }

        #endregion

        #region Unregister attachments

        /// <summary>
        /// Удаляет все файлы, прикреплённые к определённому IP-адресу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void UnregisterAllAttachments(string ipAddress)
        {
            if (!ContainsIP(ipAddress)) return;

            attachments.Remove(ipAddress);
        }

        /// <summary>
        /// Удаляет все файлы, прикреплённые к определённому IP-адресу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        public void UnregisterAllAttachments(IPAddress ipAddress)
        {
            UnregisterAllAttachments(ipAddress.ToString());
        }

        /// <summary>
        /// Удаляет файл из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="id">Идентификатор</param>
        public void UnregisterAttachment(IPAddress ipAddress, string id){
            UnregisterAttachment(ipAddress.ToString(), id);
        }

        /// <summary>
        /// Удаляет файл из базы
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="id">Идентификатор</param>
        public void UnregisterAttachment(string ipAddress, string id)
        {
            lock (attachments)
            {
                if (ContainsIP(ipAddress))
                {
                    attachments[ipAddress].Remove(id);
                }
            }
        }

        #endregion

        #region Get attachment

        /// <summary>
        /// Возвращает список всех файлов
        /// </summary>
        /// <returns>Массив объектов <see cref="Attachment"/></returns>
        public Attachment[] GetAllAttachments()
        {
            List<Attachment> result = new List<Attachment>();
            var ds = attachments.SelectMany(x => x.Value);

            foreach (var ass in ds)
            {
                result.Add(ass.Value);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Возвращает объект <see cref="Attachment"/>, прикреплённый к определённому IP-адресу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="id">Идентификатор</param>
        /// <returns>Объект <see cref="Attachment"/></returns>
        public Attachment GetRegisteredAttachmentById(string ipAddress, string id)
        {
            lock (attachments)
            {
                if (!attachments.ContainsKey(ipAddress)) return null;

                var dictionary = AddIPDictionary(ipAddress);

                if (!dictionary.ContainsKey(id)) return null;

                return dictionary[id];
            }
        }

        /// <summary>
        /// Возвращает объект <see cref="Attachment"/>, прикреплённый к определённому IP-адресу
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="id">Идентификатор</param>
        /// <returns>Объект <see cref="Attachment"/></returns>
        public Attachment GetRegisteredAttachmentById(IPAddress ipAddress, string id)
        {
            return GetRegisteredAttachmentById(ipAddress.ToString(), id);
        }

        #endregion

        #region Path

        /// <summary>
        /// Возвращает путь к папке, в которую сохраняются файлы скачаные с определённого IP-адреса
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Строка, путь к папке</returns>
        public string GetPathOfIP(IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            return this.GetPathOfIP(ipAddress.ToString());
        }

        /// <summary>
        /// Возвращает путь к файлу
        /// </summary>
        /// <param name="ip">IP-адрес</param>
        /// <param name="id">Идентификатор</param>
        /// <returns>Строка, путь к файлу</returns>
        public string GetPathOfAttachmentByID(IPAddress ip, string id)
        {
            lock (attachments)
            {
                Attachment attach = GetRegisteredAttachmentById(ip, id);
                if (attach == null) return null;

                return GetPathOfIP(ip) + @"\" + id + attach.extession;
            }
        }

        /// <summary>
        /// Возвращает путь к папке, в которую сохраняются файлы скачаные с определённого IP-адреса
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <returns>Строка, путь к папке</returns>
        string GetPathOfIP(string ipAddress)
        {
            lock (attachments)
            {
                string newPath = AttachmentPath + @"\" + ipAddress;

                if (!System.IO.Directory.Exists(AttachmentPath)) System.IO.Directory.CreateDirectory(AttachmentPath);

                if (!System.IO.Directory.Exists(newPath)) System.IO.Directory.CreateDirectory(newPath);

                return newPath;
            }
        }

        #endregion
    }
}
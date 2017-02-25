using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    /// <summary>
    /// История запросов
    /// </summary>
    class RequestHistoryBase
    {
        static public RequestHistoryBase Inbox = new RequestHistoryBase();
        static public RequestHistoryBase Outbox = new RequestHistoryBase();


        List<HistoryElement> list = new List<HistoryElement>();

        #region Add

        /// <summary>
        /// Добавляет элемент в историю
        /// </summary>
        /// <param name="element">Элемент истории</param>
        /// <returns>Элемент истории</returns>
        public HistoryElement AddToHistory(HistoryElement element)
        {
            lock (this.list)
            {
                this.list.Add(element);
                return element;
            }
        }

        #endregion

        #region Get

        /// <summary>
        /// Возвращает всю историю
        /// </summary>
        /// <returns>Массив элементов истории</returns>
        public HistoryElement[] Get()
        {
            lock (this.list)
            {
                return this.list.ToArray();
            }
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Chat
{
    /// <summary>
    /// Помогает с осуществлением запросов на транспортном уровне
    /// </summary>
    class NetServer
    {
        static public NetServer MainServer = null;

        #region Events

        public delegate Stream OnRecieveHandler(object sender, RecievedDataEventArgs e);
        public event OnRecieveHandler OnRecieve;

        public delegate void OnLongPollConnectionsChangedHandler(object sender, LongpollConnectEventArgs e);
        public event OnLongPollConnectionsChangedHandler OnLongPollConnectionsChanged;

        #endregion

        #region Constructors

        public NetServer(UInt16 port)
        {
            this.Port = port;
            this.Longpoll = new longpoll(this);
        }

        #endregion

        #region Send method

        /// <summary>
        /// Отправляет данные и получает ответ на запрос
        /// </summary>
        /// <param name="ipAddress">Адрес компьютера, на который отправлять запрос</param>
        /// <param name="data">Поток отправляемыми данными</param>
        /// <param name="result">Поток, в который будет записан ответ на запрос</param>
        /// <param name="port">Порт на удалённом компьютере</param>
        /// <param name="onRecieveProcess">Метод, вызываемай во время передачи информации</param>
        public void Send(IPAddress ipAddress, Stream data, Stream result, UInt16 port, Action<object, ProcessEventArgs> onRecieveProcess = null)
        {
            if (ipAddress == null) throw new ArgumentNullException("ipAddress");
            if (data == null) throw new ArgumentNullException("data");
            if (result == null) throw new ArgumentNullException("result");

            if (Longpoll.Server.DictionaryContains(ipAddress) || LongpollOnlyConnectionBase.Global.Contains(ipAddress))
            { // если собеседник подключён в режиме лонгпул или пользователь поставил галочку на принудительной отправке в реж. лонгпул
                Longpoll.Server.Send(ipAddress, data, result, onRecieveProcess); // отправляем данные по режиму логнпул
                return;
            }
            else
            {

                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                sender.Connect(remoteEP);
                sender.ReceiveTimeout = Settings.Global.RECIEVE_TIMEOUT;
                sender.SendTimeout = Settings.Global.SEND_TIMEOUT;
                sender.ReceiveBufferSize = Settings.Global.RECIEVE_BUFFER_SIZE;
                sender.SendBufferSize = Settings.Global.SEND_BUFFER_SIZE;

                send(sender, Utility.Convent.GetBytes(false)); // говорим, что подключились в обычном режиме

                sendEncrypted(sender, data); // отправлям данные в зашифрованном режиме

                recieveEncrypted(sender, result, onRecieveProcess); // получаем данные в зашифрованном режиме
                sender.Close();
            }
        }

        #endregion

        #region Values

        /// <summary>
        /// Прослушиваемый порт
        /// </summary>
        public UInt16 Port
        {
            get;
            private set;
        }

        #endregion

        #region Longpoll

        /// <summary>
        /// Позволяет работать в режиме лонгпул
        /// </summary>
        public longpoll Longpoll { get; private set; }

        /// <summary>
        /// Содержит необходимые методы, поля и классы для работы в режиме лонгпул
        /// </summary>
        public class longpoll
        {
            #region Internal

            NetServer NetServer { get; set; } 
            public longpoll(NetServer parent)
            {
                this.NetServer = parent; // чтобы внутри класса можно было обратиться к классу, который содержит объект 'longpoll'
                this.Server = new server(this);
                this.Client = new client(this);
            }

            /// <summary>
            /// Позволяет работать серверу в режиме лонгпул
            /// </summary>
            public server Server { get; private set; }

            /// <summary>
            /// Позволяет клиенту работать в режиме лонгпул
            /// </summary>
            public client Client { get; private set; }

            #endregion

            #region Server

            /// <summary>
            /// Содержит необходимые методы, поля и классы для работы в режиме лонгпул на стороне сервера
            /// </summary>
            public class server
            {
                #region Internal

                longpoll Longpoll { get; set; }
                public server(longpoll parent)
                {
                    this.Longpoll = parent; // чтобы внутри класса можно было обратиться к классу, который содержит объект 'server'
                }

                #endregion

                /// <summary>
                /// Список <see cref="Trio"/>, которые содержат полученные ответы на отправленные запросы
                /// T1 (string) - IP-адрес
                /// T2 (int)  - идентификатор
                /// T3 (Stream) - ответ (полученные данные)
                /// </summary>
                List<Trio<string, int, Stream>> responses = new List<Trio<string, int, Stream>>();

                /// <summary>
                /// Словарь.
                /// Ключ - IP (string),
                /// Значение - ячейки (<see cref="Cells"/>){
                ///            Значение - <see cref="Trio"/>
                ///               T1 (int) - идентификатор,
                ///               T2 (Pair) - пара (<see cref="Pair"/>){
                ///                     T1 (Stream) - содержит данные для отправки,
                ///                     T2 (Stream) - содержит поток для записи ответа
                ///                     },
                ///               T3 (Action) - метод, выполняемый во время передачи информации
                ///               }.
                /// </summary>
                Dictionary<string, Cells<Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]>>> lists = new Dictionary<string, Cells<Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]>>>();

                #region Add/remove dictonary methods

                /// <summary>
                /// Добавляет новый <see cref="Cells"/> в словарь
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                public void AddInDictionary(IPAddress ipAddress)
                {
                    string ip = ipAddress.ToString();

                    lock (lists)
                    {
                        if (!lists.ContainsKey(ip))
                        {
                            lists.Add(ip, new Cells<Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]>>());
                        }

                        lists[ip].AddFreeCell();
                        if (MainServer.OnLongPollConnectionsChanged != null) MainServer.OnLongPollConnectionsChanged.BeginInvoke(this, new LongpollConnectEventArgs(ipAddress, lists[ip].Count), null, null);

                    }
                }
                
                /// <summary>
                /// Удаляет все ячейки из словаря по IP
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <returns>Результат удаления</returns>
                public bool RemoveFromDictionary(IPAddress ipAddress)
                {
                    lock (lists)
                    {
                        if (!DictionaryContains(ipAddress)) return false;

                        lists.Remove(ipAddress.ToString());
                        if (MainServer.OnLongPollConnectionsChanged != null) MainServer.OnLongPollConnectionsChanged.BeginInvoke(this, new LongpollConnectEventArgs(ipAddress, 0), null, null);
                    }
                    return true;
                }

                #endregion

                #region Containts check methods

                /// <summary>
                /// Кол-во подключённых клиентов
                /// </summary>
                /// <param name="ipAddress">Ip-адрес</param>
                /// <returns>Кол-во подключённых клиентов</returns>
                public int LongpollConnectionCount(IPAddress ipAddress)
                {
                    lock (this.lists)
                    {
                        if (!DictionaryContains(ipAddress)) return 0;

                        return lists[ipAddress.ToString()].Count;
                    }
                }

                /// <summary>
                /// Кол-во подключённых клиентов
                /// </summary>
                /// <param name="ipAddress">Ip-адрес</param>
                /// <returns>Кол-во подключённых клиентов</returns>
                public int LongpollConnectionCount(string ipAddress)
                {
                    return LongpollConnectionCount(ipAddress.ToString());
                }

                /// <summary>
                /// Проверяет наличие подключённого клиента
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <returns>Результат проверки</returns>
                public bool DictionaryContains(IPAddress ipAddress)
                {
                    lock (lists)
                    {
                        var r = lists.ContainsKey(ipAddress.ToString()) && lists[ipAddress.ToString()].Count != 0;
                        return r;
                    }
                }

                /// <summary>
                /// Проверяет наличие запроса в базе
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <param name="pair">Запрос</param>
                /// <returns>Результат проверки</returns>
                public bool DictionaryContains(IPAddress ipAddress, Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]> pair)
                {
                    lock (lists)
                    {
                        if (!DictionaryContains(ipAddress)) return false;
                        return lists[ipAddress.ToString()].Contains(pair);
                    }
                }

                #endregion

                #region Add\remove request cell

                /// <summary>
                /// Добавление запроса в очередь (ячейку)
                /// </summary>
                /// <param name="ipAddress">Ip-адрес</param>
                /// <param name="ID">Идентификатор</param>
                /// <param name="data">Данные для отпраки</param>
                /// <param name="responseStream">Поток, для записи в него ответа</param>
                /// <param name="onProccesses">Выполняется во время передачи</param>
                /// <returns>Результат добавления</returns>
                public Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]> TryAddRequest(IPAddress ipAddress, int ID, Stream data, Stream responseStream, params Action<object, ProcessEventArgs>[] onProccesses)
                {
                    lock (lists)
                    {
                        if (!DictionaryContains(ipAddress)) return null;
                        var result = new Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]>(ID, new Pair<Stream, Stream>(data, responseStream), onProccesses);
                        return lists[ipAddress.ToString()].TryAddData(result) ? result : null;
                    }
                }

                /// <summary>
                /// Удаляет запрос из базы (ячеек)
                /// </summary>
                /// <param name="ipAddress">Ip-адрес</param>
                /// <param name="sign">Запрос, который надо удалить</param>
                /// <returns>Результат удаления</returns>
                public bool RemoveRequest(IPAddress ipAddress, Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]> sign)
                {
                    lock (lists)
                    {
                        if (!this.DictionaryContains(ipAddress)) return false;
                        return lists[ipAddress.ToString()].RemoveDataFromCell(sign);
                    }
                }

                /// <summary>
                /// Удаляет запос вместе с ячеёкой
                /// </summary>
                /// <param name="ipAddress">Ip-адрес</param>
                /// <param name="ID">Идентификатор</param>
                /// <returns>Результат удаления</returns>
                public bool RemoveRequestCell(IPAddress ipAddress, int ID)
                {
                    string ipStr = ipAddress.ToString();
                    lock (lists)
                    {
                        if (!DictionaryContains(ipAddress)) return false;

                        var l = lists[ipStr].GetAllBusyData();

                        try
                        {
                            if (l.Count() == 1)
                            {
                                return this.RemoveFromDictionary(ipAddress);
                            }

                            var ll = l.First(x => x.t1 == ID);

                            lists[ipStr].RemoveCellByData(ll);

                            if (MainServer.OnLongPollConnectionsChanged != null) MainServer.OnLongPollConnectionsChanged.BeginInvoke(this, new LongpollConnectEventArgs(ipAddress, lists[ipStr].Count), null, null);

                            return true;
                        }
                        catch { return false; }
                    }
                }

                /// <summary>
                /// Удаляет свободную ячейку
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <returns>Результат удаления</returns>
                public bool RemoveFreeCell(IPAddress ipAddress)
                {
                    lock (lists)
                    {
                        if (!DictionaryContains(ipAddress)) return false;

                        if (NetServer.MainServer.OnLongPollConnectionsChanged != null) NetServer.MainServer.OnLongPollConnectionsChanged.BeginInvoke(this, new LongpollConnectEventArgs(ipAddress, lists[ipAddress.ToString()].Count - 1), null, null);

                        if (lists.Count != 1)
                        {
                            return lists[ipAddress.ToString()].RemoveFirstFreeCell();
                        }
                        else
                        {
                            return RemoveFromDictionary(ipAddress);
                        }
                    }
                }

                /// <summary>
                /// Получает первый запрос в виде тройки(id, пара потоков с исх. и получ. данными, метод для выполн. во вермя отправки)
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <returns>Тройку первого запроса. Если исх. запросов нет, возвращается null</returns>
                public Trio<int, Pair<Stream, Stream>, Action<object, ProcessEventArgs>[]> GetIDAndRequestAndRemoveCell(IPAddress ipAddress)
                {
                    lock (lists)
                    {
                        if (!DictionaryContains(ipAddress)) return null;

                        if (NetServer.MainServer.OnLongPollConnectionsChanged != null) NetServer.MainServer.OnLongPollConnectionsChanged.BeginInvoke(this, new LongpollConnectEventArgs(ipAddress, lists[ipAddress.ToString()].Count - 1), null, null);

                        try
                        {

                            var r = lists[ipAddress.ToString()].GetFirstData();
                            lists[ipAddress.ToString()].RemoveCellByData(r);
                            return r;
                        }
                        catch { return null; }
                    }
                }

                #endregion

                #region Add\remove responses

                /// <summary>
                /// Добавляет ответ в базу ответов
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <param name="ID">Идентификатор</param>
                /// <param name="data">Полученный ответ</param>
                public void AddResponse(IPAddress ipAddress, int ID, Stream data)
                {
                    lock (responses)
                    {
                        responses.Add(new Trio<string, int, Stream>(ipAddress.ToString(), ID, data));
                    }
                }

                /// <summary>
                /// Удаляет ответ с определённым идентификатором из базы ответов
                /// </summary>
                /// <param name="ipAddress">IP-адресс</param>
                /// <param name="ID">Идентификатор</param>
                public void RemoveResponse(IPAddress ipAddress, int ID)
                {
                    lock (responses)
                    {
                        responses.Remove(responses.First(x => x.t1 == ipAddress.ToString() && x.t2 == ID));
                    }
                }

                /// <summary>
                /// Получает ответ по идентификатору
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <param name="ID">Идентификтор</param>
                /// <returns>Поток, содержащий ответ. Если в базе нет ответа с заданным идент., то метод возвращает null</returns>
                public Stream GetResponse(IPAddress ipAddress, int ID)
                {
                    lock (responses)
                    {
                        try
                        {
                            return responses.First(x => x.t1 == ipAddress.ToString() && x.t2 == ID).t3;
                        }
                        catch { return null; }
                    }
                }

                #endregion

                #region Send request method

                /// <summary>
                /// Отправляет данные по режиму лонгпул
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <param name="data">Данные для отправки</param>
                /// <param name="resulStream">Поток, в который будет записан ответ</param>
                /// <param name="onProcess">Метод, вызываемый во время отправки данных</param>
                /// <returns>Результат отправки</returns>
                public bool Send(IPAddress ipAddress, Stream data, Stream resulStream, Action<object, ProcessEventArgs> onProcess = null)
                {
                    if (ipAddress == null) throw new ArgumentNullException("ipAddress");
                    if (data == null) throw new ArgumentNullException("data");
                    if (resulStream == null) throw new ArgumentNullException("resultStream");

                    string ip = ipAddress.ToString();

                    int id = Utility.RandomInt(); // идентификатор

                    int i = 0; // счётчик, показывающий кол-во совершённых попыток поллучить ответ
                    
                    Action<object, ProcessEventArgs> a = new Action<object, ProcessEventArgs>((object s, ProcessEventArgs e) =>
                    {
                        {
                            i = 0; // если данные получаются обнуляем счётчик, чтобы таймат не был достигнут
                        }
                    }); // метод будет выполняться во время передачи/получения данных

                    var request = TryAddRequest(ipAddress, id, data, resulStream, onProcess, a); // пытаемя добавить исходящий запрос в базу
                    if (request == null)
                    {
                        return false; // если ошибка добавления
                    }

                    for (i = 0; i < Settings.Global.LongpollResponseTimeout / Settings.Global.LongpollResponseCheckInterval; i++)
                    {
                        Thread.Sleep(Settings.Global.LongpollResponseCheckInterval); // спим заданное пользователем кол-во милисек.
                        if (GetResponse(ipAddress, id) != null)
                        {
                            RemoveResponse(ipAddress, id); // если ответ получен, удаляем его из базы, т.к. он больше в ней не нужен
                            return true;
                        }
                    }

                    RemoveRequest(ipAddress, request); // удаляем запрос из базы

                    throw new Exception("Истёк таймаут ожидания ответа в режиме longpoll.");
                }
            }

                #endregion

            #endregion

            #region Client

            /// <summary>
            /// Содержит необходимые методы, поля и классы для работы в режиме лонгпул на стороне клиента
            /// </summary>
            public class client
            {
                #region Internal

                longpoll Longpoll { get; set; }

                public client(longpoll parent)
                {
                    this.Longpoll = parent;
                }

                #endregion

                /// <summary>
                /// Ключ (string) - ip-адрес
                /// Значение пара:
                /// T1 - список потоков, в которых обрабатывается longpoll-соединение
                /// T2 - порт
                /// </summary>
                Dictionary<string, Pair<List<Thread>, UInt16>> list = new Dictionary<string, Pair<List<Thread>, ushort>>();

                #region Add/remove

                /// <summary>
                /// Создаёт longpoll-соедиение
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <param name="port">Порт</param>
                public void Add(IPAddress ipAddress, UInt16 port)
                {
                    lock (list)
                    {
                        Remove(ipAddress);

                        var threadList = new List<Thread>();

                        for (int i = 0; i < Settings.Global.LongpollSimultaneousThreads; i++)
                        {
                            Thread th = new Thread(new ThreadStart(() =>
                            {
                                while (IsInLongpollList(ipAddress))
                                {
                                    try
                                    {
                                        LongpollRequestBody(ipAddress, port);
                                    }
                                    catch { }
                                }
                            }));
                            th.Start();
                            threadList.Add(th);
                        }
                        list.Add(ipAddress.ToString(), new Pair<List<Thread>, ushort>(threadList, port));
                    }
                }

                /// <summary>
                /// Закрывает longpoll-соедиение
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                public void Remove(IPAddress ipAddress)
                {
                    lock (list)
                    {
                        if (IsInLongpollList(ipAddress))
                        {
                            var ipStr = ipAddress.ToString();
                            var t = list[ipStr];

                            if (t.t1 != null)
                            {
                                foreach (var thread in t.t1)
                                {
                                    if (thread.IsAlive)
                                    {
                                        thread.Abort();
                                    }
                                }
                            }

                            list.Remove(ipAddress.ToString());
                        }
                    }
                }

                /// <summary>
                /// Закрывает все longpoll-соединения
                /// </summary>
                public void RemoveAll()
                {
                    lock (this.list)
                    {
                        this.list.Clear();
                    }
                }

                #endregion

                #region Check methods

                /// <summary>
                /// Проверяет наличие подключенного соединения
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <returns>Результат проверки</returns>
                public bool IsInLongpollList(IPAddress ipAddress)
                {
                    lock (list)
                    {
                        bool a = list.ContainsKey(ipAddress.ToString());
                        return a;
                    }
                }

                #endregion

                #region Longpoll request

                /// <summary>
                /// Метод, который обрабатывает longpoll-соединение
                /// </summary>
                /// <param name="ipAddress">IP-адрес</param>
                /// <param name="port">Порт</param>
                void LongpollRequestBody(IPAddress ipAddress, UInt16 port)
                {
                    try
                    {
                        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                        Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        sender.Connect(remoteEP);
                        sender.ReceiveTimeout = Settings.Global.RECIEVE_TIMEOUT;
                        sender.SendTimeout = Settings.Global.SEND_TIMEOUT;
                        sender.ReceiveBufferSize = Settings.Global.RECIEVE_BUFFER_SIZE;
                        sender.SendBufferSize = Settings.Global.SEND_BUFFER_SIZE;

                        try
                        {
                            send(sender, Utility.Convent.GetBytes(true)); // говорим серверу, что подключаемся в режиме longpoll

                            while (this.IsInLongpollList(ipAddress))
                            { // пока данный ip есть в списке longpoll-соединений
                                if (Utility.Convent.ToBool(recieve(sender)))
                                { // ecли есть запрос
                                    MemoryStream rec = new MemoryStream(); // получаем запрос
                                    recieveEncrypted(sender, rec);

                                    Stream toSend = null; // содержит в себе данные для отправки серверу в качестве ответа
                                    if (NetServer.MainServer.OnRecieve != null)
                                    {
                                        toSend = NetServer.MainServer.OnRecieve(this, new RecievedDataEventArgs(ipAddress, rec));
                                    }
                                    else toSend = new MemoryStream(Utility.Convent.GetBytes((int)0));

                                    try
                                    {
                                        sendEncrypted(sender, toSend); // отправляем ответ
                                    }
                                    catch (Exception ex)
                                    {
                                        toSend.Close();
                                        throw ex;
                                    }

                                    toSend.Close();

                                    sender.Close();
                                    return;
                                }
                                else
                                {// если нет событий

                                    int wait = Utility.Convent.ToInt32(recieve(sender)); // получаем кол-во милисек., полученное от сервера

                                    Thread.Sleep(wait);

                                }
                            }
                            sender.Close();
                        }
                        catch
                        {
                            sender.Close();
                        }
                    }
                    catch { }
                }
            }

                #endregion

            #endregion
        }

        #endregion

        #region Connection

        private class ConnectionInfo
        {
            public Socket Socket;
            public Thread Thread;
        }

        Socket _serverSocket;

        private Thread _acceptThread;
        private List<ConnectionInfo> _connections =
            new List<ConnectionInfo>();

        private bool _isAlive = false;

        public void Start()
        {
            _isAlive = true;

            SetupServerSocket();
            _acceptThread = new Thread(AcceptConnections);
            _acceptThread.IsBackground = true;
            _acceptThread.Start();
        }

        public void Stop()
        {
            _isAlive = false;
        }

        private void SetupServerSocket()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);

            _serverSocket.Bind(localEndPoint);
            _serverSocket.Listen(10);
        }

        private void AcceptConnections()
        {
            while (_isAlive)
            {
                Socket socket = _serverSocket.Accept(); // принимаем соединение
                if (!ConnectionBase.Main.AllowConnection((socket.RemoteEndPoint as IPEndPoint).Address))
                { // если можно принимать соединения от посторонних
                    socket.Close();
                    continue;
                }
                ConnectionInfo connection = new ConnectionInfo();
                connection.Socket = socket;

                connection.Thread = new Thread(ProcessConnection); // создаем поток для получения данных
                connection.Thread.IsBackground = true;
                connection.Thread.Start(connection);

                lock (_connections) _connections.Add(connection); // сохраняем сокет
            }
        }

        private void ProcessConnection(object state)
        {
            ConnectionInfo connection = (ConnectionInfo)state;

            Socket handler = connection.Socket;

            IPAddress ipAddress = IPAddress.Parse(((IPEndPoint)handler.RemoteEndPoint).Address.ToString());

            handler.ReceiveTimeout = Settings.Global.RECIEVE_TIMEOUT;
            handler.SendTimeout = Settings.Global.SEND_TIMEOUT;
            handler.SendBufferSize = Settings.Global.SEND_BUFFER_SIZE;
            handler.SendBufferSize = Settings.Global.RECIEVE_BUFFER_SIZE;

            try
            {
                if (Utility.Convent.ToBool(recieve(handler)))
                {   // если клиент подключился в режиме longpoll
                    int id = 0; // идентификатор запроса
                    Longpoll.Server.AddInDictionary(ipAddress);
                    try
                    {
                        
                        while (true)
                        {
                            var request = Longpoll.Server.GetIDAndRequestAndRemoveCell(ipAddress); // берём первый запрос из базы
                            if (request != null) // если запрос есть
                            {
                                id = request.t1;

                                send(handler, Utility.Convent.GetBytes(true)); // говорим клиенту, что есть запрос

                                sendEncrypted(handler, request.t2.t1); // отправляем запрос

                                recieveEncrypted(handler, request.t2.t2, request.t3); // получаем ответ

                                handler.Shutdown(SocketShutdown.Both);
                                handler.Close();

                                Longpoll.Server.AddResponse(ipAddress, id, request.t2.t2); // добавляем ответ в базу
                                Longpoll.Server.RemoveRequestCell(ipAddress, id); // удаляем запрос из базы
                                return;
                            }

                            // если нет запросов
                            send(handler, Utility.Convent.GetBytes(false)); //говорим клиенту, что нет в-новых запросов

                            int wait = Settings.Global.LongpollEventWaitingInterval;
                            byte[] waitb = Utility.Convent.GetBytes(wait); 
                            send(handler, waitb); //говорим клиенту сколько ждать

                            Thread.Sleep(wait); // ждем wait секунд
                        }
                    }
                    catch
                    {
                        if (id != 0)
                        {
                            Longpoll.Server.RemoveRequestCell(ipAddress, id);
                        }
                        else
                        {
                            Longpoll.Server.RemoveFreeCell(ipAddress);
                        }
                    }

                }
                else //если обычный прием запроса
                {
                    MemoryStream recievedBytes = new MemoryStream();
                    recieveEncrypted(handler, recievedBytes); // получаем запрос

                    if (recievedBytes == null)
                    {
                        handler.Close();
                        return;
                    }

                    Stream toSend; // содержит ответ на запрос
                    if (OnRecieve != null)
                    {
                        toSend = OnRecieve(this, new RecievedDataEventArgs(ipAddress, recievedBytes));
                    }
                    else toSend = new MemoryStream(Utility.Convent.GetBytes((int)0));

                    try
                    {
                        sendEncrypted(handler, toSend); // отправляем ответ
                    }
                    catch (Exception ex)
                    {
                        toSend.Close();
                        throw ex;
                    }
                    toSend.Close();
                }
            }

            catch (Exception exc)
            {
                Console.WriteLine("Exception: " + exc);
            }
            finally
            {
                connection.Socket.Close();
                lock (_connections) _connections.Remove(
                    connection);
            }
        }

        #endregion

        #region Send/recieve methods

        /// <summary>
        /// Принимает кусок данных
        /// </summary>
        /// <param name="handler">подключённый сокет</param>
        /// <param name="onRecieveProcesses">Выполняется во вермя получения</param>
        /// <returns>Массив байт, который содерит полученный данные</returns>
        static byte[] recieve(Socket handler, params Action<object, ProcessEventArgs>[] onRecieveProcesses)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            byte[] messageLengthByte = new byte[4];
            handler.Receive(messageLengthByte); // получаем длину куска
            int messageLength = Utility.Convent.ToInt32(messageLengthByte);

            if (messageLength <= 0) throw new Exception();

            byte[] recieved = new byte[messageLength];

            const int BUFFER_SIZE = 2048;

            byte[] buffer = new byte[BUFFER_SIZE];
            Int32 bytesRecieved = 0;
            while (bytesRecieved + BUFFER_SIZE < messageLength)
            {
                int recievadBytes = handler.Receive(buffer); // получаем весь кусок по маленьким частям

                Array.Copy(buffer, 0, recieved, bytesRecieved, BUFFER_SIZE);

                bytesRecieved += recievadBytes;

                foreach (var onRecieveProcess in onRecieveProcesses)
                {
                    if (onRecieveProcess != null) onRecieveProcess.BeginInvoke(null, new ProcessEventArgs(0, messageLength, bytesRecieved), null, null);
                }
            }

            buffer = new byte[messageLength - bytesRecieved];
            handler.Receive(buffer); // получаем последнюю часть куска
            Array.Copy(buffer, 0, recieved, bytesRecieved, buffer.Length);
            foreach (var onRecieveProcess in onRecieveProcesses)
            {
                if (onRecieveProcess != null) onRecieveProcess.BeginInvoke(null, new ProcessEventArgs(0, messageLength, messageLength), null, null);
            }
            return recieved;
        }

        /// <summary>
        /// Отправляет кусок данных
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <param name="data">Отправляемый массив</param>
        static void send(Socket handler, byte[] data)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (data == null) throw new ArgumentNullException("data");

            byte[] b = Utility.Convent.GetBytes(data.Length);
            if (b.Length == 0) throw new Exception();
            handler.Send(b); // отправляем размер куска

            handler.Send(data); // отправляем сам кусок
        }

        #region Send/recieve encrypted

        #region RSA

        /// <summary>
        /// Получает данные в зашифрованном виде по протоколу RSA
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <returns>Полученные расшифрованные данные</returns>
        static byte[] recieveRSA(Socket handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            byte[] b = Encryption.RSAGetPublicKey(); // берем публичный ключ из базы
            if (b.Length == 0) throw new Exception();
            send(handler, b); // отправляем ключ

            return Encryption.RSADecrypt(recieve(handler)); // расшифровываем полученные данные
        }

        /// <summary>
        /// Отправляет данные в зашифрованном виде по алгоритму RSA
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <param name="data">Отправляемые данные</param>
        static void sendRSA(Socket handler, byte[] data)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (data == null) throw new ArgumentNullException("data");

            byte[] b = recieve(handler); // получаем публичный ключ

            if (b.Length == 0) throw new Exception();
            send(handler, Encryption.RSAEncrypt(data, b)); // зашифровываем с помощью ключа и отправляем
        }

        #endregion

        #region AES

        /// <summary>
        /// Зашифровывает по алгоритму AES и отправляет данные
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <param name="bytes">Данные для отправки</param>
        /// <param name="password">Симметричный ключ</param>
        static void sendAES(Socket handler, byte[] bytes, byte[] password)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (password == null) throw new ArgumentNullException("password");

            byte[] salt = null;
            send(handler, Encryption.Encrypt(bytes, password, out salt)); // отправляем данные
            send(handler, salt); // отправляем вектор инициализации
        }

        /// <summary>
        /// Получает и расшифровывает данные по алг. AES
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <param name="password">Симетричный ключ</param>
        /// <returns>ПОлученные расшифрованные данные</returns>
        static byte[] recieveAES(Socket handler, byte[] password)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            if (password == null) throw new ArgumentNullException("password");

            var main = recieve(handler); // получаем данные
            var salt = recieve(handler); // получаем вектор инициализации

            return Encryption.Decrypt(main, password, salt); // расшифровываем
        }

        #endregion

        /// <summary>
        /// Получает данные в зашифрованном режиме
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <param name="result">Поток, в который будет записаны полученные данные</param>
        /// <param name="onRecieveProcesses">Выполняется во время приёма</param>
        static void recieveEncrypted(Socket handler, Stream result, params Action<object, ProcessEventArgs>[] onRecieveProcesses)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (handler == null) throw new ArgumentNullException("handler");

            IPAddress ipAddress = ((IPEndPoint)handler.RemoteEndPoint).Address;

            byte[] password = PasswordsBase.Main.GetSecret(ipAddress); // берём пароль из базы

            // проверяем необходимость шифрования
            bool encryptionIsNeeded = PasswordsBase.Main.EncryptionIsNeed(ipAddress) && !(Settings.Global.DisableEncryptionWithLocalComputers && LocalComputersBase.Global.Contains(ipAddress));

            int ENCRYPTION_BLOCK = Settings.Global.EncryptionBlock; // блок шифрования

            long length = 0;

            if (password == null && encryptionIsNeeded) // если пользоваетль не ввёл секретное слово
            {
                password = Encryption.GenerateSymmetricPassword();
                sendRSA(handler, password); // отправляем симметричный ключ

                sendAES(handler, Utility.Convent.GetBytes(ENCRYPTION_BLOCK), password); // отправляем блок шифрования
                length = Utility.Convent.ToInt64(recieveAES(handler, password)); // получаем размер данных
            }
            else
            {
                if (encryptionIsNeeded) // если пользователь ввел секретное слово
                { // защита от replay-атак
                    byte[] random = Utility.RandomBytes(32);
                    send(handler, random);

                    var r = recieveAES(handler, password);
                    if (!Utility.ByteArrayCompare(r, random))
                    {
                        throw new Exception("Security check failed.");
                    }
                }

                length = Utility.Convent.ToInt64(recieve(handler));
            }

            result.SetLength(length);

            foreach (var onRecieveProcess in onRecieveProcesses)
            {
                if (onRecieveProcess != null) onRecieveProcess.BeginInvoke(null, new ProcessEventArgs(0, length, 1), null, null);
            }
            long i = 0;

            while (i < length)
            {
                byte[] dec = null;

                try
                {
                    if (encryptionIsNeeded)
                    {
                        dec = recieveAES(handler, password);
                    }
                    else
                    {
                        dec = recieve(handler);
                    } // получаем данные по частям
                }
                catch
                {

                }

                if (dec == null)
                {
                    handler.Send(new byte[] { 0 }); // говорим, что во время расшифровки произошла ошибка
                    continue;
                }
                else
                {
                    handler.Send(new byte[] { 1 }); // говорим, что расшифровка прошла успешно
                }

                result.Position = i;
                result.Write(dec, 0, dec.Length);
                i += dec.Length;

                foreach (var onRecieveProcess in onRecieveProcesses)
                {
                    if (onRecieveProcess != null) onRecieveProcess.BeginInvoke(null, new ProcessEventArgs(0, length, i), null, null);
                }
            }

            foreach (var onRecieveProcess in onRecieveProcesses)
            {
                if (onRecieveProcess != null) onRecieveProcess.BeginInvoke(null, new ProcessEventArgs(0, length, length), null, null);
            }
        }

        /// <summary>
        /// Отправляет данные в зашифрованном режиме
        /// </summary>
        /// <param name="handler">Подключённый сокет</param>
        /// <param name="data">Данные для отправки</param>
        static void sendEncrypted(Socket handler, Stream data)
        {
            IPAddress ipAddress = ((IPEndPoint)handler.RemoteEndPoint).Address;

            byte[] password = PasswordsBase.Main.GetSecret(ipAddress); // берем пароль из базы

            // проверяем, нужно ли шифрование
            bool encryptionIsNeeded = PasswordsBase.Main.EncryptionIsNeed(ipAddress) && !(Settings.Global.DisableEncryptionWithLocalComputers && LocalComputersBase.Global.Contains(ipAddress));

            // блок шифрования
            int encryptionBlock = Settings.Global.EncryptionBlock;

            if (password == null && encryptionIsNeeded)
            { // если секретное слово не введено
                password = recieveRSA(handler);  // получаем ключ

                encryptionBlock = Utility.Convent.ToInt32(recieveAES(handler, password)); // получаем размер шифрования             
                sendAES(handler, Utility.Convent.GetBytes((long)data.Length), password); // отправляем размер данных
            }
            else
            { 
                if (encryptionIsNeeded)
                {// если секр. слово введено
                    sendAES(handler, recieve(handler), password); // защита от replay-атак
                }

                send(handler, Utility.Convent.GetBytes((long)data.Length)); // отправляем размер данных
            }

            long t = (long)data.Length - encryptionBlock;
            long i = 0;
            byte[] buf;

            for (i = 0; i < t; i += encryptionBlock)
            {
                buf = new byte[encryptionBlock];

                data.Position = i;
                data.Read(buf, 0, buf.Length);

                if (encryptionIsNeeded)
                {
                    sendAES(handler, buf, password);
                }
                else
                {
                    send(handler, buf);
                } // отправляем данные по кускам

                byte[] check = new byte[1];
                handler.Receive(check); // получаем результат расшифровки

                if (check[0] != 1)
                {
                    i -= encryptionBlock;
                    continue; // если ответ отрицательный, отправляем этот же кусок ещё раз
                }
            }

            bool ch = true;

            while (ch) // отправляем самую последнюю часть данных
            {

                buf = new byte[data.Length - i];
                data.Position = i;
                data.Read(buf, 0, buf.Length);

                if (encryptionIsNeeded)
                {
                    sendAES(handler, buf, password);
                }
                else
                {
                    send(handler, buf);
                }

                byte[] c = new byte[1];
                handler.Receive(c); // получаем результат расшифровки
                ch = c[0] != 1; 
            }

        }

        #endregion

        #endregion
    }
}

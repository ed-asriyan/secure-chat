using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Ячейки
    /// </summary>
    /// <typeparam name="T">Тип элемента, который хранится в ячейке</typeparam>
    class Cells<T>
    {
        List<Pair<bool, T>> list = null; // список ячеек
                                         // bool показывает свободна ли ячейка
                                         // T - элемент в ячейке. Если 'bool' == true, T может быть null

        #region Constructors

        /// <summary>
        /// Создаёт экземпляр класса с кол-вом ячеек равным нулю
        /// </summary>
        public Cells()
        {
            list = new List<Pair<bool, T>>();
        }

        /// <summary>
        /// Создаёт экземпляр класса с определённым кол-вом ячеек
        /// </summary>
        /// <param name="cellsCount">Кол-во ячеек</param>
        public Cells(int cellsCount)
        {
            list = new List<Pair<bool, T>>();
            for (int i = 0; i < cellsCount; i++)
            {
                list.Add(new Pair<bool, T>(true));
            }
        }

        #endregion

        #region Check methods

        /// <summary>
        /// Кол-во ячеек
        /// </summary>
        public int Count
        {
            get
            {
                lock (this.list)
                {
                    return this.list.Count;
                }
            }
        }

        /// <summary>
        /// Прверят наличие элемента в ячейках
        /// </summary>
        /// <param name="data">Элемент который надо проверить</param>
        /// <returns>Булево значение</returns>
        public bool Contains(T data)
        {
            return GetFirstCellByData(data) != null;
        }

        /// <summary>
        /// Кол-во свободных ячеек
        /// </summary>
        /// <returns>Кол-во свободных ячеек</returns>
        public int GetNumberOfFreeCells()
        {
            int n = 0;
            lock (list)
            {
                n = list.Where(x => x.t1).Count();
            }
            return n;
        }

        /// <summary>
        /// Кол-во несвободных ячеек
        /// </summary>
        /// <returns>Кол-во несвободных ячеек</returns>
        public int GetNumberOfBusyCells()
        {
            lock (list)
            {
                return list.Count - GetNumberOfFreeCells();
            }
        }

        /// <summary>
        /// Проверяет наличие свободных ячеек
        /// </summary>
        /// <returns>Булево значение</returns>
        public bool HaveFreeCells()
        {
            return GetNumberOfFreeCells() != 0;
        }

        /// <summary>
        /// Проверяет наличие несвободных ячеек
        /// </summary>
        /// <returns>Булево значение</returns>
        public bool HaveBusyCells()
        {
            return GetNumberOfBusyCells() != 0;
        }

        #endregion

        #region Add cell methods

        /// <summary>
        /// Добавляет новую свободную ячеку
        /// </summary>
        public void AddFreeCell()
        {
            lock (list)
            {
                list.Add(new Pair<bool, T>(true));
            }
        }

        /// <summary>
        /// Добавляет новую несвободную ячеку
        /// </summary>
        /// <param name="item">Элемент, который будет находиться в новой ячейке</param>
        public void AddBusyCell(T item)
        {
            lock (list)
            {
                list.Add(new Pair<bool, T>(false, item));
            }
        }

        #endregion

        #region Add data methods

        /// <summary>
        /// Добавляет элемент в свободную ячейку
        /// </summary>
        /// <param name="data">Элемент, который надо добавить</param>
        /// <returns>Булево значение. Если свободных ячеек нет, FALSE, если есть TRUE</returns>
        public bool TryAddData(T data)
        {
            lock (list)
            {
                Pair<bool, T> cell = GetFirstFreeCell();
                if (cell == null) return false;

                cell.t1 = false;
                cell.t2 = data;
                return true;
            }
        }

        #endregion

        #region Get methods

        /// <summary>
        /// Возвращает массив всех элементов, которые находятся в ячейках
        /// </summary>
        /// <returns>Массив всех элементов, которые находятся в ячейках</returns>
        public T[] GetAllBusyData()
        {
            lock (list)
            {
                List<T> result = new List<T>();
                var l = list.Where(x => !x.t1);
                foreach (var t in l)
                {
                    result.Add(t.t2);
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// Возвращает элемент, находящийся в первой несвободной (занятой) ячейке
        /// </summary>
        /// <returns>Элемент, находящийся в первой несвободной (занятой) ячейке</returns>
        public T GetFirstData()
        {
            lock (list)
            {
                if (GetNumberOfBusyCells() == 0) throw new Exception("All cells are free");

                return list.First(x => !x.t1).t2;
            }
        }

        #endregion

        #region Remove methods

        /// <summary>
        /// Удаляет элемент из ячейки (освобождает ячейку, в которой находится элемент data)
        /// </summary>
        /// <param name="data">Элемент, который находится в ячейке, которую надо освободить (сделать свободной)</param>
        /// <returns>Результат освобождения ячейки</returns>
        public bool RemoveDataFromCell(T data)
        {
            lock (list)
            {
                var cell = GetFirstCellByData(data);

                if (cell == null) return false;

                cell.t1 = true;

                return true;
            }
        }

        /// <summary>
        /// Удаляет ячейку, в которой находится элемент data, из общего списка ячеек 
        /// </summary>
        /// <param name="data">Элемент, который находится в ячейке, которую надо удалить</param>
        /// <returns>Результат удаления ячейки</returns>
        public bool RemoveCellByData(T data)
        {
            lock (list)
            {
                var cell = GetFirstCellByData(data);
                if (cell == null) return false;

                list.Remove(cell);

                return true;
            }
        }

        /// <summary>
        /// Удалет первую свободную ячейку из общего списка ячеек
        /// </summary>
        /// <returns>Результат удаления ячейки</returns>
        public bool RemoveFirstFreeCell()
        {
            lock (list)
            {
                var cell = GetFirstFreeCell();
                if (cell == null) return false;

                list.Remove(cell);

                return true;
            }
        }

        /// <summary>
        /// Удаляет первую несободную (занятую) ячейку из общего списка ячеек
        /// </summary>
        /// <returns>Результат удаления ячейки</returns>
        public bool RemoveFirstBusyCell()
        {
            lock (list)
            {
                var cell = GetFirstBusyCell();
                if (cell == null) return false;

                list.Remove(cell);

                return true;
            }
        }

        #endregion

        #region Private methods

        Pair<bool, T> GetFirstFreeCell()
        {
            lock (list)
            {
                if (!HaveFreeCells()) return null;

                return list.First(x => x.t1);
            }
        }

        Pair<bool, T> GetFirstBusyCell()
        {
            lock (list)
            {
                if (!HaveBusyCells()) return null;

                return list.First(x => !x.t1);
            }
        }

        Pair<bool, T> GetFirstCellByData(T data)
        {
            lock (list)
            {
                Pair<bool, T> result = null;
                try
                {
                    result = list.First(x => x.t2.Equals(data));
                }
                catch { }
                return result;
            }
        }

        #endregion
    }
}

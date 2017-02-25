using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Net;

namespace Chat
{
    /// <summary>
    /// Позволяет осуществить поиск компьютеров в локальной сети
    /// </summary>
    static class LocalNetwork
    {
        const int MAXLEN_PHYSADDR = 8;
        
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac0;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac1;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac2;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac3;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac4;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac5;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac6;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac7;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        // Описывает метод GetIpNetTable
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(
           IntPtr pIpNetTable,
           [MarshalAs(UnmanagedType.U4)]
         ref int pdwSize,
           bool bOrder);

        [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int FreeMibTable(IntPtr plpNetTable);

        // Ошибка недостаточного размера буфера
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        /// <summary>
        /// Получает список IP-адресов локальных компьютеров средствами API
        /// </summary>
        /// <returns>Массив IP-адресов локальных компьютеров</returns>
        static public IPAddress[] GetLocalIPs()
        {
            int bytesNeeded = 0; // Кол-во необходимых байт

            int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false); // результат вызова API функции

            // функция должна вернуть ERROR_INSUFFICIENT_BUFFER, т.к. bytesNeeded = 0
            if (result != ERROR_INSUFFICIENT_BUFFER)
            {
                throw new Win32Exception(result);
            }
            
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocCoTaskMem(bytesNeeded); // Выделяем память

                result = GetIpNetTable(buffer, ref bytesNeeded, false); // Вызываем функцию ещё раз

                if (result != 0) // если произошла ошибка
                {
                    throw new Win32Exception(result);
                }

                int entries = Marshal.ReadInt32(buffer); // кол-во компьютеров

                // Инкрументируем указатель на значение значение int'а
                IntPtr currentBuffer = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(int)));

                // Allocate an array of entries.
                MIB_IPNETROW[] table = new MIB_IPNETROW[entries];

                for (int index = 0; index < entries; index++)
                {
                    // Вызываем PtrToStructure, получаем структуру с информацией
                    table[index] = (MIB_IPNETROW)Marshal.PtrToStructure(new
                       IntPtr(currentBuffer.ToInt64() + (index *
                       Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW));
                }

                List<IPAddress> ips = new List<IPAddress>();

                for (int index = 0; index < entries; index++)
                {
                    MIB_IPNETROW row = table[index];
                    IPAddress ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));

                    ips.Add(ip);
                }
                return ips.ToArray();
            }
            finally
            {
                FreeMibTable(buffer); // освобождаем память
            }
            return null;
        }
    }
}

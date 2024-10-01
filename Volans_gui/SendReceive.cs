using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

public class SendReceive
{
    public async Task Main()
    {
        Console.WriteLine("Запуск передаточной подпрограммы...");
        StartNewProgram("VolansFile.exe");
    }

    static void StartNewProgram(string programPath)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = programPath,    // Путь к исполняемому файлу другой программы
                UseShellExecute = true,    // Это позволяет открыть программу в новой консоли
                CreateNoWindow = false     // Указывает, что окно консоли должно быть видно
            };

            Process.Start(startInfo);
            Console.WriteLine($"Передаточная под программа запущена успешно.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка запуска программы {programPath}: {ex.Message}");
        }
    }
}
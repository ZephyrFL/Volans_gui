using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Threading.Tasks;

namespace Volans_gui
{
    /// <summary>
    /// Interaction logic for FileManager.xaml
    /// </summary>
    public partial class FileManager : Window
    {
        public FileManager()
        {
            InitializeComponent();
        }

        // Открытие диалога выбора файла
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        // Асинхронная отправка файла
        private async void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FilePathTextBox.Text;
            string ipAddress = IpAddressTextBox.Text;
            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                StatusTextBlock.Text = "Некорректный порт.";
                return;
            }

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                StatusTextBlock.Text = "Файл не найден.";
                return;
            }

            StatusTextBlock.Text = "Отправка файла...";
            await SendFileAsync(filePath, ipAddress, port);
        }

        // Асинхронное принятие файла
        private async void ReceiveFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                StatusTextBlock.Text = "Некорректный порт.";
                return;
            }

            StatusTextBlock.Text = "Ожидание получения файла...";
            await ReceiveFileAsync(port);
        }

        private async Task SendFileAsync(string filePath, string ipAddress, int port)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(IPAddress.Parse(ipAddress), port);
                    using (NetworkStream networkStream = tcpClient.GetStream())
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileInfo.Name);
                        byte[] fileSizeBytes = BitConverter.GetBytes(fileInfo.Length);

                        // Отправка имени файла
                        await networkStream.WriteAsync(fileNameBytes, 0, fileNameBytes.Length);
                        await networkStream.FlushAsync();
                        StatusTextBlock.Text = "Имя файла отправлено!";

                        // Отправка размера файла
                        await networkStream.WriteAsync(fileSizeBytes, 0, fileSizeBytes.Length);
                        await networkStream.FlushAsync();
                        StatusTextBlock.Text += "\nРазмер файла отправлен!";

                        // Отправка файла по частям
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                await networkStream.WriteAsync(buffer, 0, bytesRead);
                                await networkStream.FlushAsync();
                            }
                        }

                        StatusTextBlock.Text += "\nФайл успешно отправлен!";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка отправки файла: {ex.Message}";
            }
        }

        private async Task ReceiveFileAsync(int port)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                StatusTextBlock.Text = $"Ожидание подключения на порту {port}...";

                using (TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync())
                {
                    using (NetworkStream networkStream = tcpClient.GetStream())
                    {
                        // Получаем имя файла
                        byte[] fileNameBuffer = new byte[256];
                        int fileNameBytesRead = await networkStream.ReadAsync(fileNameBuffer, 0, fileNameBuffer.Length);
                        string fileName = Encoding.UTF8.GetString(fileNameBuffer, 0, fileNameBytesRead);
                        StatusTextBlock.Text = $"Получено имя файла: {fileName}";

                        // Получаем размер файла
                        byte[] fileSizeBuffer = new byte[8];
                        await networkStream.ReadAsync(fileSizeBuffer, 0, fileSizeBuffer.Length);
                        long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);
                        StatusTextBlock.Text += $"\nРазмер файла: {fileSize} байт";

                        // Получаем путь к папке "Downloads"
                        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

                        // Если папка Downloads не существует, создаем её
                        if (!Directory.Exists(downloadsPath))
                        {
                            Directory.CreateDirectory(downloadsPath);
                        }

                        // Полный путь для сохранения файла
                        string fullFilePath = Path.Combine(downloadsPath, fileName);

                        // Получаем и сохраняем файл
                        using (FileStream fs = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[1024];
                            long totalBytesReceived = 0;
                            int bytesRead;
                            while (totalBytesReceived < fileSize && (bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                fs.Write(buffer, 0, bytesRead);
                                totalBytesReceived += bytesRead;
                            }
                        }

                        StatusTextBlock.Text += $"\nФайл успешно получен и сохранен в 'Downloads' под именем {fileName}!";
                    }
                }

                tcpListener.Stop();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка приёма файла: {ex.Message}";
            }
        }

        private void PortTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void PortTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}

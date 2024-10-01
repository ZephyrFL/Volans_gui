using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace Volans_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
        public partial class MainWindow : Window
        {
            private IPAddress localIPAddress;
            private const int port = 8080; // Используемый порт

            public MainWindow()
            {
                InitializeComponent();
                IPAddress selectedIPAddress = SelectNetworkAdapter();
                if (selectedIPAddress != null)
                {
                    Task receiveTask = Task.Run(() => ReceiveMessagesAsync(port, selectedIPAddress));
                }
            }

            private async Task ReceiveMessagesAsync(int port, IPAddress localIPAddress)
            {
                using (UdpClient udpClient = new UdpClient(new IPEndPoint(localIPAddress, port)))
                {
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                    string key = "Unknow_Secure_Key";
                    AesCipher aesCipher = new AesCipher(key);
                    Dispatcher.Invoke(() => richTextBlock.Text += $"Прием сообщений на порту {port}...\n");

                    try
                    {
                        while (true)
                        {
                            UdpReceiveResult result = await udpClient.ReceiveAsync();
                            byte[] receivedData = result.Buffer;
                            string receivedMessage = Encoding.UTF8.GetString(receivedData);

                            string decryptedText = aesCipher.Decrypt(receivedMessage);

                            Dispatcher.Invoke(() =>
                            {
                                richTextBlock.Text += $"{DateTime.Now.ToShortTimeString()} Сообщение от {result.RemoteEndPoint.Address}: {decryptedText}\n";
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() => richTextBlock.Text += $"Ошибка приема: {ex.Message}\n");
                    }
                }
            }

            private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
            {
                // Если нажата клавиша Enter
                if (e.Key == Key.Enter)
                {
                    // Выполняем тот же код, что и при нажатии на кнопку
                    button1_Click(sender, e);
                    e.Handled = true; // Предотвращаем звук "бип"
                }
            }

            private async void button1_Click(object sender, RoutedEventArgs e)
            {
                if (localIPAddress == null)
                {
                    MessageBox.Show("IP-адрес не выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (UdpClient udpClient = new UdpClient(new IPEndPoint(localIPAddress, 0)))
                using (Aes myAes = Aes.Create())
                {
                    udpClient.EnableBroadcast = true;
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, port);
                    string key = "Unknow_Secure_Key";
                    AesCipher aesCipher = new AesCipher(key);

                    string message = inputTextBox.Text;

                    if (string.IsNullOrWhiteSpace(message))
                    {
                        MessageBox.Show("Введите сообщение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string encrypted = aesCipher.Encrypt(message);
                    byte[] data = Encoding.UTF8.GetBytes(encrypted);

                    try
                    {
                        await udpClient.SendAsync(data, data.Length, endPoint);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    inputTextBox.Clear();
                }
            }

            private IPAddress SelectNetworkAdapter()
            {
                // Открываем новое окно для выбора сетевого адаптера
                AdapterSelectionWindow selectionWindow = new AdapterSelectionWindow();
                bool? result = selectionWindow.ShowDialog();

                if (result == true)
                {
                    IPAddress ipAddress = selectionWindow.SelectedIPAddress;

                    if (ipAddress != null)
                    {
                        localIPAddress = ipAddress;
                        this.Title = $"Подключено: {ipAddress}";
                        return ipAddress;
                    }
                    else
                    {
                        richTextBlock.Text += "Не удалось выбрать IP-адрес.\n";
                    }
                }
                return null;
            }

            private async void button2_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    SendReceive FileManager = new SendReceive();
                    await FileManager.Main();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отправки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        }
    }
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Volans_gui
{
    /// <summary>
    /// Interaction logic for AdapterSelectionWindow.xaml
    /// </summary>
    public partial class AdapterSelectionWindow : Window
    {
        public IPAddress SelectedIPAddress { get; private set; }

        public AdapterSelectionWindow()
        {
            InitializeComponent();
            LoadNetworkAdapters();
        }


        private void LoadNetworkAdapters()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                                             .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                                           nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                             .ToArray();

            if (interfaces.Length == 0)
            {
                MessageBox.Show("Нет доступных сетевых адаптеров.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var nic in interfaces)
            {
                adapterListBox.Items.Add($"{nic.Name} - {nic.Description}");
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = adapterListBox.SelectedIndex;

            if (selectedIndex >= 0)
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                                                 .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                                               nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                                 .ToArray();

                var selectedInterface = interfaces[selectedIndex];
                var ipProperties = selectedInterface.GetIPProperties();
                var ipAddress = ipProperties.UnicastAddresses
                                            .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.Address;

                if (ipAddress != null)
                {
                    SelectedIPAddress = ipAddress;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("IP-адрес не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сетевой адаптер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

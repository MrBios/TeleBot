using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using WTelegram;

namespace TeleBot.Windows
{
    /// <summary>
    /// Логика взаимодействия для processingWindow.xaml
    /// </summary>
    public partial class processingWindow : Window
    {
        Client client = null;
        Task task;
        CancellationTokenSource token = new CancellationTokenSource();
        public processingWindow(Client _client, string _name, Task _task)
        {
            InitializeComponent();
            client = _client;
            task = _task;
            name.Content = _name;
            Task.Run(async () =>
            {
                await task;
                Application.Current.Dispatcher.Invoke(() => { Close(); });
            }, token.Token);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            token.Cancel();
        }
    }
}

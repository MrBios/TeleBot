using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WTelegram;

namespace TeleBot.Pages
{
    /// <summary>
    /// Логика взаимодействия для GetAllSecretsPage.xaml
    /// </summary>
    public partial class GetAllChatsPage : Page
    {
        Client client;
        string ids = "";
        public GetAllChatsPage(Client _client)
        {
            InitializeComponent();
            client = _client;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                Application.Current.Dispatcher.Invoke(() => { getBut.IsEnabled = false; });
                await Task.Delay(10000);
                Application.Current.Dispatcher.Invoke(() => { getBut.IsEnabled = true; });
            });
            var res = await client.Messages_GetAllDialogs();
            text.Document.Blocks.Clear();
            text.AppendText("-----\n\n");
            ids = "";
            foreach (var chat in res.users)
            {
                if (onlyChats.IsChecked.GetValueOrDefault(false) && chat.Value.IsBot) continue;
                if (onlyBots.IsChecked.GetValueOrDefault(false) && !chat.Value.IsBot) continue;
                text.AppendText($"{(chat.Value.IsBot ? "Bot" : "Chat")}\nname: {chat.Value.first_name} {chat.Value.last_name}\nid: {chat.Key.ToString()}\nusername: {chat.Value.username}\n-----\n");
                ids += chat.Key.ToString() + "\n";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText((new TextRange(text.Document.ContentStart, text.Document.ContentEnd)).Text);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ids);
        }

        private void onlyBots_Checked(object sender, RoutedEventArgs e)
        {
            onlyChats.IsChecked = false;
        }

        private void onlyChats_Checked(object sender, RoutedEventArgs e)
        {
            onlyBots.IsChecked = false;
        }
    }
}

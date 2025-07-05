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

namespace TeleBot.Pages
{
    /// <summary>
    /// Логика взаимодействия для GetAllChatsPage.xaml
    /// </summary>
    public partial class GetAllGroupsPage : Page
    {
        MainWindow inst;
        string ids = "";
        public GetAllGroupsPage(MainWindow _inst)
        {
            InitializeComponent();
            inst = _inst;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                Application.Current.Dispatcher.Invoke(() => { getBut.IsEnabled = false; });
                await Task.Delay(10000);
                Application.Current.Dispatcher.Invoke(() => { getBut.IsEnabled = true; });
            });
            var all = await inst.account.Messages_GetAllChats();
            text.Document.Blocks.Clear();
            text.AppendText("-----\n\n");
            ids = "";
            foreach (var chat in all.chats)
            {
                if (onlyChannels.IsChecked.GetValueOrDefault(false) && !chat.Value.IsChannel) continue;
                if (onlyGroups.IsChecked.GetValueOrDefault(false) && !chat.Value.IsGroup) continue;
                text.AppendText($"{(chat.Value.IsChannel ? "Channel" : "Group")}\ntitle: {chat.Value.Title}\nid: {chat.Key.ToString()}\nisBanned: {chat.Value.IsBanned().ToString()}\n-----\n");
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

        private void onlyChannels_Checked(object sender, RoutedEventArgs e)
        {
            onlyGroups.IsChecked = false;
        }

        private void onlyGrougps_Checked(object sender, RoutedEventArgs e)
        {
            onlyChannels.IsChecked = false;
        }
    }
}

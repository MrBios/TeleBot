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
    public partial class NavigationPage : Page
    {
        Client client = null;
        List<(string, Page)> pages = new List<(string, Page)>();
        public NavigationPage(Client _client)
        {
            InitializeComponent();
            this.client = _client;

            addPage("Получение групп и каналов", new GetAllGroupsPage(client));
            addPage("Получение чатов", new GetAllChatsPage(client));
            addPage("Получение сообщений", new GetMessagesPage(client));
            addPage("Массовая отправка текстовых сообщений", new MassMessagesPage(client));
            addPage("Массовая рассылка сообщений с файлов", new MassFileMessages(client));

            foreach(var page in pages)
            {
                Button butt = new Button();
                butt.Content = page.Item1;
                butt.Margin = new Thickness(10);
                butt.VerticalAlignment = VerticalAlignment.Center;
                butt.Padding = new Thickness(5);
                butt.Click += button_Click;
                
                panel.Children.Add(butt);
            }
        }

        private void addPage(string name, Page page)
        {
            pages.Add((name, page));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Content.ToString();

            NavigationService?.Navigate(pages.Find(i => { return i.Item1 == name; }).Item2);
        }
    }
}

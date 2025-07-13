using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WTelegram;

namespace TeleBot.Pages
{
    public partial class NavigationPage : Page
    {
        private readonly Client client;
        private readonly Dictionary<string, Page> pages = new();

        public NavigationPage(Client _client)
        {
            InitializeComponent();
            client = _client;

            AddPage("Получение групп и каналов", new GetAllGroupsPage(client));
            AddPage("Получение чатов", new GetAllChatsPage(client));
            AddPage("Получение сообщений", new GetMessagesPage(client));
            AddPage("Массовая отправка текстовых сообщений", new MassMessagesPage(client));
            AddPage("Массовая рассылка сообщений с файлов", new MassFileMessages(client));
            AddPage("Проверка сообщений", new CheckMessagesPage(client));

            foreach (var page in pages)
            {
                panel.Children.Add(CreateNavButton(page.Key));
            }
        }

        private void AddPage(string name, Page page)
        {
            pages[name] = page;
        }

        private Button CreateNavButton(string name)
        {
            var button = new Button
            {
                Content = name,
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(5),
            };
            button.Style = (Style)Application.Current.Resources["TgButtonStyle"];
            button.Click += Button_Click;
            return button;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string name && pages.TryGetValue(name, out var page))
            {
                NavigationService?.Navigate(page);
            }
            else
            {
                MessageBox.Show("Страница не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

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
using TL;
using WTelegram;

namespace TeleBot.Pages
{
    /// <summary>
    /// Логика взаимодействия для MassMessagesPage.xaml
    /// </summary>
    public partial class MassMessagesPage : System.Windows.Controls.Page
    {
        Client client;
        public MassMessagesPage(Client _client)
        {
            InitializeComponent();
            client = _client;
        }

        private void info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Вы можете использовать холдеры для информации в тексте, вот их список:

Для личных чатов и ботов:
{first_name} - имя
{last_name} - фамилия
{phone} - номер телефона
{username} - юзернейм
{id} - ID

Для групп и каналов:
{username} - юзернейм
{title} - название
{id} - ID
");
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            start.IsEnabled = false;
            Task.Run(async () =>
            {
                try
                {
                    List<string> errors = new List<string>();
                    Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Отправка началась, по окончанию вам выдатся сообщение\nне запускайте сложные процессы в это время"); });
                    string[] ids = (new TextRange(idsBox.Document.ContentStart, idsBox.Document.ContentEnd)).Text.Trim(' ', '\n').Split('\n').Select(i => { return i.Trim(); }).Where(i => !String.IsNullOrEmpty(i)).ToArray();
                    string message = (new TextRange(text.Document.ContentStart, text.Document.ContentEnd)).Text;
                    var allUsers = (await client.Messages_GetAllDialogs()).users;
                    var allChats = (await client.Messages_GetAllChats()).chats;
                    foreach (string id in ids)
                    {
                        bool userSuc = allUsers.TryGetValue(Convert.ToInt64(id), out var dialog);
                        bool chatSuc = allChats.TryGetValue(Convert.ToInt64(id), out var chat);
                        if (!userSuc && !chatSuc)
                        {
                            errors.Add(id);
                            continue;
                        }

                        (string, string)[] map = null;
                        if (userSuc)
                            map = new[]
                            {
                                ("{first_name}", dialog.first_name),
                                ("{last_name}", dialog.last_name),
                                ("{phone}", dialog.phone),
                                ("{username}", dialog.username),
                                ("{id}", dialog.id.ToString())
                            };
                        else if (chatSuc)
                            map = new[]
                            {
                                ("{username}", String.IsNullOrEmpty(chat.MainUsername) ? "no username" : chat.MainUsername),
                                ("{id}", chat.ID.ToString()),
                                ("{title}", chat.Title)
                            };
                        else
                        {
                            errors.Add(id);
                            continue;
                        }

                        string msg = message;
                        foreach (var one in map)
                        {
                            msg = msg.Replace(one.Item1, one.Item2);
                        }

                        if (userSuc)
                            await client.SendMessageAsync(dialog!.ToInputPeer(), msg);
                        else if (chatSuc)
                            await client.SendMessageAsync(chat!.ToInputPeer(), msg);
                        else
                        {
                            errors.Add(id);
                            continue;
                        }

                        await Task.Delay(Config.sendMessageDelay);
                    }

                    if (errors.Count == 0)
                        Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Отправка {ids.Length.ToString()} сообщений завершена"); });
                    else
                        Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Отправка {ids.Length.ToString()} сообщений завершена\n\nОшибки отправки для:\n{string.Join('\n', errors)}"); });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Отправка сообщений прервалась ошибкой\n{ex.Message}"); });
                }
            });
        }
    }
}

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

namespace TeleBot.Pages
{
    /// <summary>
    /// Логика взаимодействия для MassMessagesPage.xaml
    /// </summary>
    public partial class MassMessagesPage : System.Windows.Controls.Page
    {
        MainWindow inst;
        public MassMessagesPage(MainWindow _inst)
        {
            InitializeComponent();
            inst = _inst;
        }

        private void info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Вы можете использовать холдеры для информации в тексте, вот их список:
{first_name} - имя
{last_name} - фамилия
{phone} - номер телефона
{username} - юзернейм
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
                    Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Отправка началась, по окончанию вам выдатся сообщение\nне запускайте сложные процессы в это время"); });
                    string[] ids = (new TextRange(idsBox.Document.ContentStart, idsBox.Document.ContentEnd)).Text.Trim(' ','\n').Split('\n').Select(i => { return i.Trim(); }).Where(i => !String.IsNullOrEmpty(i)).ToArray();
                    string message = (new TextRange(text.Document.ContentStart, text.Document.ContentEnd)).Text;
                    var allUsers = (await inst.account.Messages_GetAllDialogs()).users;
                    foreach (string id in ids)
                    {
                        bool suc = allUsers.TryGetValue(Convert.ToInt64(id), out var dialog);
                        if (!suc) continue;

                        (string, string)[] map = new []
                        {
                            ("{first_name}", dialog.first_name),
                            ("{last_name}", dialog.last_name),
                            ("{phone}", dialog.phone),
                            ("{username}", dialog.username),
                            ("{id}", dialog.id.ToString())
                        };

                        string msg = message;
                        foreach(var one in map)
                        {
                            msg = msg.Replace(one.Item1, one.Item2);
                        }
                        await inst.account.SendMessageAsync(dialog!.ToInputPeer(), msg);

                        await Task.Delay(1000);
                    }

                    Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Отправка {ids.Length.ToString()} сообщений завершена"); });
                }
                catch(Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Отправка сообщений прервалась ошибкой\n{ex.Message}"); });
                }
            });
        }
    }
}

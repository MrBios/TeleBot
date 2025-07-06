using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeleBot.Types;
using TL;
using WTelegram;
using Page = System.Windows.Controls.Page;
using Path = System.IO.Path;

namespace TeleBot.Pages
{
    /// <summary>
    /// Логика взаимодействия для GetMessagesPage.xaml
    /// </summary>
    public partial class GetMessagesPage : Page
    {
        MainWindow inst;
        public GetMessagesPage(MainWindow _inst)
        {
            InitializeComponent();
            onlyChats.IsChecked = true;
            inst = _inst;
        }

        private void onlyChats_Checked(object sender, RoutedEventArgs e)
        {
            onlyChannels.IsChecked = false;
        }

        private void onlyChannels_Checked(object sender, RoutedEventArgs e)
        {
            onlyChats.IsChecked = false;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog() { Title = "Выберите папку для сохранения" };
            var result = folderDialog.ShowDialog();
            if (!result.GetValueOrDefault(false))
                return;
            start.IsEnabled = false;
            Task.Run(async () =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Получение началось, по окончанию вам выдатся сообщение\nне запускайте сложные процессы в это время"); });
                    List<long> errors = new List<long>();
                    int maxCount = 0;
                    bool dialog = false;
                    Application.Current.Dispatcher.Invoke(() => { maxCount = Convert.ToInt32(maxBox.Text); dialog = onlyChats.IsChecked!.Value; });
                    string path = Path.Combine(folderDialog.FolderName, "messages");
                    string[] ids = (new TextRange(idsBox.Document.ContentStart, idsBox.Document.ContentEnd)).Text.Trim(' ', '\n').Split('\n').Select(i => { return i.Trim(); }).Where(i => !String.IsNullOrEmpty(i)).ToArray();
                    Dictionary<long, User> users = null;
                    Dictionary<long, ChatBase> chats = null;
                    if (dialog)
                    {
                        users = (await inst.account.Messages_GetAllDialogs()).users;
                    }
                    else
                    {
                        chats = (await inst.account.Messages_GetAllChats()).chats;
                    }

                    foreach (long id in ids.Select(i => Convert.ToInt64(i)))
                    {
                        try
                        {
                            List<Message> msgs = null;
                            if (dialog)
                            {
                                var res = users!.TryGetValue(id, out User user);
                                if (!res)
                                {
                                    errors.Add(id);
                                    continue;
                                }
                                msgs = await getAllMessagesAsync(user.ToInputPeer(), maxCount);
                            }
                            else
                            {
                                var res = chats!.TryGetValue(id, out ChatBase chat);
                                if (!res)
                                {
                                    errors.Add(id);
                                    continue;
                                }
                                msgs = await getAllMessagesAsync(chat.ToInputPeer(), maxCount);
                            }
                            foreach (var msg in msgs)
                            {
                                await saveMessageAsync(msg, Path.Combine(path, id.ToString()));
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add(id);
                        }

                        await Task.Delay(Config.getMessageDelay);
                    }

                    if (errors.Count == 0)
                        Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Загрузка сообщений завершена"); });
                    else
                    {
                        string mess = $"Загрузка сообщений завершена\n\nПроизошли ошибки при попытки найти/загрузить сообщения у следующих id:";
                        foreach (var error in errors)
                        {
                            mess += "\n" + error.ToString();
                        }
                        Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show(mess); });
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() => { start.IsEnabled = true; MessageBox.Show($"Загрузка сообщений прервалась ошибкой\n{ex.Message}"); });
                }
            });
        }

        private async Task saveMessageAsync(Message message, string path)
        {
            saveMessage rawMessage = saveMessage.fromMessage(message);
            string json = JsonSerializer.Serialize<saveMessage>(rawMessage, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            Directory.CreateDirectory(path);
            await File.WriteAllTextAsync(Path.Combine(path, $"message_{message.id.ToString()}.json"), json);

            if (message.media != null)
            {
                Directory.CreateDirectory(Path.Combine(path, "media"));

                if (message.media is TL.MessageMediaPhoto photoMedia && photoMedia.photo is TL.Photo photo)
                {
                    var size = photo.sizes.OfType<TL.PhotoSize>().OrderByDescending(s => s.w * s.h).FirstOrDefault();
                    if (size != null)
                    {
                        using var fs = File.Create(Path.Combine(path, "media", $"photo_{message.id.ToString()}.jpg"));
                        await inst.account.DownloadFileAsync(photo, fs, size);
                        fs.Close();
                    }
                }
                else if (message.media is TL.MessageMediaDocument docMedia && docMedia.document is TL.Document doc)
                {
                    string ext;
                    if (doc.mime_type == "image/webp") ext = ".webp";
                    else if (doc.mime_type == "video/mp4") ext = ".mp4";
                    else if (doc.mime_type == "image/gif") ext = ".gif";
                    else if (doc.mime_type == "audio/ogg") ext = ".ogg";
                    else if (doc.mime_type == "audio/mpeg") ext = ".mp3";
                    else ext = "." + doc.mime_type.Split('/').Last();

                    using var fs = File.Create(Path.Combine(path, "media", $"doc_{message.id.ToString()}" + ext));
                    await inst.account.DownloadFileAsync(doc, fs);
                }
                else if (message.media is TL.MessageMediaContact contact)
                {
                    File.WriteAllText(Path.Combine(path, "media", $"contact_{message.id.ToString()}.vcf"), $"BEGIN:VCARD\nFN:{contact.first_name} {contact.last_name}\nTEL:{contact.phone_number}\nEND:VCARD");
                }
                else if (message.media is TL.MessageMediaGeo geo && geo.geo is TL.GeoPoint geoPoint)
                {
                    File.WriteAllText(Path.Combine(path, "media", $"geo_{message.id.ToString()}.json"), $"{{\"lat\":{geoPoint.lat.ToString()},\"lon\":{geoPoint.lon.ToString()}}}");
                }
            }
        }

        private async Task<List<Message>> getAllMessagesAsync(InputPeer chat, int maxCount = 0)
        {
            var allMessages = new List<Message>();
            int offsetId = 0;
            int batchSize = Config.getMessageBatch;
            bool done = false;

            while (!done)
            {
                var history = await inst.account.Messages_GetHistory(chat, offset_id: offsetId, limit: batchSize);

                if (history.Messages == null || history.Messages.Count() == 0)
                    break;

                var messages = history.Messages
                    .OfType<Message>()
                    .ToList();

                allMessages.AddRange(messages);

                if (maxCount > 0 && allMessages.Count >= maxCount)
                {
                    allMessages = allMessages.Take(maxCount).ToList();
                    break;
                }

                if (messages.Count() < batchSize)
                    break;

                offsetId = messages.Last().ID;
            }
            return allMessages;
        }
    }
}

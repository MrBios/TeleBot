using HeyRed.Mime;
using Microsoft.Win32;
using System;
using System.Collections;
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
    public partial class MassFileMessages : Page
    {
        Client client = null;
        string[] messages;
        public MassFileMessages(Client _client)
        {
            InitializeComponent();
            client = _client;
        }

        private void loadMesgsButt_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файлы сообщений для отправки";
            openFileDialog.Filter = "Файл сообщения|*.message";
            openFileDialog.Multiselect = true;

            var result = openFileDialog.ShowDialog();
            if (result.GetValueOrDefault(false))
            {
                messages = openFileDialog.FileNames;
                loadedLabel.Content = $"Загружено {messages.Length.ToString()} сообщений";
            }
        }

        private void sendButt_Click(object sender, RoutedEventArgs e)
        {
            if (messages == null || messages.Length == 0) return;
            string[] _ids = (new TextRange(idsBox.Document.ContentStart, idsBox.Document.ContentEnd)).Text.Trim(' ', '\n').Split('\n').Select(i => { return i.Trim(); }).Where(i => !String.IsNullOrEmpty(i)).ToArray();
            if (_ids == null || _ids.Length == 0) return;
            bool _doMedia = doMediaBox.IsChecked.GetValueOrDefault(false);
            sendButt.IsEnabled = false;
            Task.Run(async () =>
            {
                string[] ids = _ids;
                bool doMedia = _doMedia;
                string[] msgs = messages;
                var allUsers = (await client.Messages_GetAllDialogs()).users;
                var allChats = (await client.Messages_GetAllChats()).chats;
                List<string> errors = new List<string>();
                Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Отправка началась, по окончанию вам выдатся сообщение\nне запускайте сложные процессы в это время\nне взаимодействуйте с файлами сообщений до конца"); });
                foreach (var msg in msgs)
                {
                    try
                    {
                        saveMessage mess = JsonSerializer.Deserialize<saveMessage>(File.ReadAllText(msg));
                        string media = "";
                        var mediaFile = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(msg)!, "media")).Where(i => Path.GetFileName(i).Contains(mess.id.ToString())).FirstOrDefault("");
                        InputMedia inputMedia = await CreateInputMediaAsync(mediaFile);

                        foreach (var id in ids)
                        {
                            try
                            {
                                bool isUser = allUsers.TryGetValue(Convert.ToInt64(id), out User user);
                                bool isChat = allChats.TryGetValue(Convert.ToInt64(id), out ChatBase chat);

                                if (!isUser && !isChat)
                                {
                                    errors.Add($"Проблема с ID: {id}");
                                    continue;
                                }

                                InputPeer input = isUser ? user.ToInputPeer() : chat.ToInputPeer();

                                if (doMedia)
                                {
                                    if (!String.IsNullOrEmpty(mediaFile))
                                    {
                                        await client.SendMessageAsync(input, mess.message, inputMedia, 0, mess.entities);
                                    }
                                }
                                else
                                {
                                    await client.SendMessageAsync(input, mess.message, null, 0, mess.entities);
                                }

                                await Task.Delay(Config.sendMessageDelay);
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Ошибка в ID: {id}\n\n{ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Ошибка в Файле: {Path.GetFileName(msg)}\n\n{ex.Message}");
                    }
                }

                if (errors.Count == 0)
                    Application.Current.Dispatcher.Invoke(() => { sendButt.IsEnabled = true; MessageBox.Show($"Отправка сообщений завершена"); });
                else
                    Application.Current.Dispatcher.Invoke(() => { sendButt.IsEnabled = true; MessageBox.Show($"Отправка сообщений завершена\n\nОшибки отправки:\n\n{string.Join('\n', errors)}"); });
            });
        }

        public async Task<InputMedia> CreateInputMediaAsync(string filePath)
        {
            var inputFile = await client.UploadFileAsync(filePath);

            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            string[] photoExts = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            if (Array.Exists(photoExts, e => e == ext))
            {
                return new InputMediaUploadedPhoto { file = inputFile };
            }
            else
            {
                return new InputMediaUploadedDocument
                {
                    file = inputFile,
                    mime_type = MimeTypesMap.GetMimeType(ext),
                    attributes = new[]
                    {
                new DocumentAttributeFilename { file_name = Path.GetFileName(filePath) }
            }
                };
            }
        }
    }
}

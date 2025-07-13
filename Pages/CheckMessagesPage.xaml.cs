using FFMediaToolkit;
using FFMediaToolkit.Decoding;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeleBot.Types;
using WTelegram;
using Path = System.IO.Path;

namespace TeleBot.Pages
{
    public partial class CheckMessagesPage : Page
    {
        private readonly Client client;
        private string[] messageFiles;
        public CheckMessagesPage(Client _client)
        {
            InitializeComponent();
            client = _client;
            try
            {
                FFmpegLoader.FFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFmpeg");
            }
            catch { }
        }

        private void loadMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файлы сообщений";
            openFileDialog.Filter = "Файл сообщения|*.message";
            openFileDialog.Multiselect = true;
            var result = openFileDialog.ShowDialog();
            if (result.GetValueOrDefault(false))
            {
                messageFiles = openFileDialog.FileNames;
                loadedLabel.Content = $"Загружено {messageFiles.Length} сообщений";
                messagesPanel.Children.Clear();
                foreach (var file in messageFiles)
                {
                    try
                    {
                        var msg = JsonSerializer.Deserialize<saveMessage>(File.ReadAllText(file));
                        var msgPanel = CreateMessagePanel(msg, file);
                        messagesPanel.Children.Add(msgPanel);
                    }
                    catch (Exception ex)
                    {
                        var errorText = new TextBlock {
                            Text = $"Ошибка чтения файла: {Path.GetFileName(file)}\n{ex.Message}",
                            Foreground = (Brush)Application.Current.Resources["TgErrorBrush"]
                        };
                        messagesPanel.Children.Add(errorText);
                    }
                }
            }
        }

        private BitmapImage GetVideoThumbnail(string videoPath, out string error)
        {
            error = null;
            try
            {
                using var file = MediaFile.Open(videoPath);
                var frame = file.Video.GetFrame(TimeSpan.FromSeconds(1));
                var data = frame.Data;
                var size = frame.ImageSize;
                int width = size.Width;
                int height = size.Height;
                int srcStride = frame.Stride; // stride in bytes per row in source
                byte[] pixelData;
                int stride = width * 4; // destination stride
                // FFMediaToolkit frame.PixelFormat: 0 = Unknown, 1 = Gray, 2 = RGB24, 3 = BGR24, 4 = RGBA32, 5 = BGRA32
                if ((int)frame.PixelFormat == 5) // BGRA32
                {
                    pixelData = data.ToArray();
                }
                else if ((int)frame.PixelFormat == 2) // RGB24
                {
                    // Конвертируем RGB24 в BGRA32 с учетом stride
                    var src = data.ToArray();
                    pixelData = new byte[height * stride];
                    for (int y = 0; y < height; y++)
                    {
                        int srcRow = y * srcStride;
                        int dstRow = y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            int srcIdx = srcRow + x * 3;
                            int dstIdx = dstRow + x * 4;
                            pixelData[dstIdx + 0] = src[srcIdx + 2]; // B
                            pixelData[dstIdx + 1] = src[srcIdx + 1]; // G
                            pixelData[dstIdx + 2] = src[srcIdx + 0]; // R
                            pixelData[dstIdx + 3] = 255;            // A
                        }
                    }
                }
                else if ((int)frame.PixelFormat == 3) // BGR24
                {
                    // Конвертируем BGR24 в BGRA32 с учетом stride
                    var src = data.ToArray();
                    pixelData = new byte[height * stride];
                    for (int y = 0; y < height; y++)
                    {
                        int srcRow = y * srcStride;
                        int dstRow = y * stride;
                        for (int x = 0; x < width; x++)
                        {
                            int srcIdx = srcRow + x * 3;
                            int dstIdx = dstRow + x * 4;
                            pixelData[dstIdx + 0] = src[srcIdx + 0]; // B
                            pixelData[dstIdx + 1] = src[srcIdx + 1]; // G
                            pixelData[dstIdx + 2] = src[srcIdx + 2]; // R
                            pixelData[dstIdx + 3] = 255;            // A
                        }
                    }
                }
                else
                {
                    error = $"Unsupported pixel format: {frame.PixelFormat}";
                    return null;
                }
                var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var ms = new MemoryStream();
                encoder.Save(ms);
                ms.Position = 0;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch (Exception ex) { error = ex.ToString(); return null; }
        }

        private UIElement CreateMessagePanel(saveMessage msg, string filePath)
        {
            var border = new Border
            {
                BorderBrush = (Brush)Application.Current.Resources["TgBorderBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(8),
                Background = (Brush)Application.Current.Resources["TgPanelBrush"]
            };
            var stack = new StackPanel { Orientation = Orientation.Vertical };

            var header = new StackPanel { Orientation = Orientation.Horizontal };
            var avatar = new Ellipse
            {
                Width = 36,
                Height = 36,
                Fill = (Brush)Application.Current.Resources["TgAccentBrush"],
                Margin = new Thickness(0, 0, 8, 0)
            };
            header.Children.Add(avatar);
            var nameAndDate = new StackPanel { Orientation = Orientation.Vertical };
            nameAndDate.Children.Add(new TextBlock { Text = $"ID: {msg.from_id}", FontWeight = FontWeights.Bold, Foreground = (Brush)Application.Current.Resources["TgTextBrush"] });
            nameAndDate.Children.Add(new TextBlock { Text = msg.date.ToString("g"), FontSize = 11, Foreground = (Brush)Application.Current.Resources["TgTextSecondaryBrush"] });
            header.Children.Add(nameAndDate);
            stack.Children.Add(header);

            stack.Children.Add(new TextBlock { Text = msg.message, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 6), Foreground = (Brush)Application.Current.Resources["TgTextBrush"] });

            string mediaDir = Path.Combine(Path.GetDirectoryName(filePath), "media");
            if (Directory.Exists(mediaDir))
            {
                var mediaFiles = Directory.GetFiles(mediaDir).Where(f => Path.GetFileName(f).Contains(msg.id.ToString())).ToArray();
                foreach (var mediaFile in mediaFiles)
                {
                    var ext = Path.GetExtension(mediaFile).ToLowerInvariant();
                    UIElement mediaElement = null;
                    string mediaType = "document";
                    if (new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" }.Contains(ext))
                    {
                        try
                        {
                            var img = new Image
                            {
                                Source = new BitmapImage(new Uri(mediaFile)),
                                MaxWidth = 200,
                                MaxHeight = 200,
                                Margin = new Thickness(0, 4, 0, 4),
                                Cursor = System.Windows.Input.Cursors.Hand
                            };
                            img.MouseLeftButtonUp += (s, e) => OpenMedia(mediaFile, "image");
                            mediaElement = img;
                            mediaType = "image";
                        }
                        catch { }
                    }
                    else if (new[] { ".mp4", ".webm", ".avi", ".mov", ".mkv" }.Contains(ext))
                    {
                        var previewStack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 4, 0, 4), Cursor = System.Windows.Input.Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Center };
                        var previewGrid = new Grid { Width = 200, Height = 120, Background = (Brush)Application.Current.Resources["TgBorderBrush"], HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                        string thumbError;
                        var thumb = GetVideoThumbnail(mediaFile, out thumbError);
                        if (thumb != null)
                        {
                            var img = new Image { Source = thumb, Stretch = Stretch.UniformToFill };
                            previewGrid.Children.Add(img);
                        }
                        var playIcon = new Canvas { Width = 48, Height = 48, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                        var triangle = new Polygon
                        {
                            Points = new PointCollection { new Point(8, 8), new Point(40, 24), new Point(8, 40) },
                            Fill = (Brush)Application.Current.Resources["TgTextBrush"],
                            Opacity = 0.85,
                            Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 4, ShadowDepth = 0, Color = Colors.Black, Opacity = 0.5 }
                        };
                        playIcon.Children.Add(triangle);
                        var iconBorder = new Border { Width = 48, Height = 48, Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Child = playIcon };
                        previewGrid.Children.Add(iconBorder);
                        previewStack.Children.Add(previewGrid);
                        previewStack.Children.Add(new TextBlock { Text = Path.GetFileName(mediaFile), FontSize = 12, Foreground = (Brush)Application.Current.Resources["TgTextBrush"], Margin = new Thickness(0, 2, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis, MaxWidth = 200, HorizontalAlignment = HorizontalAlignment.Center });
                        if (!string.IsNullOrEmpty(thumbError))
                        {
                            previewStack.Children.Add(new TextBlock { Text = $"[Ошибка превью: {thumbError}]", Foreground = (Brush)Application.Current.Resources["TgErrorBrush"], FontSize = 10, TextWrapping = TextWrapping.Wrap, MaxWidth = 200, HorizontalAlignment = HorizontalAlignment.Center });
                        }
                        previewStack.MouseLeftButtonUp += (s, e) => OpenMedia(mediaFile, "video");
                        mediaElement = previewStack;
                        mediaType = "video";
                    }
                    else if (ext == ".vcf")
                    {
                        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4), Cursor = System.Windows.Input.Cursors.Hand };
                        var ellipse = new Ellipse { Width = 24, Height = 24, Fill = (Brush)Application.Current.Resources["TgSuccessBrush"], Margin = new Thickness(0, 0, 8, 0) };
                        panel.Children.Add(ellipse);
                        panel.Children.Add(new TextBlock { Text = "Контакт", VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)Application.Current.Resources["TgTextBrush"] });
                        panel.MouseLeftButtonUp += (s, e) => OpenMedia(mediaFile, "document");
                        mediaElement = panel;
                        mediaType = "document";
                    }
                    else if (ext == ".json")
                    {
                        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4), Cursor = System.Windows.Input.Cursors.Hand };
                        var rect = new Rectangle { Width = 24, Height = 24, Fill = (Brush)Application.Current.Resources["TgAccentBrush"], Margin = new Thickness(0, 0, 8, 0) };
                        panel.Children.Add(rect);
                        panel.Children.Add(new TextBlock { Text = "Геолокация", VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)Application.Current.Resources["TgTextBrush"] });
                        panel.MouseLeftButtonUp += (s, e) => OpenMedia(mediaFile, "geo");
                        mediaElement = panel;
                        mediaType = "geo";
                    }
                    else if (new[] { ".mp3", ".ogg", ".wav", ".flac" }.Contains(ext))
                    {
                        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4), Cursor = System.Windows.Input.Cursors.Hand };
                        var rect = new Rectangle { Width = 24, Height = 24, Fill = (Brush)Application.Current.Resources["TgAccentBrush"], Margin = new Thickness(0, 0, 8, 0) };
                        panel.Children.Add(rect);
                        panel.Children.Add(new TextBlock { Text = $"Аудио: {Path.GetFileName(mediaFile)}", VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)Application.Current.Resources["TgTextBrush"] });
                        panel.MouseLeftButtonUp += (s, e) => OpenMedia(mediaFile, "document");
                        mediaElement = panel;
                        mediaType = "document";
                    }
                    else
                    {
                        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4), Cursor = System.Windows.Input.Cursors.Hand };
                        var rect = new Rectangle { Width = 24, Height = 24, Fill = (Brush)Application.Current.Resources["TgBorderBrush"], Margin = new Thickness(0, 0, 8, 0) };
                        panel.Children.Add(rect);
                        panel.Children.Add(new TextBlock { Text = $"Документ: {Path.GetFileName(mediaFile)}", VerticalAlignment = VerticalAlignment.Center, Foreground = (Brush)Application.Current.Resources["TgTextBrush"] });
                        panel.MouseLeftButtonUp += (s, e) => OpenMedia(mediaFile, "document");
                        mediaElement = panel;
                        mediaType = "document";
                    }
                    if (mediaElement != null)
                        stack.Children.Add(mediaElement);
                }
            }

            border.Child = stack;
            return border;
        }

        private void OpenMedia(string path, string type)
        {
            var wnd = new TeleBot.Windows.openMediaWindow(path, type);
            wnd.Show();
        }
    }
}

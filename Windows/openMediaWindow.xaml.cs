using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TeleBot.Windows
{
    public partial class openMediaWindow : Window
    {
        public openMediaWindow(string path, string type)
        {
            InitializeComponent();
            ShowMedia(path, type);
        }

        private void ShowMedia(string path, string type)
        {
            MainGrid.Children.Clear();
            if (type == "image")
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri(path)),
                    Stretch = Stretch.Fill,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var scroll = new ScrollViewer { Content = img, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
                MainGrid.Children.Add(scroll);
            }
            else if (type == "video")
            {
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var media = new MediaElement
                {
                    Source = new Uri(path),
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Manual,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(media, 0);
                grid.Children.Add(media);

                var controls = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10), HorizontalAlignment = HorizontalAlignment.Center };
                var playPauseBtn = new Button { Content = "stop", Width = 40, Height = 32, Margin = new Thickness(0, 0, 8, 0) };
                var slider = new Slider { Minimum = 0, Maximum = 1, Value = 0, Width = 300, Margin = new Thickness(0, 0, 8, 0) };
                var timeLabel = new TextBlock { Text = "00:00 / 00:00", VerticalAlignment = VerticalAlignment.Center, FontFamily = new FontFamily("Consolas") };
                controls.Children.Add(playPauseBtn);
                controls.Children.Add(slider);
                controls.Children.Add(timeLabel);
                Grid.SetRow(controls, 1);
                grid.Children.Add(controls);

                bool isPlaying = true;
                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                timer.Tick += (s, e) =>
                {
                    if (media.NaturalDuration.HasTimeSpan)
                    {
                        slider.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;
                        slider.Value = media.Position.TotalSeconds;
                        timeLabel.Text = $"{media.Position:mm\\:ss} / {media.NaturalDuration.TimeSpan:mm\\:ss}";
                    }
                };
                media.Loaded += (s, e) => { media.Play(); timer.Start(); isPlaying = true; playPauseBtn.Content = "stop"; };
                this.Closing += (s, e) => { media.Stop(); timer.Stop(); };
                playPauseBtn.Click += (s, e) =>
                {
                    if (media.NaturalDuration.HasTimeSpan)
                    {
                        if (isPlaying)
                        {
                            media.Pause();
                            playPauseBtn.Content = "play";
                            isPlaying = false;
                        }
                        else
                        {
                            media.Play();
                            playPauseBtn.Content = "stop";
                            isPlaying = true;
                        }
                    }
                };
                slider.PreviewMouseDown += (s, e) => timer.Stop();
                slider.PreviewMouseUp += (s, e) =>
                {
                    media.Position = TimeSpan.FromSeconds(slider.Value);
                    timer.Start();
                };
                MainGrid.Children.Add(grid);
            }
            else if (type == "geo")
            {
                try
                {
                    var geo = JsonSerializer.Deserialize<GeoPoint>(File.ReadAllText(path));
                    var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    stack.Children.Add(new TextBlock { Text = $"Геолокация:", FontWeight = FontWeights.Bold, FontSize = 18, Margin = new Thickness(0, 0, 0, 8) });
                    stack.Children.Add(new TextBlock { Text = $"Широта: {geo.lat}", FontSize = 16 });
                    stack.Children.Add(new TextBlock { Text = $"Долгота: {geo.lon}", FontSize = 16 });
                    var btn = new Button { Content = "Открыть в Яндекс.Картах", Margin = new Thickness(0, 12, 0, 0) };
                    btn.Click += (s, e) =>
                    {
                        var url = $"https://yandex.ru/maps/?ll={geo.lon},{geo.lat}&z=16";
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    };
                    stack.Children.Add(btn);
                    MainGrid.Children.Add(stack);
                }
                catch
                {
                    MainGrid.Children.Add(new TextBlock { Text = "Ошибка чтения гео-данных", Foreground = Brushes.Red });
                }
            }
            else if (type == "document")
            {
                var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                stack.Children.Add(new TextBlock { Text = System.IO.Path.GetFileName(path), FontWeight = FontWeights.Bold, FontSize = 16 });
                var btn = new Button { Content = "Открыть файл", Margin = new Thickness(0, 12, 0, 0) };
                btn.Click += (s, e) => Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                stack.Children.Add(btn);
                MainGrid.Children.Add(stack);
            }
            else
            {
                MainGrid.Children.Add(new TextBlock { Text = "Неизвестный тип медиа", Foreground = Brushes.Red });
            }
        }

        private class GeoPoint
        {
            public double lat { get; set; }
            public double lon { get; set; }
        }
    }
}

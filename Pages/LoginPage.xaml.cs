using System;
using System.Collections.Generic;
using System.IO;
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
using WTelegram;

namespace TeleBot.Pages
{
    public partial class LoginPage : Page
    {
        MainWindow instance;
        Client client;
        public LoginPage(MainWindow instance)
        {
            this.instance = instance;
            InitializeComponent();
        }

        private void submit_phone_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(phoneBox.Text))
            {
                MessageBox.Show("Введите телефон");
                return;
            }
            Func<string, string> config = inp =>
            {
                switch (inp)
                {
                    case "api_id":
                        return Config.api_id;
                    case "api_hash":
                        return Config.api_hash;
                    case "phone_number":
                        return phoneBox.Text;
                    case "verification_code":
                        string code = "";
                        Application.Current.Dispatcher.Invoke(() => { code = instance.confirmCode(phoneBox.Text); });
                        return code;
                        //return instance.confirmCode(phoneBox.Text);
                    case "password":
                        return instance.getPassword(phoneBox.Text);
                    case "session_pathname":
                        return Path.Combine(Directory.GetCurrentDirectory(), "sessions", phoneBox.Text + ".session");
                    default:
                        return null;
                }
            };
            instance.account = new Client(config);
            this.NavigationService?.Navigate(new NavigationPage(instance.account));
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TeleBot.Pages;
using TeleBot.Windows;
using TL;
using WTelegram;

namespace TeleBot
{
    public partial class MainWindow : Window
    {
        private Client _acc = null;
        public Client account
        {
            get { return _acc; }
            set
            {
                _acc = value;
                if (value != null)
                {
                    login();
                }
                viewAccount();
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            account = null;
            if (!String.IsNullOrEmpty(Config.last_acc))
            {
                Func<string, string> config = inp =>
                {
                    switch (inp)
                    {
                        case "api_id":
                            return Config.api_id;
                        case "api_hash":
                            return Config.api_hash;
                        case "phone_number":
                            return Config.last_acc;
                        case "verification_code":
                            return confirmCode(Config.last_acc);
                        case "password":
                            return getPassword(Config.last_acc);
                        case "session_pathname":
                            return Path.Combine(Directory.GetCurrentDirectory(), "sessions", Config.last_acc + ".session");
                        default:
                            return null;
                    }
                };
                try
                {
                    account = new Client(config);
                }
                catch { }
            }

            viewAccount();
        }

        async Task login()
        {
            await _acc.LoginUserIfNeeded();
            accountLabel.Content = account.User.phone;
        }

        void viewAccount()
        {
            if (account == null)
            {
                accountLabel.Content = "не залогинен";
                loginButton.Content = "Войти";
                loginButton.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                accountLabel.Content = "загрузка...";
                loginButton.Content = "Выйти";
                loginButton.Background = new SolidColorBrush(Colors.Red);
            }
        }

        public string confirmCode(string phone)
        {
            ConfirmCodeDialog dialog = new ConfirmCodeDialog(phone);
            dialog.ShowDialog();
            return dialog.code;
        }

        public string getPassword(string phone)
        {
            GetPasswordDialog dialog = new GetPasswordDialog(phone);
            dialog.ShowDialog();
            return dialog.pass;
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (account == null)
            {
                frame.Content = new LoginPage(this);
            }
            else
            {
                account.Auth_LogOut();
                Config.last_acc = null;
                account = null;
                frame.Content = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(account != null)
            {
                if(account.User != null)
                {
                    Config.last_acc = "+" + account.User.phone;
                    return;
                }
            }

            Config.last_acc = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(account != null && account.User != null)
            {
                frame.Content = new NavigationPage(this);
            }
        }
    }
}
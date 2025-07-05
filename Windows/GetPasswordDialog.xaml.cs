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
using System.Windows.Shapes;

namespace TeleBot.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConfirmCodeDialog.xaml
    /// </summary>
    public partial class GetPasswordDialog : Window
    {
        public string pass { get; private set; }
        public GetPasswordDialog(string phone)
        {
            InitializeComponent();
            this.phone.Content = phone;
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(codeBox.Text))
            {
                MessageBox.Show("Введите пароль");
                return;
            }

            pass = codeBox.Text;
            this.Close();
        }
    }
}

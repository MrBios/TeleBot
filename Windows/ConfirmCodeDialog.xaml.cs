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
    public partial class ConfirmCodeDialog : Window
    {
        public string code { get; private set; }
        public ConfirmCodeDialog(string phone)
        {
            InitializeComponent();
            this.phone.Content = phone;
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(codeBox.Text))
            {
                MessageBox.Show("Введите код");
                return;
            }

            code = codeBox.Text;
            this.Close();
        }
    }
}

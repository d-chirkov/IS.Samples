using IdentityModel.Client;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LogonWindow : Window
    {
        public LogonWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            // адрес сервера аутентификации
            // Для пользователей из базы данных (пока мнимой)
            string idsrvUri = "https://localhost:44301/identity";

            // С windows-аутентификацией пока не работает, реализуется неочевидно

            string clientId = "desktop1";
            string clientSecret = "secret3";

            var client = new TokenClient(
                idsrvUri + "/connect/token",
                clientId,
                clientSecret);

            // Запашиваем токен через ResourceOwner и со scope-ом равным openid
            var tokenResponse = await client.RequestResourceOwnerPasswordAsync(this.loginTextBox.Text, this.passwordTextBox.Password, "openid");
            // И смотрим ошибки в ответе, если они есть, значит аутентификация не удалась
            if (tokenResponse.IsError)
            {
                this.IsEnabled = true;
                string error = (string)tokenResponse.Json["error"];
                if (error == "invalid_grant")
                {
                    MessageBox.Show("Неверные логин или пароль");
                }
                else
                {
                    MessageBox.Show("Незвестная ошибка: " + error);
                }
            }
            // Если всё хорошо, закрываем текущее окно, и открываем само приложение
            else
            {
                MainWindow main = new MainWindow();
                App.Current.MainWindow = main;
                this.Close();
                main.Show();
            }
        }
    }
}

using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
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
            // Для пользователей из IS {
            //string idsrvUri = "https://localhost:44363/identity";
            //}

            // Для windows пользователей {
            string idsrvUri = "https://localhost:44363/winidentity";
            //string idsrvUri = "https://localhost:44384/identity";
            //}


            string clientId = "9bf3b01c-9689-4db2-87a2-09fd390df550";
            string clientSecret = "secret3";

            var client = new TokenClient(
                idsrvUri + "/connect/token",
                clientId,
                clientSecret);

            // Для обычной аутентификации через IS {
            //var tokenResponse = await client.RequestResourceOwnerPasswordAsync(this.loginTextBox.Text, this.passwordTextBox.Password, "openid");
            //}

            // Для windows аутентификации через IS.WindowsAuth {
            var creds = new Dictionary<string, string>();
            string userName = this.loginTextBox.Text;
            if (!userName.Contains("\\"))
            {
                var pc = new PrincipalContext(ContextType.Machine);
                userName = pc.ConnectedServer + "\\" + userName;
            }
            creds.Add("name", userName);
            creds.Add("password", this.passwordTextBox.Password);
            var tokenResponse = await client.RequestCustomGrantAsync("winauth", "profile", creds);
            //}

            // Смотрим ошибки в ответе, если они есть, значит аутентификация не удалась
            if (tokenResponse.IsError)
            {
                this.IsEnabled = true;
                string error = (string)tokenResponse.Json["error"] ?? "unknown";
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
                CurrentUserProvider.UserName = this.loginTextBox.Text;
                MainWindow main = new MainWindow();
                App.Current.MainWindow = main;
                this.Close();
                main.Show();
            }
        }
    }
}

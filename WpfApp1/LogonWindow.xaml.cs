namespace WpfApp1
{
    using System.Collections.Generic;
    using System.DirectoryServices.AccountManagement;
    using System.Windows;
    using IdentityModel.Client;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LogonWindow : Window
    {
        /// <summary>
        /// Конструктор окна входа. В App.xaml это окно устанавливается как ставртовое, так что оно будет показано
        /// перед основным прилоежнием.
        /// </summary>
        public LogonWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки входа пользователя.
        /// Данный метод взаимодействует в identity server через его api.
        /// Проверяются аутентификационные данные пользователя, далее просто отрывает само приложение - окно Main
        /// (либо messagebox с информацией об ошибке).
        /// </summary>
        /// <param name="sender">Отправитель события.</param>
        /// <param name="e">Аргументы события.</param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            // адрес сервера аутентификации
            // Для пользователей из IS {
            // string idsrvUri = "https://localhost:44363/identity";
            // }
            // Для windows пользователей {
            string idsrvUri = "https://localhost:44363/winidentity";

            // string idsrvUri = "https://localhost:44384/identity";
            // }
            string clientId = "9bf3b01c-9689-4db2-87a2-09fd390df550";
            string clientSecret = "secret3";

            var client = new TokenClient(
                idsrvUri + "/connect/token",
                clientId,
                clientSecret);

            // Для обычной аутентификации через IS {
            // var tokenResponse = await client.RequestResourceOwnerPasswordAsync(this.loginTextBox.Text, this.passwordTextBox.Password, "openid");
            // }

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

            // }
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
            else
            {
                // Если всё хорошо, закрываем текущее окно, и открываем само приложение
                CurrentUserProvider.UserName = this.loginTextBox.Text;
                MainWindow main = new MainWindow();
                App.Current.MainWindow = main;
                this.Close();
                main.Show();
            }
        }
    }
}

namespace WpfApp1
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Конструтор главного окна приложения. Эмулирует работу всего приложения. Должно быть показано только
        /// после успешного входа пользователя в окне <see cref="LogonWindow"/>.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Событие загрузки окна. Если пользователь вошёл, то в центре вывеедется его имя, иначе сообщение об ошибке.
        /// Ситуацией с ошибкой не должно произойти, окно <see cref="LogonWindow"/> не должно 
        /// открывать <see cref="MainWindow"/> до того, как пользователь войдёт.
        /// </summary>
        /// <param name="sender">Отправитель события.</param>
        /// <param name="e">Аргументы события.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.infoLabel.Content = 
                CurrentUserProvider.UserName == null ?
                "неизвестная ошибка" : 
                this.infoLabel.Content.ToString() + CurrentUserProvider.UserName;
        }
    }
}

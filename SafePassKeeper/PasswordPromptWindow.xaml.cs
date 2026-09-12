using System.Windows;
using System.Windows.Media;

namespace SafePassKeeper
{
    public partial class PasswordPromptWindow : Window
    {
        public string? Password { get; private set; }

        // Upravený konstruktor přijímá bool
        public PasswordPromptWindow(bool isNewFile)
        {
            InitializeComponent();

            if (isNewFile)
            {
                // Režim: Nový prázdný soubor
                TitleText.Text = "INITIALIZE NEW DATABANK";
                TitleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E5FF")); // Tritanium Cyan
                TitleGlow.Color = (Color)ColorConverter.ConvertFromString("#00E5FF");

                SubtitleText.Text = "// SET MASTER ENCRYPTION KEY";

                ActionBtn.Content = "[ ENCRYPT ]";
                ActionBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF66")); // Shield Green
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Password;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
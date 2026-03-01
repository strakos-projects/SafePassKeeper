using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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

namespace SafePassKeeper
{
    /// <summary>
    /// Interakční logika pro AddAccountWindow.xaml
    /// </summary>
    public partial class AddAccountWindow : Window
    {
        //  public Account NewAccount { get; private set; }
        public Account EditedAccount { get; private set; }
        public bool IsNewAccount { get; private set; }
        public AddAccountWindow(Account account = null)
        {
            InitializeComponent();
            if (account == null)
            {
                // Inicializace pro nový účet
                EditedAccount = new Account();
                IsNewAccount = true;
                this.Title = "Add New Account";
            }
            else
            {
                // Inicializace pro úpravu existujícího účtu
                EditedAccount = account;
                IsNewAccount = false;
                this.Title = "Edit Account";

                // Nastavení textových polí na hodnoty existujícího účtu
                AccountNameTextBox.Text = account.AccountName;
                UsernameTextBox.Text = account.Username;
                PasswordBox.Password = account.Password;
                DomainTextBox.Text = account.Domain;
                NoteTextBox.Text = account.Note;
            }
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AccountNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }
            
            // Aktualizace nebo vytvoření nového účtu
            EditedAccount.AccountName = AccountNameTextBox.Text;
            EditedAccount.Username = UsernameTextBox.Text;
            EditedAccount.Password = PasswordBox.Password;
            EditedAccount.Domain = DomainTextBox.Text;
            EditedAccount.Note = NoteTextBox.Text;

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

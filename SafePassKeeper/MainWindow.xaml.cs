using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace SafePassKeeper
{
    public partial class MainWindow : Window
    {
        private static string? FilePath;
        private static string? Password;
        private ObservableCollection<Account> accounts = new ObservableCollection<Account>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SetFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Encrypted files (*.txt)|*.txt",
                Title = "Select Encrypted Password File"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                FilePath = openFileDialog.FileName;

                // Zjistíme, zda je soubor úplně prázdný (nově vytvořený textový dokument)
                bool isNewFile = new FileInfo(FilePath).Length == 0;

                // Předáme tuto informaci oknu s heslem
                var passwordPrompt = new PasswordPromptWindow(isNewFile);

                if (passwordPrompt.ShowDialog() == true)
                {
                    Password = passwordPrompt.Password;
                    LoadAccounts();
                }
                else
                {
                    MessageBox.Show("ACCESS DENIED. No key provided.");
                }
            }
        }

        private void LoadAccounts()
        {
            if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("File path or password is not set.");
                return;
            }

            try
            {
                FileInfo fi = new FileInfo(FilePath);

                // Pokud je soubor prázdný, NEDEŠIFRUJEME ho, jen založíme čistou kolekci
                if (fi.Length == 0)
                {
                    accounts = new ObservableCollection<Account>();
                    SaveAccounts(); // Rovnou ho poprvé zašifrujeme s novým heslem
                }
                else
                {
                    // Klasický postup pro existující soubor
                    byte[] encryptedBytes = File.ReadAllBytes(FilePath);
                    string decryptedJson = Decrypt(encryptedBytes, Password);

                    if (string.IsNullOrWhiteSpace(decryptedJson))
                    {
                        accounts = new ObservableCollection<Account>();
                    }
                    else
                    {
                        accounts = JsonSerializer.Deserialize<ObservableCollection<Account>>(decryptedJson) ?? new ObservableCollection<Account>();
                    }
                }

                // Napojení dat (BEZ DisplayMemberPath - to dělalo ten error na screenshotu!)
                AccountListBox.ItemsSource = accounts;

                AccountListBox.Visibility = Visibility.Visible;
                DetailsTextBox.Visibility = Visibility.Visible;
                AddAccountButton.Visibility = Visibility.Visible;
                EditAccountButton.Visibility = Visibility.Visible;
                DeleteAccountButton.Visibility = Visibility.Visible;
                ExportBackupButton.Visibility = Visibility.Visible;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Specifický catch, pokud uživatel zadá ŠPATNÉ HESLO
                MessageBox.Show("DECRYPTION FAILED. Invalid master key or corrupted databank.", "Security Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Catch pro všechny ostatní chyby
                MessageBox.Show($"SYSTEM ERROR: {ex.Message}");
            }
        }



        private void CreateDefaultAccounts()
        {
            accounts = new ObservableCollection<Account>
            {
                new Account
                {
                    AccountName = "Example Account 1",
                    Username = "example1@example.com",
                    Password = "password123",
                    Domain = "example.com",
                    Note = "Default account"
                },
                new Account
                {
                    AccountName = "Example Account 2",
                    Username = "example2@example.com",
                    Password = "password456",
                    Domain = "example.org",
                    Note = "Another default account"
                }
            };

            SaveAccounts();
        }

        

        private void AccountListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountListBox.SelectedItem is Account selectedAccount)
            {
                DetailsTextBox.Text = $"Account Name: {selectedAccount.AccountName}\n" +
                                      $"Username: {selectedAccount.Username}\n" +
                                      $"Password: {selectedAccount.Password}\n" +
                                      $"Domain: {selectedAccount.Domain}\n" +
                                      $"Note: {selectedAccount.Note}";
            }
        }

        private void EncryptAndSaveText(string text, string password)
        {
            byte[] encryptedBytes = Encrypt(text, password);
            File.WriteAllBytes(FilePath, encryptedBytes);
        }

        private string DecryptAndReadText(string password)
        {
            byte[] encryptedBytes = File.ReadAllBytes(FilePath);
            return Decrypt(encryptedBytes, password);
        }

        private byte[] Encrypt(string text, string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(password, aes.KeySize / 8);
                aes.IV = GenerateKey(password, aes.BlockSize / 8);

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(text);
                            }
                        }
                        return ms.ToArray();
                    }
                }
            }
        }

        private string Decrypt(byte[] cipherText, string password)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GenerateKey(password, aes.KeySize / 8);
                aes.IV = GenerateKey(password, aes.BlockSize / 8);

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream ms = new MemoryStream(cipherText))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        private void SaveAccounts()
        {
            try
            {
                string json = JsonSerializer.Serialize(accounts);
                byte[] encryptedData = Encrypt(json, Password);
                File.WriteAllBytes(FilePath, encryptedData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving accounts: {ex.Message}");
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountListBox.SelectedItem as Account;
            if (selectedAccount == null)
            {
                MessageBox.Show("Please select an account to delete.");
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the account '{selectedAccount.AccountName}'?", "Confirm Deletion", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                accounts.Remove(selectedAccount);
                SaveAccounts();
                DetailsTextBox.Clear();
            }
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var addAccountWindow = new AddAccountWindow(null);
            if (addAccountWindow.ShowDialog() == true)
            {
                accounts.Add(addAccountWindow.EditedAccount);
                SaveAccounts();
            }
        }

        private void ExportBackupButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = "backup_passwords.json"
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    string json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, json);
                    MessageBox.Show("Backup exported successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting backup: {ex.Message}");
                }
            }
        }

        private void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountListBox.SelectedItem as Account;
            if (selectedAccount == null)
            {
                MessageBox.Show("Please select an account to edit.");
                return;
            }

            var editAccountWindow = new AddAccountWindow(selectedAccount);
            if (editAccountWindow.ShowDialog() == true)
            {
                AccountListBox.ItemsSource = null;
                AccountListBox.ItemsSource = accounts;
                SaveAccounts();
            }
        }

        private byte[] GenerateKey(string password, int length)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                byte[] keyBytes = new byte[length];
                Array.Copy(hashBytes, keyBytes, length);
                return keyBytes;
            }
        }
        private async void CopyPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListBox.SelectedItem is Account selectedAccount)
            {
                // Okamžitě zkopíruje heslo vybraného účtu do schránky
                Clipboard.SetText(selectedAccount.Password);

                if (sender is Button btn)
                {
                    // Dočasná změna UI
                    string originalText = btn.Content.ToString();
                    var originalBrush = btn.Foreground;

                    btn.Content = "[ COPIED ]";
                    btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF66")); // Shield Green

                    // Počká 1 sekundu
                    await Task.Delay(1000);

                    // Vrátí do původního stavu
                    btn.Content = originalText;
                    btn.Foreground = originalBrush;
                }
            }
        }
    }
}
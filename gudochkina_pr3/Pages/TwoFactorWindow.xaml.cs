using gudochkina_pr3.Models;
using gudochkina_pr3.Services;
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

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для TwoFactorWindow.xaml
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private EmailService _emailService;
        private Users _user;
        private string _userEmail;
        private string _generatedCode;

        private const string SMTP_SERVER = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        private const string SMTP_USERNAME = "dfdsfsfs096@gmail.com";
        private const string SMTP_PASSWORD = "vkvj obye tecf skpm";
        private const string FROM_EMAIL = "dfdsfsfs096@gmail.com";
       

        public bool IsAuthenticated { get; private set; }
        public TwoFactorWindow(Users user)
        {
            InitializeComponent();
            _user = user;
            InitializeEmailService();
            GetUserEmail();
            SendVerificationCode();
        }

        private void InitializeEmailService()
        {
            _emailService = new EmailService(
                SMTP_SERVER,
                SMTP_PORT,
                SMTP_USERNAME,
                SMTP_PASSWORD,
                FROM_EMAIL,
                "Двухфакторная аутентификация"
            );
        }

        private void GetUserEmail()
        {
            if (_user == null || string.IsNullOrEmpty(_user.Email))
            {
                MessageBox.Show("У вашей учетной записи не указан email. Обратитесь к администратору.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
                return;
            }

            _userEmail = _user.Email;

            txtEmailInfo.Text = $"Код отправлен на {_userEmail}";
        }

        private async void SendVerificationCode()
        {
            try
            {
                _generatedCode = EmailService.GenerateVerificationCode();

                string userName = _user.Login;

                string body = EmailService.CreateVerificationEmailBody(_generatedCode, userName);
                bool sent = await _emailService.SendEmailAsync(_userEmail, "Код подтверждения входа", body);

                if (!sent)
                {
                    ShowError("Не удалось отправить код. Попробуйте позже.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private async void btnResend_Click(object sender, RoutedEventArgs e)
        {
            btnResend.IsEnabled = false;
            await System.Threading.Tasks.Task.Delay(30000);
            btnResend.IsEnabled = true;

            SendVerificationCode();
            txtError.Visibility = Visibility.Collapsed;
        }

        private void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(code) || code.Length != 4)
            {
                ShowError("Введите 4-значный код");
                return;
            }

            if (code == _generatedCode)
            {
                IsAuthenticated = true;
                this.Close();
            }
            else
            {
                ShowError("Неверный код подтверждения");
                txtCode.Clear();
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}

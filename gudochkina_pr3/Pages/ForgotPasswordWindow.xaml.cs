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
    /// Логика взаимодействия для ForgotPasswordWindow.xaml
    /// </summary>
    public partial class ForgotPasswordWindow : Window
    {
        private EmailService _emailService;
        private string _userEmail;
        private int? _userId;
        private string _generatedCode;

        private const string SMTP_SERVER = "smtp.gmail.com";
        private const int SMTP_PORT = 587;
        private const string SMTP_USERNAME = "dfdsfsfs096@gmail.com";
        private const string SMTP_PASSWORD = "vkvj obye tecf skpm";
        private const string FROM_EMAIL = "dfdsfsfs096@gmail.com";
        public ForgotPasswordWindow()
        {
            InitializeComponent();
            InitializeEmailService();
        }

        private void InitializeEmailService()
        {
            _emailService = new EmailService(
                SMTP_SERVER,
                SMTP_PORT,
                SMTP_USERNAME,
                SMTP_PASSWORD,
                FROM_EMAIL,
                "Восстановление пароля"
            );
        }

        private async void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError(txtEmailError, "Введите email");
                return;
            }

            using (var db = new Entities1())
            {
                var user = db.Users
            .FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    ShowError(txtEmailError, "Пользователь с таким email не найден");
                    return;
                }

                _userId = user.Id;
                _userEmail = user.Email;

                _generatedCode = EmailService.GenerateVerificationCode();

                AuthTempData.SaveVerificationCode(email, _generatedCode, _userId.Value);

                string body = EmailService.CreatePasswordResetEmailBody(_generatedCode);
                bool sent = await _emailService.SendEmailAsync(email, "Код восстановления пароля", body);

                if (sent)
                {
                    spEmailStep.Visibility = Visibility.Collapsed;
                    spCodeStep.Visibility = Visibility.Visible;
                    txtEmailError.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ShowError(txtEmailError, "Не удалось отправить код. Попробуйте позже.");
                }
            }
        }

        private void btnVerifyCode_Click(object sender, RoutedEventArgs e)
        {
            string code = txtCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(code) || code.Length != 4)
            {
                ShowError(txtCodeError, "Введите 4-значный код");
                return;
            }

            if (AuthTempData.VerifyCode(_userEmail, code, out int? userId))
            {
                _userId = userId;

                spCodeStep.Visibility = Visibility.Collapsed;
                spPasswordStep.Visibility = Visibility.Visible;
                txtCodeError.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowError(txtCodeError, "Неверный или истекший код");
            }
        }

        private async void btnResendCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_userEmail))
            {
                spCodeStep.Visibility = Visibility.Collapsed;
                spEmailStep.Visibility = Visibility.Visible;
                return;
            }

            _generatedCode = EmailService.GenerateVerificationCode();
            AuthTempData.SaveVerificationCode(_userEmail, _generatedCode, _userId.Value);

            string body = EmailService.CreatePasswordResetEmailBody(_generatedCode);
            bool sent = await _emailService.SendEmailAsync(_userEmail, "Код восстановления пароля (повторно)", body);

            if (sent)
            {
                MessageBox.Show("Код отправлен повторно", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ShowError(txtCodeError, "Не удалось отправить код. Попробуйте позже.");
            }
        }

        private void btnSavePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowError(txtPasswordError, "Введите новый пароль");
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowError(txtPasswordError, "Пароль должен содержать минимум 6 символов");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError(txtPasswordError, "Пароли не совпадают");
                return;
            }

            // Сохранение нового пароля
            try
            {
                using (var db = new Entities1())
                {
                    var user = db.Users.Find(_userId);
                    if (user != null)
                    {
                        user.PasswordHash = Hash.HashPassword(newPassword);
                        db.SaveChanges();

                        AuthTempData.RemoveCode(_userEmail);

                        MessageBox.Show("Пароль успешно изменен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(txtPasswordError, $"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ShowError(TextBlock errorText, string message)
        {
            errorText.Text = message;
            errorText.Visibility = Visibility.Visible;
        }

        private void txtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPasswordError.Visibility = Visibility.Collapsed;
        }

        private void txtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPasswordError.Visibility = Visibility.Collapsed;
        }
    }
}

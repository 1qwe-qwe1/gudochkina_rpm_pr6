using gudochkina_pr3.Models;
using gudochkina_pr3.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Windows.Threading;
using Block = gudochkina_pr3.Services.Block;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autho.xaml
    /// </summary>
    public partial class Autho : Page
    {
        private int click;
        private int failedAttempts;
        private DateTime? blockEndTime;
        private DispatcherTimer timer;
        public Autho()
        {
            InitializeComponent();
            click = 0;
            failedAttempts = 0;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            CheckBlockStatus();
        }
        private void CheckBlockStatus()
        {
            blockEndTime = Block.GetBlockEndTime();

            if (blockEndTime.HasValue)
            {
                StartBlocking();
            }
            else
            {
                SetControlsEnabled(true);
                tbBlockTimer.Visibility = Visibility.Collapsed;
            }
        }

        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, "guest", "Гость"));
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (blockEndTime.HasValue && DateTime.Now < blockEndTime.Value)
                return;

            click += 1;
            string login = tbLogin.Text.Trim();
            string password = Hash.HashPassword(tbPassword.Text.Trim());

            Entities1 db = Entities1.GetContext();

            var user = db.Users
           .Include(u => u.Roles)
           .Include(u => u.Employees) // Добавляем загрузку данных сотрудника
           .Include(u => u.Clients)
           .FirstOrDefault(x => x.Login == login && x.PasswordHash == password);
           

            if (click == 1)
            {
                if (user != null)
                {
                    // Проверяем доступ для сотрудников
                    if (IsEmployeeRole(user.Roles.Name.ToString()))
                    {
                        if (!TimeHelper.IsWithinWorkingHours())
                        {
                            MessageBox.Show("Доступ запрещен! Рабочее время с 10:00 до 19:00.");
                            return;
                        }
                    }

                    SuccessfulLogin(user);
                }
                else
                {
                    FailedLogin();
                }

            }

            else if (click > 1)
            {
                if (user != null && tbCaptcha.Text == tblCaptcha.Text)
                {
                    // Проверяем доступ для сотрудников
                    if (IsEmployeeRole(user.Roles.Name.ToString()))
                    {
                        if (!TimeHelper.IsWithinWorkingHours())
                        {
                            MessageBox.Show("Доступ запрещен! Рабочее время с 10:00 до 19:00.");
                            return;
                        }
                    }
                    SuccessfulLogin(user);
                }

                else
                {
                    FailedLogin();
                }
            }
        }
        private void SuccessfulLogin(Users user)
        {
            MessageBox.Show("Вы вошли под: " + user.Roles.Name.ToString());
            failedAttempts = 0;
            Block.ClearBlockTime();

            // Получаем ФИО пользователя
            string surname = "";
            string name = "";
            string patronymic = "";

            // В зависимости от роли получаем данные из соответствующей таблицы
            if (user.Employees != null && user.Employees.Any())
            {
                var employee = user.Employees.First();
                surname = employee.Surname ?? "";
                name = employee.Name ?? "";
                patronymic = employee.Patronymic ?? "";
            }
            else if (user.Clients != null && user.Clients.Any())
            {
                var client = user.Clients.First();
                surname = client.Surname ?? "";
                name = client.Name ?? "";
                patronymic = client.Patronymic ?? "";
            }
            else
            {
                // Если нет данных в связанных таблицах, используем логин
                surname = user.Login;
                name = "";
            }
            LoadPage(user.Roles.Name.ToString(), user, surname, name, patronymic);
        }
        private bool IsEmployeeRole(string roleName)
        {
            string role = roleName.ToLower();

            return role == "администратор" ||
                   role == "кассир" ||
                   role == "фотограф";
        }
        private void FailedLogin()
        {
            failedAttempts++;

            if (failedAttempts >= 3)
            {
                blockEndTime = DateTime.Now.AddSeconds(10);
                Block.SetBlockEndTime(blockEndTime.Value);
                StartBlocking();
            }
            else
            {
                MessageBox.Show("Вы ввели логин или пароль неверно!");
                GenerateCaptcha();
                tbPassword.Clear();
                tbCaptcha.Clear();
            }
        }

        private void StartBlocking()
        {
            SetControlsEnabled(false);
            tbBlockTimer.Visibility = Visibility.Visible;
            timer.Start();
            UpdateTimerText();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (blockEndTime.HasValue)
            {
                var timeLeft = blockEndTime.Value - DateTime.Now;

                if (timeLeft.TotalSeconds <= 0)
                {
                    timer.Stop();
                    failedAttempts = 0;
                    blockEndTime = null;
                    Block.ClearBlockTime();
                    SetControlsEnabled(true);
                    tbBlockTimer.Visibility = Visibility.Collapsed;
                }
                else
                {
                    UpdateTimerText();
                }
            }
        }

        private void UpdateTimerText()
        {
            if (blockEndTime.HasValue)
            {
                var timeLeft = blockEndTime.Value - DateTime.Now;
                tbBlockTimer.Text = $"Блокировка: {timeLeft.Seconds} сек";
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            tbLogin.IsEnabled = enabled;
            tbPassword.IsEnabled = enabled;
            tbCaptcha.IsEnabled = enabled;
            btnEnter.IsEnabled = enabled;
            btnEnterGuests.IsEnabled = enabled;

            if (!enabled)
            {
                tbLogin.Background = Brushes.LightGray;
                tbPassword.Background = Brushes.LightGray;
                tbCaptcha.Background = Brushes.LightGray;
            }
            else
            {
                tbLogin.ClearValue(BackgroundProperty);
                tbPassword.ClearValue(BackgroundProperty);
                tbCaptcha.ClearValue(BackgroundProperty);
            }
        }
        private void GenerateCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;

            string capctchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = capctchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }
        private void LoadPage(string _role, Users user, string surname, string name, string patronymic)

        {
            click = 0;

            NavigationService.Navigate(new Client(user, _role, surname, name, patronymic));

        }
        public void ClearFields()
        {
            tbLogin.Clear();
            tbPassword.Clear();
            tbCaptcha.Clear();
            tbCaptcha.Visibility = Visibility.Collapsed;
            tblCaptcha.Visibility = Visibility.Collapsed;
            click = 0;
            failedAttempts = 0;
            Block.ClearBlockTime();
        }
    }
}

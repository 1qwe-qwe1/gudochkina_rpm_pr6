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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Client.xaml
    /// </summary>
    public partial class Client : Page
    {
        private string _surname;
        private string _name;
        private string _patronymic;
        private Users _user;
        private string _role;
        private DispatcherTimer _workTimer;

        public Client(Users user, string role, string surname = null, string name = null, string patronymic = null)
        {
            InitializeComponent();

            _user = user;
            _role = role;
            _surname = surname;
            _name = name;
            _patronymic = patronymic;

            // Устанавливаем приветствие
            UpdateGreetingText();

            // Показываем меню администратора если пользователь - администратор
            if (_role.ToLower() == "администратор" || _role.ToLower() == "administrator")
            {
                adminMenuPanel.Visibility = Visibility.Visible;
            }


            // Если это сотрудник, проверяем рабочее время
            if (!string.IsNullOrEmpty(role) &&
        (role.ToLower() == "сотрудник" || role.ToLower() == "employee"))
            {
                StartWorkingHoursCheck();
            }
        }

        private void UpdateGreetingText()
        {
            if (!string.IsNullOrWhiteSpace(_surname) && !string.IsNullOrWhiteSpace(_name))
            {
                greetingTextBlock.Text = TimeHelper.GetFullGreeting(_surname, _name, _patronymic);
            }
            else
            {
                greetingTextBlock.Text = "Добро пожаловать!";
            }
        }

        private void StartWorkingHoursCheck()
        {
            _workTimer = new DispatcherTimer();
            _workTimer.Interval = TimeSpan.FromMinutes(1);
            _workTimer.Tick += (s, e) =>
            {
                if (!TimeHelper.IsWithinWorkingHours())
                {
                    _workTimer.Stop();
                    MessageBox.Show("Рабочее время закончилось! Доступ закрыт.");
                    // Возвращаем на страницу авторизации
                    NavigationService?.Navigate(new Autho());
                }
            };
            _workTimer.Start();
        }

        

        // Очистка ресурсов при выгрузке страницы
        private void Client_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_workTimer != null)
            {
                _workTimer.Stop();
                _workTimer = null;
            }
        }
        private void btnEmployees_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу управления сотрудниками
            NavigationService.Navigate(new EmployeesList());
        }
    }
}

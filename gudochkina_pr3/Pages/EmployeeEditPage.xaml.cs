using gudochkina_pr3.Models;
using gudochkina_pr3.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeeEditPage.xaml
    /// </summary>
    public partial class EmployeeEditPage : Page
    {
        private int? _employeeId;
        private byte[] _photoBytes;

        public EmployeeEditPage()
        {
            InitializeComponent();
            LoadComboBoxData();
            tbTitle.Text = "Добавление сотрудника";
        }

        public EmployeeEditPage(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            LoadComboBoxData();
            LoadEmployeeData();
            tbTitle.Text = "Редактирование сотрудника";
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var db = new Entities1())
                {
                    // Загрузка статусов
                    string[] statuses = new string[] { "Активен", "Неактивен" };
                    cmbStatus.ItemsSource = statuses;
                    cmbStatus.SelectedIndex = 0;

                    // Загрузка должностей
                    var posts = db.Posts.ToList();
                    cmbPost.ItemsSource = posts;
                    cmbPost.DisplayMemberPath = "Name";
                    cmbPost.SelectedValuePath = "PostId";

                    // Загрузка ролей
                    var roles = db.Roles.ToList();
                    cmbRole.ItemsSource = roles;
                    cmbRole.DisplayMemberPath = "Name";
                    cmbRole.SelectedValuePath = "RoleID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadEmployeeData()
        {
            try
            {
                using (var db = new Entities1())
                {
                    var employee = db.Employees
                        .Include("Users.Roles")
                        .Include("Posts")
                        .FirstOrDefault(e => e.EmployeeId == _employeeId);

                    if (employee != null)
                    {
                        txtSurname.Text = employee.Surname;
                        txtName.Text = employee.Name;
                        txtPatronymic.Text = employee.Patronymic;
                        txtPhoneNumber.Text = employee.PhoneNumber;
                        txtLogin.Text = employee.Users?.Login;

                        // Статус - используем GetValueOrDefault() для nullable bool
                        bool isActive = employee.IsActive.GetValueOrDefault(); // или employee.IsActive ?? false
                        cmbStatus.SelectedItem = isActive ? "Активен" : "Неактивен";

                        // Должность - PostId не nullable, проверяем если > 0
                        if (employee.PostId > 0) // или if (employee.PostId != null && employee.PostId > 0)
                        {
                            cmbPost.SelectedValue = employee.PostId;
                        }

                        // Роль
                        if (employee.Users?.RoleID != null)
                        {
                            cmbRole.SelectedValue = employee.Users.RoleID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных сотрудника: {ex.Message}");
            }
        }

        private void LoadImageFromBytes(byte[] imageData)
        {
            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgPhoto.Source = bitmap;
                }
            }
            catch
            {
                // Если не удалось загрузить изображение, оставляем заглушку
            }
        }

        private void btnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*",
                Title = "Выберите фотографию сотрудника"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _photoBytes = File.ReadAllBytes(openFileDialog.FileName);
                    LoadImageFromBytes(_photoBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                using (var db = new Entities1())
                {
                    if (_employeeId.HasValue)
                    {
                        // Редактирование существующего сотрудника
                        var employee = db.Employees
                            .Include("Users")
                            .FirstOrDefault(emp => emp.EmployeeId == _employeeId);

                        if (employee != null)
                        {
                            UpdateEmployeeData(employee, db);
                            db.SaveChanges();
                            MessageBox.Show("Данные сотрудника успешно обновлены!");
                            NavigationService.GoBack();
                        }
                    }
                    else
                    {
                        // Добавление нового сотрудника
                        var newEmployee = new Employees();
                        UpdateEmployeeData(newEmployee, db);

                        // Создание пользователя
                        var user = new Users
                        {
                            Login = txtLogin.Text,
                            PasswordHash = Hash.HashPassword(txtPassword.Password),
                            RoleID = (int)cmbRole.SelectedValue
                        };

                        db.Users.Add(user);
                        db.SaveChanges();

                        newEmployee.UserId = user.Id;
                        db.Employees.Add(newEmployee);
                        db.SaveChanges();

                        MessageBox.Show("Сотрудник успешно добавлен!");
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void UpdateEmployeeData(Employees employee, Entities1 db)
        {
            employee.Surname = txtSurname.Text;
            employee.Name = txtName.Text;
            employee.Patronymic = txtPatronymic.Text;
            employee.PhoneNumber = txtPhoneNumber.Text;
            employee.IsActive = cmbStatus.SelectedItem.ToString() == "Активен";
            employee.HireDate = DateTime.Now;

            if (cmbPost.SelectedValue != null)
            {
                employee.PostId = (int)cmbPost.SelectedValue;
            }

            // Обновление логина пользователя
            if (employee.Users != null && !string.IsNullOrEmpty(txtLogin.Text))
            {
                employee.Users.Login = txtLogin.Text;

                // Обновление пароля, если он введен
                if (!string.IsNullOrEmpty(txtPassword.Password))
                {
                    employee.Users.PasswordHash = Hash.HashPassword(txtPassword.Password);
                }

                // Обновление роли
                if (cmbRole.SelectedValue != null)
                {
                    employee.Users.RoleID = (int)cmbRole.SelectedValue;
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                MessageBox.Show("Введите фамилию!");
                txtSurname.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя!");
                txtName.Focus();
                return false;
            }

            if (!_employeeId.HasValue && string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль для нового сотрудника!");
                txtPassword.Focus();
                return false;
            }

            if (cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Выберите роль!");
                cmbRole.Focus();
                return false;
            }

            return true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSurname.Clear();
            txtName.Clear();
            txtPatronymic.Clear();
            txtPhoneNumber.Clear();
            txtLogin.Clear();
            txtPassword.Clear();
            cmbStatus.SelectedIndex = 0;
            cmbPost.SelectedItem = null;
            cmbRole.SelectedItem = null;

            // Сброс фото на заглушку
            try
            {
                imgPhoto.Source = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Resources/default-avatar.png"));
            }
            catch
            {
                // Если не удалось загрузить заглушку, просто очищаем
                imgPhoto.Source = null;
            }

            _photoBytes = null;
        }
    }
}

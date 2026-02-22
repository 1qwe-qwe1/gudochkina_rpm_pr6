using gudochkina_pr3.Models;
using gudochkina_pr3.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace gudochkina_pr3.Pages
{

    /// <summary>
    /// Класс для отображения ошибок валидации
    /// </summary>
    public class ValidationError
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для EmployeeEditPage.xaml
    /// </summary>
    public partial class EmployeeEditPage : Page
    {
        private int? _employeeId;
        private byte[] _photoBytes;
        private EmployeeValidator _validator;

        private Dictionary<string, FrameworkElement> _validationControls;

        public EmployeeEditPage()
        {
            InitializeComponent();
            _validator = new EmployeeValidator();
            InitializeValidationControls();
            LoadComboBoxData();
            tbTitle.Text = "Добавление сотрудника";
        }

        public EmployeeEditPage(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            _validator = new EmployeeValidator();
            InitializeValidationControls();
            LoadComboBoxData();
            LoadEmployeeData();
            tbTitle.Text = "Редактирование сотрудника";

            txtPassword.Tag = "optional";
        }

        private void InitializeValidationControls()
        {
            _validationControls = new Dictionary<string, FrameworkElement>
            {
                { "Surname", txtSurname },
                { "Name", txtName },
                { "Patronymic", txtPatronymic },
                { "PhoneNumber", txtPhoneNumber },
                { "Login", txtLogin },
                { "Password", txtPassword },
                { "Status", cmbStatus },
                { "PostId", cmbPost },
                { "RoleId", cmbRole }
            };

            SetupRealTimeValidation();
        }

        private void SetupRealTimeValidation()
        {
            txtSurname.TextChanged += (s, e) => ValidateField("Surname", txtSurname.Text);
            txtName.TextChanged += (s, e) => ValidateField("Name", txtName.Text);
            txtPatronymic.TextChanged += (s, e) => ValidateField("Patronymic", txtPatronymic.Text);
            txtPhoneNumber.TextChanged += (s, e) => ValidateField("PhoneNumber", txtPhoneNumber.Text);
            txtLogin.TextChanged += (s, e) => ValidateField("Login", txtLogin.Text);
            txtPassword.PasswordChanged += (s, e) => ValidateField("Password", txtPassword.Password);
            cmbStatus.SelectionChanged += (s, e) => ValidateField("Status", cmbStatus.SelectedItem?.ToString());
            cmbPost.SelectionChanged += (s, e) => ValidateField("PostId", cmbPost.SelectedValue);
            cmbRole.SelectionChanged += (s, e) => ValidateField("RoleId", cmbRole.SelectedValue);
        }

        private void ValidateField(string propertyName, object value)
        {
            var model = CreateValidationModel();
            SetPropertyValue(model, propertyName, value);

            int? existingUserId = null;
            if (_employeeId.HasValue)
            {
                using (var db = new Entities1())
                {
                    var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == _employeeId);
                    if (employee?.UserId.HasValue == true)
                    {
                        existingUserId = employee.UserId.Value;
                    }
                }
            }

            var errors = _validator.Validate(model, !_employeeId.HasValue, existingUserId);
            var fieldErrors = errors.Where(e => e.PropertyName == propertyName).ToList();

            ShowFieldErrors(propertyName, fieldErrors);
        }

        private void ShowFieldErrors(string propertyName, List<ValidationError> errors)
        {
            if (_validationControls.TryGetValue(propertyName, out var control))
            {
                if (control is TextBox textBox)
                {
                    textBox.Style = (Style)FindResource("TextBoxStyle");
                    var toolTip = ToolTipService.GetToolTip(textBox) as ToolTip;
                    if (toolTip != null)
                    {
                        toolTip.Content = null;
                    }
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.Style = (Style)FindResource("ComboBoxStyle");
                    var toolTip = ToolTipService.GetToolTip(comboBox) as ToolTip;
                    if (toolTip != null)
                    {
                        toolTip.Content = null;
                    }
                }
                else if (control is PasswordBox passwordBox)
                {
                    passwordBox.Style = (Style)FindResource("PasswordBoxStyle");
                    var toolTip = ToolTipService.GetToolTip(passwordBox) as ToolTip;
                    if (toolTip != null)
                    {
                        toolTip.Content = null;
                    }
                }

                if (errors.Any())
                {
                    var errorMessage = string.Join("\n", errors.Select(e => e.ErrorMessage));

                    if (control is TextBox textBoxError)
                    {
                        textBoxError.Style = (Style)FindResource("TextBoxErrorStyle");
                        ToolTipService.SetToolTip(textBoxError, new ToolTip { Content = errorMessage });
                    }
                    else if (control is ComboBox comboBoxError)
                    {
                        comboBoxError.Style = (Style)FindResource("ComboBoxErrorStyle");
                        ToolTipService.SetToolTip(comboBoxError, new ToolTip { Content = errorMessage });
                    }
                    else if (control is PasswordBox passwordBoxError)
                    {
                        passwordBoxError.Style = (Style)FindResource("PasswordBoxErrorStyle");
                        ToolTipService.SetToolTip(passwordBoxError, new ToolTip { Content = errorMessage });
                    }
                }
            }
        }

        private EmployeeValidationModel CreateValidationModel()
        {
            return new EmployeeValidationModel
            {
                Surname = txtSurname.Text,
                Name = txtName.Text,
                Patronymic = txtPatronymic.Text,
                PhoneNumber = txtPhoneNumber.Text,
                Login = txtLogin.Text,
                Password = txtPassword.Password,
                Status = cmbStatus.SelectedItem?.ToString(),
                PostId = cmbPost.SelectedValue as int?,
                RoleId = cmbRole.SelectedValue as int?,
                Photo = _photoBytes
            };
        }

        private void SetPropertyValue(EmployeeValidationModel model, string propertyName, object value)
        {
            var property = typeof(EmployeeValidationModel).GetProperty(propertyName);
            if (property != null)
            {
                try
                {
                    property.SetValue(model, Convert.ChangeType(value, property.PropertyType));
                }
                catch
                {
                   
                }
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var db = new Entities1())
                {
                    string[] statuses = new string[] { "Активен", "Неактивен" };
                    cmbStatus.ItemsSource = statuses;
                    cmbStatus.SelectedIndex = 0;

                    var posts = db.Posts.ToList();
                    cmbPost.ItemsSource = posts;
                    cmbPost.DisplayMemberPath = "PostName";
                    cmbPost.SelectedValuePath = "PostId";

                    var roles = db.Roles.ToList();
                    cmbRole.ItemsSource = roles;
                    cmbRole.DisplayMemberPath = "Name";
                    cmbRole.SelectedValuePath = "RoleID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadEmployeeData()
        {
            try
            {
                using (var db = new Entities1())
                {
                    var employee = db.Employees
                        .Include("Users")
                        .Include("Users.Roles")
                        .Include("Posts")
                        .FirstOrDefault(e => e.EmployeeId == _employeeId);

                    if (employee != null)
                    {
                        txtSurname.Text = employee.Surname;
                        txtName.Text = employee.Name;
                        txtPatronymic.Text = employee.Patronymic;
                        txtPhoneNumber.Text = employee.Users?.PhoneNumber ?? "";

                        txtLogin.Text = employee.Users?.Login;

                        bool isActive = employee.IsActive.GetValueOrDefault();
                        cmbStatus.SelectedItem = isActive ? "Активен" : "Неактивен";

                        if (employee.PostId > 0)
                        {
                            cmbPost.SelectedValue = employee.PostId;
                        }

                        if (employee.Users?.RoleID != null)
                        {
                            cmbRole.SelectedValue = employee.Users.RoleID;
                        }

                        // Загрузка фото
                      /*  if (employee.Photo != null)
                        {
                            _photoBytes = employee.Photo;
                            LoadImageFromBytes(_photoBytes);
                        }*/
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных сотрудника: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImageFromBytes(byte[] imageData)
        {
            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgPhoto.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        private void btnAddPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                Title = "Выберите фотографию сотрудника",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Length > 5 * 1024 * 1024) 
                    {
                        MessageBox.Show("Размер файла не должен превышать 5 МБ", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _photoBytes = File.ReadAllBytes(openFileDialog.FileName);
                    LoadImageFromBytes(_photoBytes);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ClearAllValidationErrors();

            var model = CreateValidationModel();
            int? existingUserId = null;
            if (_employeeId.HasValue)
            {
                using (var db = new Entities1())
                {
                    var employee = db.Employees.FirstOrDefault(f => f.EmployeeId == _employeeId);
                    if (employee?.UserId.HasValue == true)
                    {
                        existingUserId = employee.UserId.Value;
                    }
                }
            }

            var errors = _validator.Validate(model, !_employeeId.HasValue, existingUserId);

            if (errors.Any())
            {
                ShowAllValidationErrors(errors);

                var errorMessage = "Обнаружены ошибки валидации:\n\n" +
                    string.Join("\n", errors.Select(er => $"- {er.ErrorMessage}"));

                MessageBox.Show(errorMessage, "Ошибки валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveEmployeeData();
        }

        private void ClearAllValidationErrors()
        {
            foreach (var control in _validationControls.Values)
            {
                if (control is TextBox textBox)
                {
                    textBox.Style = (Style)FindResource("TextBoxStyle");
                    ToolTipService.SetToolTip(textBox, null);
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.Style = (Style)FindResource("ComboBoxStyle");
                    ToolTipService.SetToolTip(comboBox, null);
                }
                else if (control is PasswordBox passwordBox)
                {
                    passwordBox.Style = (Style)FindResource("PasswordBoxStyle");
                    ToolTipService.SetToolTip(passwordBox, null);
                }
            }
        }

        private void ShowAllValidationErrors(List<ValidationError> errors)
        {
            var groupedErrors = errors.GroupBy(e => e.PropertyName);

            foreach (var group in groupedErrors)
            {
                if (_validationControls.TryGetValue(group.Key, out var control))
                {
                    var errorMessage = string.Join("\n", group.Select(e => e.ErrorMessage));

                    if (control is TextBox textBox)
                    {
                        textBox.Style = (Style)FindResource("TextBoxErrorStyle");
                        ToolTipService.SetToolTip(textBox, new ToolTip { Content = errorMessage });
                    }
                    else if (control is ComboBox comboBox)
                    {
                        comboBox.Style = (Style)FindResource("ComboBoxErrorStyle");
                        ToolTipService.SetToolTip(comboBox, new ToolTip { Content = errorMessage });
                    }
                    else if (control is PasswordBox passwordBox)
                    {
                        passwordBox.Style = (Style)FindResource("PasswordBoxErrorStyle");
                        ToolTipService.SetToolTip(passwordBox, new ToolTip { Content = errorMessage });
                    }
                }
            }
        }

        private void SaveEmployeeData()
        {
            try
            {
                using (var db = new Entities1())
                {
                    if (_employeeId.HasValue)
                    {
                        var employee = db.Employees
                            .Include("Users")
                            .FirstOrDefault(emp => emp.EmployeeId == _employeeId);

                        if (employee != null)
                        {
                            UpdateEmployeeData(employee, db);
                            db.SaveChanges();
                            MessageBox.Show("Данные сотрудника успешно обновлены!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            NavigationService.GoBack();
                        }
                    }
                    else
                    {
                        var newEmployee = new Employees();
                        UpdateEmployeeData(newEmployee, db);

                        var user = new Users
                        {
                            Login = txtLogin.Text,
                           // Email = txtEmail.Text.Trim(),          
                            PhoneNumber = txtPhoneNumber.Text.Trim(),
                            PasswordHash = Hash.HashPassword(txtPassword.Password),
                            RoleID = (int)cmbRole.SelectedValue
                        };

                        db.Users.Add(user);
                        db.SaveChanges();

                        newEmployee.UserId = user.Id;
                        db.Employees.Add(newEmployee);
                        db.SaveChanges();

                        MessageBox.Show("Сотрудник успешно добавлен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\nДетали: {ex.InnerException?.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateEmployeeData(Employees employee, Entities1 db)
        {
            employee.Surname = txtSurname.Text.Trim();
            employee.Name = txtName.Text.Trim();
            employee.Patronymic = txtPatronymic.Text?.Trim();
          //  employee.Users.PhoneNumber = txtPhoneNumber.Text.Trim();
            employee.IsActive = cmbStatus.SelectedItem.ToString() == "Активен";
            employee.HireDate = DateTime.Now;
          //  employee.Photo = _photoBytes;

            if (cmbPost.SelectedValue != null)
            {
                employee.PostId = (int)cmbPost.SelectedValue;
            }

            if (employee.Users != null && !string.IsNullOrEmpty(txtLogin.Text))
            {
                employee.Users.Login = txtLogin.Text.Trim();
                employee.Users.PhoneNumber = txtPhoneNumber.Text.Trim();

                if (!string.IsNullOrEmpty(txtPassword.Password))
                {
                    employee.Users.PasswordHash = Hash.HashPassword(txtPassword.Password);
                }

                if (cmbRole.SelectedValue != null)
                {
                    employee.Users.RoleID = (int)cmbRole.SelectedValue;
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearAllValidationErrors();

            txtSurname.Clear();
            txtName.Clear();
            txtPatronymic.Clear();
            txtPhoneNumber.Clear();
            txtLogin.Clear();
            txtPassword.Clear();
            cmbStatus.SelectedIndex = 0;
            cmbPost.SelectedItem = null;
            cmbRole.SelectedItem = null;

            try
            {
                imgPhoto.Source = new BitmapImage(
                    new Uri("pack://application:,,,/Resources/default-avatar.png"));
            }
            catch
            {
                imgPhoto.Source = null;
            }

            _photoBytes = null;
        }

        private void txtPhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '+' && c != '(' && c != ')' && c != ' ' && c != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtSurname_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsLetter(c) && c != ' ' && c != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsLetter(c) && c != ' ' && c != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtPatronymic_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsLetter(c) && c != ' ' && c != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
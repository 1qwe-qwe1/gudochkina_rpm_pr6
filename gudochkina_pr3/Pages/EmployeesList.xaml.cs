using gudochkina_pr3.Models;
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
using static gudochkina_pr3.Pages.Autho;
using System.Data.Entity;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeesList.xaml
    /// </summary>
    public partial class EmployeesList : Page
    {
        private List<EmployeeViewModel> _allEmployees;

        public EmployeesList()
        {
            InitializeComponent();
            this.Loaded += EmployeesList_Loaded;
        }

        private void EmployeesList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
            LoadRolesFilter();
        }

        private void LoadEmployees()
        {
            try
            {
                using (var db = new Entities1())
                {
                    var employeeUserIds = db.Users
                .Where(u => u.Roles.Name.ToLower() == "Сотрудник" ||
                           u.Roles.Name.ToLower() == "Администратор" ||
                           u.Roles.Name.ToLower() == "employee" ||
                           u.Roles.Name.ToLower() == "administrator")
                .Select(u => u.Id)
                .ToList();

                    _allEmployees = db.Employees
                        .Include("Users.Roles")
                        .Include("Posts")
                        .Where(e => employeeUserIds.Contains(e.UserId.Value))
                        .AsEnumerable()
                        .Select(e => new EmployeeViewModel
                        {
                            EmployeeId = e.EmployeeId,
                            Surname = e.Surname,
                            Name = e.Name,
                            Patronymic = e.Patronymic,
                            PhoneNumber = e.Users?.PhoneNumber ?? "Не указан",
                            Email = e.Users?.Email ?? "Не указан",
                            RoleName = e.Users?.Roles?.Name ?? "Не указана",
                            PostName = e.Posts?.PostName ?? "Не указана",
                            IsActive = e.IsActive,
                            Login = e.Users?.Login ?? "",
                            UserId = e.UserId,
                            HireDate = e.HireDate,
                            PostId = e.PostId
                        })
                        .ToList();

                    lvEmployees.ItemsSource = _allEmployees;
                    tbStatus.Text = $"Найдено сотрудников: {_allEmployees.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void LoadRolesFilter()
        {
            try
            {
                using (var db = new Entities1())
                {
                    var roles = db.Roles
                        .Select(r => r.Name)
                        .Distinct()
                        .ToList();

                    cmbRoleFilter.Items.Clear();
                    cmbRoleFilter.Items.Add(new ComboBoxItem { Content = "Все роли", IsSelected = true });

                    foreach (var role in roles)
                    {
                        cmbRoleFilter.Items.Add(new ComboBoxItem { Content = role });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(emp =>
                    (emp.FullName != null && emp.FullName.ToLower().Contains(searchText)) ||
                    (emp.PhoneNumber != null && emp.PhoneNumber.ToLower().Contains(searchText)));
            }

            if (cmbRoleFilter.SelectedItem is ComboBoxItem selectedRole &&
                selectedRole.Content.ToString() != "Все роли")
            {
                filtered = filtered.Where(emp => emp.RoleName == selectedRole.Content.ToString());
            }

            lvEmployees.ItemsSource = filtered.ToList();
            tbStatus.Text = $"Найдено сотрудников: {filtered.Count()}";
        }

        private void btnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeeEditPage());
        }

        private void lvEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvEmployees.SelectedItem is EmployeeViewModel selectedEmployee)
            {
                NavigationService.Navigate(new EmployeeEditPage(selectedEmployee.EmployeeId));
            }
        }
    }
}

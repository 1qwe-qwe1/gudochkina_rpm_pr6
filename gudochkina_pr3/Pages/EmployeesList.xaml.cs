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
using System.Printing;

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
                .Where(u => u.Roles.Name.ToLower() == "Сотрудник" || u.Roles.Name.ToLower() == "Администратор")
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

        // Применение фильтров
        private void ApplyFilters()
        {
            if (_allEmployees == null) return;

            var filtered = _allEmployees.AsEnumerable();

            // Фильтр по поисковому тексту
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(emp =>
                    (emp.FullName != null && emp.FullName.ToLower().Contains(searchText)) ||
                    (emp.PhoneNumber != null && emp.PhoneNumber.ToLower().Contains(searchText)));
            }
            // Фильтр по роли
            if (cmbRoleFilter.SelectedItem is ComboBoxItem selectedRole &&
                selectedRole.Content.ToString() != "Все роли")
            {
                filtered = filtered.Where(emp => emp.RoleName == selectedRole.Content.ToString());
            }

            // Обновляем ListView и счетчик
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


        // Печать списка сотрудников в PDF
        private void PrintListButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FlowDocument printDoc = new FlowDocument();

                printDoc.PageWidth = 793.7;
                printDoc.PageHeight = 1122.5;
                printDoc.PagePadding = new Thickness(20, 40, 20, 40);

                Paragraph title = new Paragraph(new Run("Список сотрудников"))
                {
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                printDoc.Blocks.Add(title);

                int totalEmployees = lvEmployees.Items.Count;
                Paragraph countInfo = new Paragraph(new Run($"Всего сотрудников: {totalEmployees}"))
                {
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                printDoc.Blocks.Add(countInfo);

                // Создаем таблицу для данных
                Table table = new Table();
                table.CellSpacing = 0;
                table.FontSize = 11;

                // Определяем колонки таблицы
                table.Columns.Add(new TableColumn { Width = new GridLength(10, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(28, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(22, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(15, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(15, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(10, GridUnitType.Star) });

                // Группа для заголовка таблицы
                TableRowGroup headerGroup = new TableRowGroup();
                TableRow headerRow = new TableRow();
                headerRow.Background = Brushes.LightGray;

                string[] headers = { "№", "ФИО", "Должность", "Роль", "Телефон", "Статус" };
                foreach (string header in headers)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(header)));
                    cell.Padding = new Thickness(10, 8, 10, 8);
                    cell.TextAlignment = TextAlignment.Center;
                    ((Paragraph)cell.Blocks.FirstBlock).FontWeight = FontWeights.Bold;
                    ((Paragraph)cell.Blocks.FirstBlock).TextAlignment = TextAlignment.Center;
                    headerRow.Cells.Add(cell);
                }

                headerGroup.Rows.Add(headerRow);
                table.RowGroups.Add(headerGroup);

                // Группа для данных
                TableRowGroup dataGroup = new TableRowGroup();
                int number = 1;

                foreach (var employee in lvEmployees.Items)
                {
                    var emp = (EmployeeViewModel)employee;
                    TableRow row = new TableRow();

                    string[] rowData = {
                        number.ToString(),
                        emp.FullName ?? "",
                        emp.PostName ?? "",
                        emp.RoleName ?? "",
                        emp.PhoneNumber ?? "",
                        emp.IsActive == true ? "Активен" : "Неактивен"
                    };

                    for (int i = 0; i < rowData.Length; i++)
                    {
                        TableCell cell = new TableCell();
                        cell.Padding = new Thickness(10, 6, 10, 6);
                        cell.TextAlignment = TextAlignment.Center;

                        // Для статуса устанавливаем цвет
                        if (i == 5)
                        {
                            Paragraph statusParagraph = new Paragraph();
                            Run statusRun = new Run(rowData[i]);
                            statusRun.Foreground = (emp.IsActive == true) ? Brushes.Green : Brushes.Red;
                            statusParagraph.Inlines.Add(statusRun);
                            statusParagraph.TextAlignment = TextAlignment.Center;
                            cell.Blocks.Add(statusParagraph);
                        }
                        else
                        {
                            Paragraph paragraph = new Paragraph(new Run(rowData[i]));
                            paragraph.TextAlignment = TextAlignment.Center;
                            cell.Blocks.Add(paragraph);
                        }

                        row.Cells.Add(cell);
                    }

                    dataGroup.Rows.Add(row);
                    number++;
                }

                table.RowGroups.Add(dataGroup);

                printDoc.Blocks.Add(table);

                // Подвал с датой печати
                Paragraph footer = new Paragraph(new Run($"Дата печати: {DateTime.Now:dd.MM.yyyy HH:mm}"))
                {
                    FontSize = 9,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 20, 0, 0),
                    FontStyle = FontStyles.Italic
                };
                printDoc.Blocks.Add(footer);

                // Диалог печати
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    // Получаем размеры области печати
                    double printableWidth = printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea.ExtentWidth;
                    double printableHeight = printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea.ExtentHeight;

                    // Устанавливаем размеры для FlowDocument
                    printDoc.PageWidth = printableWidth;
                    printDoc.PageHeight = printableHeight;
                    printDoc.PagePadding = new Thickness(40);
                    printDoc.ColumnGap = 0;
                    printDoc.ColumnWidth = printableWidth;

                    // Отправляем на печать
                    IDocumentPaginatorSource idpSource = printDoc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, "Список сотрудников");

                    MessageBox.Show("Документ отправлен на печать.", "Успешно",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

    }
}

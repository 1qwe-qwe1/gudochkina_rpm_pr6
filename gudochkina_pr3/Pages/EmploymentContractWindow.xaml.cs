using gudochkina_pr3.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmploymentContractWindow.xaml
    /// </summary>
    public partial class EmploymentContractWindow : Window
    {
        private Employees _employee;

        public EmploymentContractWindow(Employees employee)
        {
            InitializeComponent();
            _employee = employee;

            EmployeeNameTextBlock.Text = $"{_employee.Surname} {_employee.Name} {_employee.Patronymic}";
            EmployeePositionTextBlock.Text = _employee.Posts?.PostName ?? "Не указана";

            ContractDayTextBox.Text = DateTime.Now.Day.ToString();
            ContractMonthTextBox.Text = GetMonthName(DateTime.Now.Month);
            ContractYearTextBox.Text = DateTime.Now.Year.ToString();

            ContractNumberTextBox.Text = GenerateContractNumber();
        }

        private string GenerateContractNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string employeeId = _employee.EmployeeId.ToString().PadLeft(3, '0');
            return $"ТД-{datePart}-{employeeId}";
        }

        private string GetMonthName(int month)
        {
            string[] months = { "Января", "Февраля", "Марта", "Апреля", "Мая", "Июня",
                                "Июля", "Августа", "Сентября", "Октября", "Ноября", "Декабря" };
            return months[month - 1];
        }

        private void CreateContractButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ContractNumberTextBox.Text))
                {
                    MessageBox.Show("Введите номер договора!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ContractNumberTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(EmployeeSalaryTextBox.Text))
                {
                    MessageBox.Show("Введите зарплату сотрудника!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmployeeSalaryTextBox.Focus();
                    return;
                }

                // Создаем словарь замен
                var replacements = new Dictionary<string, string>
                {
                    { "{ContractNumber}", ContractNumberTextBox.Text.Trim() },
                    { "{City}", CityTextBox.Text.Trim() },
                    { "{ContractDay}", ContractDayTextBox.Text.Trim() },
                    { "{ContractMonth}", ContractMonthTextBox.Text.Trim() },
                    { "{ContractYear}", ContractYearTextBox.Text.Trim() },
                    { "{EmployerName}", EmployerNameTextBox.Text.Trim() },
                    { "{DirectorName}", DirectorNameTextBox.Text.Trim() },
                    { "{EmployerAddress}", EmployerAddressTextBox.Text.Trim() },
                    { "{EmployerInnKpp}", EmployerInnKppTextBox.Text.Trim() },
                    { "{EmployeeName}", $"{_employee.Surname} {_employee.Name} {_employee.Patronymic}" },
                    { "{EmployeePosition}", _employee.Posts?.PostName ?? "Не указана" },
                    { "{DepartmentName}", "Отдел продаж" },
                    { "{TestPeriod}", TestPeriodTextBox.Text.Trim() },
                    { "{EmployeeSalary}", EmployeeSalaryTextBox.Text.Trim() },
                    { "{PassportSeries}", PassportSeriesTextBox.Text.Trim() },
                    { "{PassportIssuedBy}", PassportIssuedByTextBox.Text.Trim() },
                    { "{PassportIssueDate}", PassportIssueDateTextBox.Text.Trim() }
                };

                // Путь к шаблону
                string templatePath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Resources",
                    "blank-trudovogo-dogovora.docx");

                if (!File.Exists(templatePath))
                {
                    MessageBox.Show($"Шаблон не найден по пути:\n{templatePath}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Путь сохранения
                string employeeFullName = $"{_employee.Surname}_{_employee.Name}_{_employee.Patronymic}".Replace(" ", "_");
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string outputPath = System.IO.Path.Combine(desktopPath,
                    $"Трудовой_договор_{employeeFullName}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");

                // Загружаем документ и выполняем замены
                using (var document = DocX.Load(templatePath))
                {
                    foreach (var replacement in replacements)
                    {
                        var options = new StringReplaceTextOptions
                        {
                            SearchValue = replacement.Key,
                            NewValue = replacement.Value,
                            RemoveEmptyParagraph = true,
                            TrackChanges = false,
                            RegExOptions = RegexOptions.None
                        };

                        document.ReplaceText(options);
                    }

                    document.SaveAs(outputPath);
                }

                MessageBox.Show($"Трудовой договор успешно создан!\nСохранен по пути:\n{outputPath}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании договора: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

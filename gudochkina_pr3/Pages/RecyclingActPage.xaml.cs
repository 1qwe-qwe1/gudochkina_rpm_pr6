using gudochkina_pr3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для RecyclingActPage.xaml
    /// </summary>
    public partial class RecyclingActPage : Page
    {
        private int _actId;
        private ActViewModel _act;

        public RecyclingActPage(int actId)
        {
            InitializeComponent();
            _actId = actId;
            this.Loaded += RecyclingActPrintPage_Loaded;
        }

        private void RecyclingActPrintPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadActData();
            BuildPrintDocument();
        }

        private void LoadActData()
        {
            try
            {
                using (var db = new Entities1())
                {
                    var act = (from a in db.RecyclingActs
                               join contract in db.RecyclingContracts on a.ContractId equals contract.ContractId
                               join counterparty in db.RecyclingCompanies on contract.CompanyId equals counterparty.CompanyId
                               join wasteType in db.WasteTypes on a.WasteTypeId equals wasteType.WasteTypeId
                               where a.ActId == _actId
                               select new ActViewModel
                               {
                                   ActId = a.ActId,
                                   ContractId = a.ContractId,
                                   ContractNumber = contract.ContractNumber,
                                   ActDate = a.ActDate,
                                   WasteTypeId = a.WasteTypeId,
                                   WasteTypeName = wasteType.WasteTypeName,
                                   Volume = a.Volume,
                                   Cost = a.Cost,
                                   Notes = a.Notes,
                                   CounterpartyName = counterparty.CompanyName
                               }).FirstOrDefault();

                    _act = act;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки акта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildPrintDocument()
        {
            if (_act == null) return;

            printContent.Children.Clear();

            var textStyle = new Style(typeof(TextBlock));
            textStyle.Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Times New Roman")));

            printContent.Resources.Add(typeof(TextBlock), textStyle);

            // Заголовок документа
            var titleBlock = new TextBlock
            {
                Text = "АКТ ПРИЕМА-ПЕРЕДАЧИ ОТХОДОВ",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            printContent.Children.Add(titleBlock);

            // Номер и дата
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var actNumberBlock = new TextBlock
            {
                Text = $"№ {_act.ActId}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetColumn(actNumberBlock, 0);
            headerGrid.Children.Add(actNumberBlock);

            var dateBlock = new TextBlock
            {
                Text = $"г. Санкт-Петербург, {_act.ActDate:dd.MM.yyyy}",
                FontSize = 14,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetColumn(dateBlock, 1);
            headerGrid.Children.Add(dateBlock);

            printContent.Children.Add(headerGrid);

            // Стороны договора
            var partiesBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 15)
            };

            var partiesStack = new StackPanel();

            partiesStack.Children.Add(new TextBlock
            {
                Text = "Стороны договора:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            partiesStack.Children.Add(new TextBlock
            {
                Text = $"Сдатчик: ООО \"Фотоцентр\"",
                Margin = new Thickness(20, 0, 0, 3)
            });

            partiesStack.Children.Add(new TextBlock
            {
                Text = $"Переработчик: {_act.CounterpartyName}",
                Margin = new Thickness(20, 0, 0, 3)
            });

            partiesStack.Children.Add(new TextBlock
            {
                Text = $"Договор №: {_act.ContractNumber}",
                Margin = new Thickness(20, 0, 0, 0)
            });

            partiesBorder.Child = partiesStack;
            printContent.Children.Add(partiesBorder);

            // Таблица с данными акта
            var dataGrid = new Grid();
            dataGrid.Margin = new Thickness(0, 0, 0, 20);
            dataGrid.ShowGridLines = true;

            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            dataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Заголовки таблицы
            string[] headers = { "№", "Тип отходов", "Объем (кг)", "Стоимость", "Примечания" };
            for (int i = 0; i < headers.Length; i++)
            {
                var headerCell = new Border
                {
                    Background = Brushes.LightGray,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(5)
                };
                headerCell.Child = new TextBlock
                {
                    Text = headers[i],
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(headerCell, 0);
                Grid.SetColumn(headerCell, i);
                dataGrid.Children.Add(headerCell);
            }

            // Данные строки
            var dataRow = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };
            Grid.SetRow(dataRow, 1);
            Grid.SetColumn(dataRow, 0);
            dataGrid.Children.Add(dataRow);
            dataRow.Child = new TextBlock
            {
                Text = "1",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var wasteTypeCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };
            Grid.SetRow(wasteTypeCell, 1);
            Grid.SetColumn(wasteTypeCell, 1);
            dataGrid.Children.Add(wasteTypeCell);
            wasteTypeCell.Child = new TextBlock
            {
                Text = _act.WasteTypeName,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var volumeCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };
            Grid.SetRow(volumeCell, 1);
            Grid.SetColumn(volumeCell, 2);
            dataGrid.Children.Add(volumeCell);
            volumeCell.Child = new TextBlock
            {
                Text = _act.Volume?.ToString("0.00") ?? "0.00",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var costCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };
            Grid.SetRow(costCell, 1);
            Grid.SetColumn(costCell, 3);
            dataGrid.Children.Add(costCell);
            costCell.Child = new TextBlock
            {
                Text = _act.Cost?.ToString("C2") ?? "0.00 руб.",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var notesCell = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };
            Grid.SetRow(notesCell, 1);
            Grid.SetColumn(notesCell, 4);
            dataGrid.Children.Add(notesCell);
            notesCell.Child = new TextBlock
            {
                Text = _act.Notes ?? "-",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            // Добавляем строки в сетку
            for (int i = 0; i < 2; i++)
            {
                dataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            printContent.Children.Add(dataGrid);

            // Итоговая сумма
            var totalBorder = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 10, 0, 20)
            };

            var totalStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };

            totalStack.Children.Add(new TextBlock
            {
                Text = $"Итого к оплате: {_act.Cost?.ToString("C2") ?? "0.00 руб."}",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            });

            totalBorder.Child = totalStack;
            printContent.Children.Add(totalBorder);

            // Подписи
            var signaturesGrid = new Grid();
            signaturesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            signaturesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            signaturesGrid.Margin = new Thickness(0, 20, 0, 0);

            // Левая подпись (Сдатчик)
            var leftSignature = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            leftSignature.Children.Add(new TextBlock
            {
                Text = "Сдатчик:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });
            leftSignature.Children.Add(new TextBlock
            {
                Text = "________________________",
                Margin = new Thickness(0, 0, 0, 5)
            });
            leftSignature.Children.Add(new TextBlock
            {
                Text = "/ ООО \"Фотоцентр\" /",
                FontSize = 10
            });
            Grid.SetColumn(leftSignature, 0);
            signaturesGrid.Children.Add(leftSignature);

            // Правая подпись (Переработчик)
            var rightSignature = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            rightSignature.Children.Add(new TextBlock
            {
                Text = "Переработчик:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });
            rightSignature.Children.Add(new TextBlock
            {
                Text = "________________________",
                Margin = new Thickness(0, 0, 0, 5)
            });
            rightSignature.Children.Add(new TextBlock
            {
                Text = $"/ {_act.CounterpartyName} /",
                FontSize = 10
            });
            Grid.SetColumn(rightSignature, 1);
            signaturesGrid.Children.Add(rightSignature);

            printContent.Children.Add(signaturesGrid);

            // Подвал с датой
            var footer = new TextBlock
            {
                Text = $"Дата печати: {DateTime.Now:dd.MM.yyyy HH:mm}",
                FontSize = 9,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 30, 0, 0),
                FontStyle = FontStyles.Italic
            };
            printContent.Children.Add(footer);

            tbStatus.Text = $"Акт №{_act.ActId} от {_act.ActDate:dd.MM.yyyy} загружен";
        }

        private void PrintActButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();

                printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;

                if (printDialog.ShowDialog() == true)
                {
                    // Создаем копию FlowDocument для печати
                    FlowDocument printDoc = new FlowDocument();

                    printDoc.FontFamily = new FontFamily("Times New Roman");

                    // Копируем содержимое
                    var container = new BlockUIContainer();
                    var clonedContent = new StackPanel();

                    foreach (var child in printContent.Children)
                    {
                        if (child is FrameworkElement element)
                        {
                            clonedContent.Children.Add(CloneElement(element));
                        }
                    }

                    container.Child = clonedContent;
                    printDoc.Blocks.Add(container);

                    double printableWidth = printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea.ExtentWidth;
                    double printableHeight = printDialog.PrintQueue.GetPrintCapabilities().PageImageableArea.ExtentHeight;

                    printDoc.PageWidth = printableHeight;
                    printDoc.PageHeight = printableWidth;
                    printDoc.PagePadding = new Thickness(40);
                    printDoc.ColumnGap = 0;
                    printDoc.ColumnWidth = printableHeight;

                    IDocumentPaginatorSource idpSource = printDoc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, $"Акт переработки №{_act.ActId}");

                    MessageBox.Show("Акт отправлен на печать.", "Успешно",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FrameworkElement CloneElement(FrameworkElement original)
        {
            // Сохраняем в XAML
            string xaml = System.Windows.Markup.XamlWriter.Save(original);
            var cloned = System.Windows.Markup.XamlReader.Parse(xaml) as FrameworkElement;

            SetFontFamilyRecursive(cloned, new FontFamily("Times New Roman"));

            return cloned;
        }

        private void SetFontFamilyRecursive(DependencyObject element, FontFamily fontFamily)
        {
            if (element == null) return;

            if (element is TextBlock textBlock)
            {
                textBlock.FontFamily = fontFamily;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                SetFontFamilyRecursive(child, fontFamily);
            }
        }
    }
}

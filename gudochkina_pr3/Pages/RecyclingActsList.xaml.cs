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

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для RecyclingActsList.xaml
    /// </summary>
    public partial class RecyclingActsList : Page
    {
        private List<ActViewModel> _allActs;

        public RecyclingActsList()
        {
            InitializeComponent();
            this.Loaded += RecyclingActsList_Loaded;
        }

        private void RecyclingActsList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadActs();
        }

        private void LoadActs()
        {
            try
            {
                using (var db = new Entities1())
                {
                    var acts = from act in db.RecyclingActs
                               join contract in db.RecyclingContracts on act.ContractId equals contract.ContractId
                               join counterparty in db.RecyclingCompanies on contract.CompanyId equals counterparty.CompanyId
                               join wasteType in db.WasteTypes on act.WasteTypeId equals wasteType.WasteTypeId
                               select new ActViewModel
                               {
                                   ActId = act.ActId,
                                   ContractId = act.ContractId,
                                   ContractNumber = contract.ContractNumber,
                                   //ActDate = act.ActDate ?? DateTime.Now,
                                   WasteTypeId = act.WasteTypeId,
                                   WasteTypeName = wasteType.WasteTypeName,
                                   Volume = act.Volume,
                                   Cost = act.Cost,
                                   Notes = act.Notes,
                                   CounterpartyName = counterparty.CompanyName
                               };

                    _allActs = acts.ToList();
                    lvActs.ItemsSource = _allActs;
                    tbStatus.Text = $"Всего актов: {_allActs.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvActs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvActs.SelectedItem is ActViewModel selectedAct)
            {
             //   NavigationService.Navigate(new RecyclingActPrintPage(selectedAct.ActId));
            }
        }
    }
}

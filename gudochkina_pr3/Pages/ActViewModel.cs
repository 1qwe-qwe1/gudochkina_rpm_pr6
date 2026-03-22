using System;

namespace gudochkina_pr3.Pages
{
public partial class Autho
    {
        public class ActViewModel
        {
            public int ActId { get; set; }
            public int? ContractId { get; set; }
            public string ContractNumber { get; set; }
            public DateTime ActDate { get; set; }
            public string ActDateFormatted => ActDate.ToString("dd.MM.yyyy");
            public int? WasteTypeId { get; set; }
            public string WasteTypeName { get; set; }
            public decimal? Volume { get; set; }
            public string VolumeFormatted => Volume?.ToString("0.00") + " кг";
            public decimal? Cost { get; set; }
            public string CostFormatted => Cost?.ToString("C2");
            public string Notes { get; set; }
            public string CounterpartyName { get; set; }
        }
    }
}

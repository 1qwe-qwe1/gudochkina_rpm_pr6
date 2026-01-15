using System;

namespace gudochkina_pr3.Pages
{
public partial class Autho
    {
        public class EmployeeViewModel
        {
            public int EmployeeId { get; set; }
            public string Surname { get; set; }
            public string Name { get; set; }
            public string Patronymic { get; set; }
            public string FullName => $"{Surname} {Name} {Patronymic}";
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string RoleName { get; set; }
            public string PostName { get; set; }
            public bool? IsActive { get; set; }
            public string Login { get; set; }
            public byte[] Photo { get; set; }
            public int? UserId { get; set; }
            public DateTime HireDate { get; set; }
            public int? PostId { get; set; }
        }
    }
}

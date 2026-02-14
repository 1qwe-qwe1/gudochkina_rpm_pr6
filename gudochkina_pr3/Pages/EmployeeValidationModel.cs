using System.ComponentModel.DataAnnotations;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Модель для валидации данных сотрудника
    /// </summary>
    public class EmployeeValidationModel
    {
        [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна содержать от 2 до 50 символов")]
        [RegularExpression(@"^[А-ЯЁа-яёA-Za-z\s\-]+$", ErrorMessage = "Фамилия может содержать только буквы, пробелы и дефисы")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно содержать от 2 до 50 символов")]
        [RegularExpression(@"^[А-ЯЁа-яёA-Za-z\s\-]+$", ErrorMessage = "Имя может содержать только буквы, пробелы и дефисы")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Отчество не должно превышать 50 символов")]
        [RegularExpression(@"^[А-ЯЁа-яёA-Za-z\s\-]*$", ErrorMessage = "Отчество может содержать только буквы, пробелы и дефисы")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Телефон обязателен для заполнения")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Телефон должен содержать от 5 до 20 символов")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Логин обязателен для заполнения")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен содержать от 3 до 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Логин может содержать только латинские буквы, цифры и подчеркивание")]
        public string Login { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Статус обязателен для выбора")]
        public string Status { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите должность")]
        public int? PostId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите роль")]
        public int? RoleId { get; set; }

        public byte[] Photo { get; set; }
    }
}
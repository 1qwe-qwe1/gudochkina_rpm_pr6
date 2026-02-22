using gudochkina_pr3.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Валидатор для сотрудника
    /// </summary>
    public class EmployeeValidator
    {
        public List<ValidationError> Validate(EmployeeValidationModel model, bool isNewEmployee, int? existingUserId = null)
        {
            var errors = new List<ValidationError>();
            var validationContext = new ValidationContext(model);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        if (memberName == "Password" && !isNewEmployee)
                            continue;

                        errors.Add(new ValidationError
                        {
                            PropertyName = memberName,
                            ErrorMessage = validationResult.ErrorMessage
                        });
                    }
                }
            }

            if (isNewEmployee)
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    errors.Add(new ValidationError
                    {
                        PropertyName = "Password",
                        ErrorMessage = "Пароль обязателен для нового сотрудника"
                    });
                }
                else if (model.Password.Length < 6)
                {
                    errors.Add(new ValidationError
                    {
                        PropertyName = "Password",
                        ErrorMessage = "Пароль должен содержать минимум 6 символов"
                    });
                }
            }

            // Проверка уникальности логина
            using (var db = new Entities1())
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Login == model.Login);
                if (existingUser != null)
                {
                    if (existingUserId.HasValue && existingUser.Id != existingUserId.Value)
                    {
                        errors.Add(new ValidationError
                        {
                            PropertyName = "Login",
                            ErrorMessage = "Пользователь с таким логином уже существует"
                        });
                    }
                    else if (!existingUserId.HasValue)
                    {
                        errors.Add(new ValidationError
                        {
                            PropertyName = "Login",
                            ErrorMessage = "Пользователь с таким логином уже существует"
                        });
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                var phoneDigits = new string(model.PhoneNumber.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 10)
                {
                    errors.Add(new ValidationError
                    {
                        PropertyName = "PhoneNumber",
                        ErrorMessage = "Номер телефона должен содержать минимум 10 цифр"
                    });
                }
            }

            return errors;
        }
    }
}
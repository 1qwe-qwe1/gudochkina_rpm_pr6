using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gudochkina_pr3.Services
{
    internal class TimeHelper
    {
        // Метод для определения времени суток по текущему времени
        public static string GetTimeOfDayGreeting()
        {
            var hour = DateTime.Now.Hour;

            if (hour >= 10 && hour < 12)
                return "Доброе утро";
            else if (hour >= 12 && hour < 17)
                return "Добрый день";
            else if (hour >= 17 && hour <= 19)
                return "Добрый вечер";
            else
                return "Добрый день"; // По умолчанию, если время вне указанных диапазонов
        }

        // Метод для проверки, находится ли текущее время в рабочем интервале (10:00-19:00)
        public static bool IsWithinWorkingHours()
        {
            var now = DateTime.Now.TimeOfDay;
            var start = new TimeSpan(10, 0, 0);
            var end = new TimeSpan(19, 0, 0);

            return now >= start && now <= end;
        }

        // Метод для получения полного приветствия с ФИО
        public static string GetFullGreeting(string surname, string name, string patronymic = null)
        {
            string timeOfDay = GetTimeOfDayGreeting();
            string fio = $"{surname} {name}";

            if (!string.IsNullOrWhiteSpace(patronymic))
                fio += $" {patronymic}";

            return $"{timeOfDay}!\n{fio}";
        }
    }
}

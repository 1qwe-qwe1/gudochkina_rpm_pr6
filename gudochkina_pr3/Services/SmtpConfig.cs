using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace gudochkina_pr3.Services
{
    public static class SmtpConfig
    {
        public static string SmtpServer => ConfigurationManager.AppSettings["SmtpServer"];
        public static int SmtpPort => int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
        public static string SmtpUsername => ConfigurationManager.AppSettings["SmtpUsername"];
        public static string SmtpPassword => ConfigurationManager.AppSettings["SmtpPassword"];
        public static string FromEmail => ConfigurationManager.AppSettings["FromEmail"];
        public static string FromName => ConfigurationManager.AppSettings["FromName"];
    }
}

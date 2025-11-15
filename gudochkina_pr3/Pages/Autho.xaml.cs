using gudochkina_pr3.Models;
using gudochkina_pr3.Services;
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
using System.Data.Entity;

namespace gudochkina_pr3.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autho.xaml
    /// </summary>
    public partial class Autho : Page
    {
        int click;
        public Autho()
        {
            InitializeComponent();
            click = 0;
        }

        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client(null, null));
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            click += 1;
            string login = tbLogin.Text.Trim();
            string password = Hash.HashPassword(tbPassword.Text.Trim());

            Entities1 db = Entities1.GetContext();

            var user = db.Users
           .Include(u => u.Roles)
           .FirstOrDefault(x => x.Login == login && x.PasswordHash == password);
           

            if (click == 1)
            {
                if (user != null)
                {
                    MessageBox.Show("Вы вошли под: " + user.Roles.Name.ToString());
                    LoadPage(user.Roles.Name.ToString(), user);
                }
                else
                {
                    MessageBox.Show("Вы ввели логин или пароль неверно!");
                    GenerateCapctcha();

                    tbPassword.Clear();

                    string captchaText = CaptchaGenerator.GenerateCaptchaText(6);

                    tblCaptcha.Visibility = Visibility.Visible;
                    tbCaptcha.Visibility = Visibility.Visible;

                }

            }

            else if (click > 1)
            {
                if (user != null && tbCaptcha.Text == tblCaptcha.Text)
                {
                    MessageBox.Show("Вы вошли под: " + user.Roles.Name.ToString());
                    LoadPage(user.Roles.Name.ToString(), user);
                }

                else
                {
                    MessageBox.Show("Введите данные заново!");
                }
            }
        }

        private void GenerateCapctcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;

            string capctchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = capctchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }
        private void LoadPage(string _role, Users user)

        {
            click = 0;

            NavigationService.Navigate(new Client(user, _role));

        }
    }
}

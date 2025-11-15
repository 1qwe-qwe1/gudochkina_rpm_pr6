using gudochkina_pr3.Pages;
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

namespace gudochkina_pr3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FrmMain.Content = new Autho();
            FrmMain.Navigated += FrmMain_Navigated;
        }
        private void FrmMain_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Autho)
            {
                ClearAuthoFields();
            }
       
        }

        private void ClearAuthoFields()
        {
            if (FrmMain.Content is Autho authoPage)
            {
                authoPage.tbLogin.Text = "";
                authoPage.tbPassword.Text = "";
                authoPage.tbCaptcha.Text = "";
                authoPage.tblCaptcha.Visibility = Visibility.Collapsed;
                authoPage.tbCaptcha.Visibility = Visibility.Collapsed;
            }
        }
        private void FrmMain_ContentRendered(object sender, EventArgs e)
        {
            if (FrmMain.CanGoBack)
                btnBack.Visibility = Visibility.Visible;
            else 
                btnBack.Visibility = Visibility.Hidden;

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            FrmMain.GoBack();
        }
    }
}

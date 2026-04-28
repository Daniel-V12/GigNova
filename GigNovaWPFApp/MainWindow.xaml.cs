using System.Windows;
using GigNovaWPFApp.UserControls;

namespace GigNovaWPFApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Content = new HomePage();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new HomePage();
        }

        private void ViewCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new CatalogPage();
        }
    }
}
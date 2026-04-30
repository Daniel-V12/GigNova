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
            CatalogPage page = new CatalogPage();
            page.GigSelected += OpenSelectedGig;
            MainFrame.Content = page;
        }

        public void OpenSelectedGig(string gigId)
        {
            SelectedGigPage page = new SelectedGigPage(gigId);
            MainFrame.Content = page;
        }
    }
}
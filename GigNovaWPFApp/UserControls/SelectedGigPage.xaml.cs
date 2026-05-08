using GigNovaModels.ViewModels;
using GigNovaWSClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GigNovaWPFApp.UserControls
{
    public partial class SelectedGigPage : UserControl
    {
        private const string ImagesBaseUrl = "http://localhost:7059/Images/";
        private string gigId;

        public SelectedGigPage(string gigId)
        {
            InitializeComponent();
            this.gigId = gigId;
            LoadGig();
        }

        private async void LoadGig()
        {
            ApiClient<SelectedGigViewModel> client = new ApiClient<SelectedGigViewModel>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            client.Path = "api/Guest/GetSelectedGigViewModel";
            client.AddParameter("gig_id", gigId);

            SelectedGigViewModel model = await client.GetAsync();
            if (model == null || model.gig == null)
            {
                MessageBox.Show("Gig was not found.", "GigNova");
                return;
            }

            GigNameText.Text = model.gig.Gig_name;
            GigDescriptionText.Text = model.gig.Gig_description;
            GigPriceText.Text = "$" + model.gig.Gig_price.ToString("0");
            GigImageBorder.Background = MakeImageBrush(model.gig.Gig_photo);

            CategoriesWrap.Children.Clear();
            if (model.gig.Category_id != null && model.gig.Category_id.Trim() != "")
            {
                string[] parts = model.gig.Category_id.Split(',');
                foreach (string part in parts)
                {
                    string t = part.Trim();
                    if (t != "") CategoriesWrap.Children.Add(MakeChip(t));
                }
            }

            if (model.seller != null)
            {
                SellerNameText.Text = model.seller.Seller_display_name;
                SellerAvatar.Fill = MakeImageBrush(model.seller.Seller_avatar);
            }
        }

        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Window.GetWindow(this);
            main.OpenGigReviews(this.gigId);
        }

        private ImageBrush MakeImageBrush(string fileName)
        {
            if (fileName == null || fileName.Trim() == "") return null;
            BitmapImage bmp = new BitmapImage(new Uri(ImagesBaseUrl + fileName, UriKind.Absolute));
            ImageBrush brush = new ImageBrush(bmp);
            brush.Stretch = Stretch.UniformToFill;
            return brush;
        }

        private Border MakeChip(string text)
        {
            Border chip = new Border();
            chip.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A"));
            chip.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B83C5A"));
            chip.BorderThickness = new Thickness(1);
            chip.CornerRadius = new CornerRadius(10);
            chip.Padding = new Thickness(10, 4, 10, 4);
            chip.Margin = new Thickness(0, 0, 6, 6);

            TextBlock txt = new TextBlock();
            txt.Text = text;
            txt.Foreground = Brushes.White;
            txt.FontSize = 12;
            txt.FontWeight = FontWeights.Bold;

            chip.Child = txt;
            return chip;
        }
    }
}
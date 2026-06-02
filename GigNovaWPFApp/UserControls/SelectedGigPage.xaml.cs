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
        // Where all gig / seller images are served from (the WS's /Images folder).
        private const string ImagesBaseUrl = "http://localhost:7059/Images/";

        private string gigId;


        // ============================== Initialization ==============================

        public SelectedGigPage(string gigId)
        {
            InitializeComponent();
            this.gigId = gigId;
            LoadGig();
        }


        // ============================== Loading + Rendering ==============================

        // Fetch the gig from the WS and fill the UI elements.
        private async void LoadGig()
        {
            ApiClient<SelectedGigViewModel> client = WpfHelpers.BuildClient<SelectedGigViewModel>("api/Guest/GetSelectedGigViewModel");
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

            // Render the comma-separated category ids as chips (one chip per id).
            CategoriesWrap.Children.Clear();
            if (model.gig.Category_id != null && model.gig.Category_id.Trim() != "")
            {
                string[] parts = model.gig.Category_id.Split(',');
                foreach (string part in parts)
                {
                    string t = part.Trim();
                    if (t != "")
                    {
                        CategoriesWrap.Children.Add(MakeChip(t));
                    }
                }
            }

            // Seller side card.
            if (model.seller != null)
            {
                SellerNameText.Text = model.seller.Seller_display_name;
                SellerAvatar.Fill = MakeImageBrush(model.seller.Seller_avatar);
            }
        }

        // Navigate to the reviews page for this gig (handled by the MainWindow).
        private void ReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Window.GetWindow(this);
            main.OpenGigReviews(this.gigId);
        }


        // ============================== Helpers ==============================

        // Builds an ImageBrush from a filename in the WS's /Images folder. Returns null if no filename.
        private ImageBrush MakeImageBrush(string fileName)
        {
            if (fileName == null || fileName.Trim() == "")
            {
                return null;
            }
            BitmapImage bmp = new BitmapImage(new Uri(ImagesBaseUrl + fileName, UriKind.Absolute));
            ImageBrush brush = new ImageBrush(bmp);
            brush.Stretch = Stretch.UniformToFill;
            return brush;
        }

        // Builds one category chip (dark pill with white text) for the categories row.
        private Border MakeChip(string text)
        {
            Border chip = new Border();
            chip.Background = WpfHelpers.Brush("#2A2A2A");
            chip.BorderBrush = WpfHelpers.Brush("#B83C5A");
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

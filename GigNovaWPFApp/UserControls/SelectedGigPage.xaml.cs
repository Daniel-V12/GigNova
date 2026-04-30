using GigNovaModels.ViewModels;
using GigNovaWSClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GigNovaWPFApp.UserControls
{
    public partial class SelectedGigPage : UserControl
    {
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

            GigNameTitle.Text = model.gig.Gig_name;
            GigNameText.Text = model.gig.Gig_name;
            GigDescriptionText.Text = model.gig.Gig_description;
            GigPriceText.Text = "Starting at $" + model.gig.Gig_price.ToString("0");

            CategoriesWrap.Children.Clear();
            if (model.gig.Category_id != null && model.gig.Category_id.Trim() != "")
            {
                string[] parts = model.gig.Category_id.Split(',');
                foreach (string part in parts)
                {
                    string t = part.Trim();
                    if (t != "")
                    {
                        Border chip = new Border();
                        chip.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A"));
                        chip.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                        chip.BorderThickness = new Thickness(1);
                        chip.CornerRadius = new CornerRadius(8);
                        chip.Padding = new Thickness(8, 4, 8, 4);
                        chip.Margin = new Thickness(0, 0, 8, 8);
                        TextBlock txt = new TextBlock();
                        txt.Text = t;
                        txt.Foreground = Brushes.White;
                        txt.FontWeight = FontWeights.Bold;
                        chip.Child = txt;
                        CategoriesWrap.Children.Add(chip);
                    }
                }
            }

            if (model.seller != null)
            {
                SellerNameText.Text = model.seller.Seller_display_name;
                SellerRatingText.Text = "Rating: " + model.Review.ToString("0.0");
            }
        }
    }
}
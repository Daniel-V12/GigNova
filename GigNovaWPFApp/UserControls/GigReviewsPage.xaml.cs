using GigNovaModels.Models;
using GigNovaWSClient;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GigNovaWPFApp.UserControls
{
    public partial class GigReviewsPage : UserControl
    {
        private string gigId;


        // ============================== Initialization ==============================

        public GigReviewsPage(string gigId)
        {
            InitializeComponent();
            this.gigId = gigId;
            LoadReviews();
        }


        // ============================== Loading + Rendering ==============================

        // Fetch the reviews from the WS, compute the average, and build one card per review.
        private async void LoadReviews()
        {
            ApiClient<List<Review>> client = WpfHelpers.BuildClient<List<Review>>("api/Guest/ViewGigReviews");
            client.AddParameter("gig_id", gigId);

            List<Review> reviews = await client.GetAsync();
            int count = 0;
            if (reviews != null)
            {
                count = reviews.Count;
            }

            // Compute the average rating.
            double average = 0;
            if (count > 0)
            {
                double sum = 0;
                foreach (Review r in reviews)
                {
                    sum += r.Review_rating;
                }
                average = sum / count;
            }

            HeaderText.Text = "Gig Reviews (" + count + ")";
            AverageText.Text = "Overall average rating: " + average.ToString("0.0");

            // Rebuild the list of review cards.
            ReviewsList.Children.Clear();
            if (count == 0)
            {
                ReviewsList.Children.Add(MakeEmptyState());
                return;
            }
            foreach (Review r in reviews)
            {
                ReviewsList.Children.Add(MakeReviewCard(r));
            }
        }

        // Builds one review card: rating + comment + date on the left, "Remove" button on the right.
        private Border MakeReviewCard(Review review)
        {
            Border card = new Border();
            card.Background = WpfHelpers.Brush("#1A1A1A");
            card.BorderBrush = WpfHelpers.Brush("#B83C5A");
            card.BorderThickness = new Thickness(1);
            card.CornerRadius = new CornerRadius(10);
            card.Padding = new Thickness(18);
            card.Margin = new Thickness(0, 0, 0, 12);

            Grid grid = new Grid();
            ColumnDefinition col0 = new ColumnDefinition();
            col0.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition col1 = new ColumnDefinition();
            col1.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(col0);
            grid.ColumnDefinitions.Add(col1);

            // Left column: rating + comment + date stacked vertically.
            StackPanel panel = new StackPanel();
            Grid.SetColumn(panel, 0);

            TextBlock rating = new TextBlock();
            rating.Text = "Rating: " + review.Review_rating + " / 5";
            rating.Foreground = Brushes.White;
            rating.FontSize = 24;
            rating.FontWeight = FontWeights.Bold;
            rating.Margin = new Thickness(0, 0, 0, 6);
            panel.Children.Add(rating);

            TextBlock comment = new TextBlock();
            comment.Text = review.Review_comment;
            comment.Foreground = WpfHelpers.Brush("#D3D3D3");
            comment.FontSize = 15;
            comment.TextWrapping = TextWrapping.Wrap;
            comment.Margin = new Thickness(0, 0, 0, 8);
            panel.Children.Add(comment);

            TextBlock date = new TextBlock();
            date.Text = review.Review_creation_date;
            date.Foreground = WpfHelpers.Brush("#BFBFBF");
            date.FontSize = 13;
            date.FontWeight = FontWeights.SemiBold;
            panel.Children.Add(date);

            grid.Children.Add(panel);

            // Right column: "Remove" button. Stores the review id in its Tag.
            Button removeButton = new Button();
            removeButton.Style = (Style)FindResource("NiceButtonStyle");
            removeButton.Content = "Remove";
            removeButton.Background = WpfHelpers.Brush("#E94560");
            removeButton.Width = 100;
            removeButton.Height = 36;
            removeButton.VerticalAlignment = VerticalAlignment.Top;
            removeButton.Tag = review.Review_id;
            removeButton.Click += RemoveReview_Click;
            Grid.SetColumn(removeButton, 1);
            grid.Children.Add(removeButton);

            card.Child = grid;
            return card;
        }

        // Remove button click: confirm with the user, then call the WS Admin/RemoveGigReview endpoint.
        private async void RemoveReview_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string reviewId = btn.Tag.ToString();

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this review?",
                "Confirm Delete",
                MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            ApiClient<bool> client = WpfHelpers.BuildClient<bool>("api/Admin/RemoveGigReview");
            client.AddParameter("review_id", reviewId);

            bool ok = await client.PostAsync(false);
            if (ok == false)
            {
                MessageBox.Show("Failed to delete review.", "GigNova");
                return;
            }
            LoadReviews();
        }

        // The "no reviews yet" card shown when the list is empty.
        private Border MakeEmptyState()
        {
            Border empty = new Border();
            empty.Background = WpfHelpers.Brush("#1A1A1A");
            empty.BorderBrush = WpfHelpers.Brush("#B83C5A");
            empty.BorderThickness = new Thickness(1);
            empty.CornerRadius = new CornerRadius(10);
            empty.Padding = new Thickness(20);

            TextBlock txt = new TextBlock();
            txt.Text = "No reviews yet for this gig.";
            txt.Foreground = Brushes.White;
            txt.FontSize = 20;
            txt.FontWeight = FontWeights.Bold;
            txt.HorizontalAlignment = HorizontalAlignment.Center;

            empty.Child = txt;
            return empty;
        }
    }
}

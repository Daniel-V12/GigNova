using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GigNovaModels.Models;

namespace GigNovaWPFApp.UserControls
{
    public partial class CatalogPage : UserControl
    {
        private readonly List<Category> categories = new();
        private readonly List<Delivery_time> deliveryTimes = new();
        private readonly List<Language> languages = new();
        private readonly List<CatalogGigItem> allGigs = new();
        private readonly HashSet<string> selectedCategoryIds = new();

        private int currentPage = 1;
        private const int gigsPerPage = 6;

        public CatalogPage()
        {
            InitializeComponent();
            SeedData();
            BuildCategoryButtons();
            SetupFilters();
            RefreshCatalog();
        }

        private void SeedData()
        {
            categories.Add(new Category { Category_id = "1", Category_name = "Design" });
            categories.Add(new Category { Category_id = "2", Category_name = "Development" });
            categories.Add(new Category { Category_id = "3", Category_name = "Writing" });
            categories.Add(new Category { Category_id = "4", Category_name = "Marketing" });

            deliveryTimes.Add(new Delivery_time { Delivery_time_id = "1", Delivery_time_name = "1 day" });
            deliveryTimes.Add(new Delivery_time { Delivery_time_id = "2", Delivery_time_name = "3 days" });
            deliveryTimes.Add(new Delivery_time { Delivery_time_id = "3", Delivery_time_name = "7 days" });

            languages.Add(new Language { Language_id = "1", Language_name = "English" });
            languages.Add(new Language { Language_id = "2", Language_name = "Spanish" });
            languages.Add(new Language { Language_id = "3", Language_name = "French" });

            allGigs.Add(new CatalogGigItem("1", "Logo Design", "I will create a modern logo for your brand.", 45, "1", "1", 5));
            allGigs.Add(new CatalogGigItem("2", "Landing Page", "I will build a responsive landing page.", 120, "2", "1", 5));
            allGigs.Add(new CatalogGigItem("3", "SEO Article", "I will write a high-quality SEO article.", 30, "3", "2", 4));
            allGigs.Add(new CatalogGigItem("4", "Social Media Kit", "I will design social media templates.", 60, "4", "1", 4));
            allGigs.Add(new CatalogGigItem("5", "API Integration", "I will integrate API endpoints into your app.", 150, "2", "1", 5));
            allGigs.Add(new CatalogGigItem("6", "Brand Guide", "I will prepare a complete brand style guide.", 95, "1", "3", 5));
            allGigs.Add(new CatalogGigItem("7", "Product Description", "I will write product descriptions for your store.", 28, "3", "1", 4));
            allGigs.Add(new CatalogGigItem("8", "Ad Campaign", "I will set up and optimize ad campaigns.", 140, "4", "2", 5));
        }

        private void BuildCategoryButtons()
        {
            CategoryWrapPanel.Children.Clear();

            foreach (Category category in categories)
            {
                CheckBox checkBox = new CheckBox
                {
                    Content = category.Category_name,
                    Tag = category.Category_id,
                    Margin = new Thickness(0, 0, 10, 10),
                    Padding = new Thickness(10, 6, 10, 6),
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")),
                    BorderThickness = new Thickness(1)
                };

                checkBox.Checked += CategoryChanged;
                checkBox.Unchecked += CategoryChanged;
                CategoryWrapPanel.Children.Add(checkBox);
            }
        }

        private void SetupFilters()
        {
            DeliveryComboBox.Items.Clear();
            DeliveryComboBox.Items.Add(new ComboBoxItem { Content = "Any Time", Tag = "0" });
            foreach (Delivery_time delivery in deliveryTimes)
            {
                DeliveryComboBox.Items.Add(new ComboBoxItem { Content = delivery.Delivery_time_name, Tag = delivery.Delivery_time_id });
            }
            DeliveryComboBox.SelectedIndex = 0;

            LanguageComboBox.Items.Clear();
            LanguageComboBox.Items.Add(new ComboBoxItem { Content = "All Languages", Tag = "0" });
            foreach (Language language in languages)
            {
                LanguageComboBox.Items.Add(new ComboBoxItem { Content = language.Language_name, Tag = language.Language_id });
            }
            LanguageComboBox.SelectedIndex = 0;

            RatingComboBox.Items.Clear();
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "All Ratings", Tag = "0" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "5 stars & up", Tag = "5" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "4 stars & up", Tag = "4" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "3 stars & up", Tag = "3" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "2 stars & up", Tag = "2" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "1 star & up", Tag = "1" });
            RatingComboBox.SelectedIndex = 0;

            MinPriceTextBox.Text = "";
            MaxPriceTextBox.Text = "";
        }

        private void CategoryChanged(object sender, RoutedEventArgs e)
        {
            selectedCategoryIds.Clear();
            foreach (object child in CategoryWrapPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox.IsChecked == true && checkBox.Tag != null)
                {
                    selectedCategoryIds.Add(checkBox.Tag.ToString() ?? "");
                }
            }
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            currentPage = 1;
            RefreshCatalog();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            foreach (object child in CategoryWrapPanel.Children)
            {
                if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = false;
                }
            }

            selectedCategoryIds.Clear();
            SetupFilters();
            currentPage = 1;
            RefreshCatalog();
        }

        private void RefreshCatalog()
        {
            List<CatalogGigItem> filtered = ApplyFilters(allGigs);
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)filtered.Count / gigsPerPage));
            if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            List<CatalogGigItem> pageItems = filtered
                .Skip((currentPage - 1) * gigsPerPage)
                .Take(gigsPerPage)
                .ToList();

            ResultsTextBlock.Text = $"Available Gigs ({pageItems.Count} on this page)";
            RenderGigCards(pageItems);
            RenderPagination(totalPages);
        }

        private List<CatalogGigItem> ApplyFilters(List<CatalogGigItem> source)
        {
            double minPrice = ParseDouble(MinPriceTextBox.Text);
            double maxPrice = ParseDouble(MaxPriceTextBox.Text);
            string selectedDelivery = GetComboTag(DeliveryComboBox);
            string selectedLanguage = GetComboTag(LanguageComboBox);
            int minRating = (int)ParseDouble(GetComboTag(RatingComboBox));

            IEnumerable<CatalogGigItem> query = source;

            if (selectedCategoryIds.Count > 0)
            {
                query = query.Where(g => selectedCategoryIds.Contains(g.CategoryId));
            }

            if (minPrice > 0)
            {
                query = query.Where(g => g.Price >= minPrice);
            }

            if (maxPrice > 0)
            {
                query = query.Where(g => g.Price <= maxPrice);
            }

            if (selectedDelivery != "0")
            {
                query = query.Where(g => g.DeliveryTimeId == selectedDelivery);
            }

            if (selectedLanguage != "0")
            {
                query = query.Where(g => g.LanguageId == selectedLanguage);
            }

            if (minRating > 0)
            {
                query = query.Where(g => g.Rating >= minRating);
            }

            return query.ToList();
        }

        private void RenderGigCards(List<CatalogGigItem> gigs)
        {
            GigsWrapPanel.Children.Clear();

            if (gigs.Count == 0)
            {
                Border emptyState = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(20),
                    Width = 860,
                    Child = new TextBlock
                    {
                        Text = "No gigs match your filters.",
                        Foreground = Brushes.White,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                };

                GigsWrapPanel.Children.Add(emptyState);
                return;
            }

            foreach (CatalogGigItem gig in gigs)
            {
                Border card = new Border
                {
                    Width = 260,
                    Margin = new Thickness(0, 0, 16, 16),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(12)
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(new TextBlock
                {
                    Text = "🖼️ Preview",
                    Foreground = Brushes.White,
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                panel.Children.Add(new TextBlock
                {
                    Text = gig.Title,
                    Foreground = Brushes.White,
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap
                });
                panel.Children.Add(new TextBlock
                {
                    Text = gig.Description,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D3D3D3")),
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 8, 0, 8)
                });

                panel.Children.Add(new TextBlock
                {
                    Text = "Starting at",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BFBFBF")),
                    FontSize = 13,
                    FontWeight = FontWeights.Bold
                });
                panel.Children.Add(new TextBlock
                {
                    Text = "$" + gig.Price.ToString("0", CultureInfo.InvariantCulture),
                    Foreground = Brushes.White,
                    FontSize = 28,
                    FontWeight = FontWeights.Bold
                });

                card.Child = panel;
                GigsWrapPanel.Children.Add(card);
            }
        }

        private void RenderPagination(int totalPages)
        {
            PaginationPanel.Children.Clear();
            if (totalPages <= 1)
            {
                return;
            }

            PaginationPanel.Children.Add(CreatePageButton("Previous", currentPage > 1, () =>
            {
                currentPage--;
                RefreshCatalog();
            }));

            for (int i = 1; i <= totalPages; i++)
            {
                int pageNumber = i;
                Button pageButton = CreatePageButton(i.ToString(), true, () =>
                {
                    currentPage = pageNumber;
                    RefreshCatalog();
                });

                if (pageNumber == currentPage)
                {
                    pageButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                    pageButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                }

                PaginationPanel.Children.Add(pageButton);
            }

            PaginationPanel.Children.Add(CreatePageButton("Next", currentPage < totalPages, () =>
            {
                currentPage++;
                RefreshCatalog();
            }));
        }

        private Button CreatePageButton(string text, bool isEnabled, Action onClick)
        {
            Button button = new Button
            {
                Content = text,
                Margin = new Thickness(4, 0, 4, 0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A")),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560")),
                BorderThickness = new Thickness(1),
                IsEnabled = isEnabled,
                Opacity = isEnabled ? 1.0 : 0.5
            };

            button.Click += (_, _) => onClick();
            return button;
        }

        private static double ParseDouble(string? text)
        {
            if (double.TryParse(text, out double value))
            {
                return value;
            }

            return 0;
        }

        private static string GetComboTag(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                return item.Tag.ToString() ?? "0";
            }

            return "0";
        }
    }

    public class CatalogGigItem
    {
        public CatalogGigItem(string categoryId, string title, string description, double price, string deliveryTimeId, string languageId, int rating)
        {
            CategoryId = categoryId;
            Title = title;
            Description = description;
            Price = price;
            DeliveryTimeId = deliveryTimeId;
            LanguageId = languageId;
            Rating = rating;
        }

        public string CategoryId { get; }
        public string Title { get; }
        public string Description { get; }
        public double Price { get; }
        public string DeliveryTimeId { get; }
        public string LanguageId { get; }
        public int Rating { get; }
    }
}
using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GigNovaWPFApp.UserControls
{
    public partial class CatalogPage : UserControl
    {
        public event Action<string> GigSelected;

        CatalogViewModel catalogViewModel;
        List<string> selectedCategoryIds;

        public CatalogPage()
        {
            InitializeComponent();
            selectedCategoryIds = new List<string>();
            MinPriceTextBox.Text = "Min $";
            MaxPriceTextBox.Text = "Max $";
            MinPriceTextBox.GotFocus += PriceBox_GotFocus;
            MaxPriceTextBox.GotFocus += PriceBox_GotFocus;
            MinPriceTextBox.LostFocus += PriceBox_LostFocus;
            MaxPriceTextBox.LostFocus += PriceBox_LostFocus;
            LoadCatalog(1);
        }

        private void PriceBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Text == "Min $" || box.Text == "Max $")
            {
                box.Text = "";
            }
        }

        private void PriceBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Text.Trim() == "")
            {
                if (box == MinPriceTextBox) box.Text = "Min $";
                if (box == MaxPriceTextBox) box.Text = "Max $";
            }
        }

        private void CollectSelectedCategoriesFromUI()
        {
            selectedCategoryIds.Clear();
            foreach (object child in CategoryWrapPanel.Children)
            {
                CheckBox checkBox = child as CheckBox;
                if (checkBox != null && checkBox.IsChecked == true && checkBox.Tag != null)
                {
                    selectedCategoryIds.Add(checkBox.Tag.ToString());
                }
            }
        }

        private async void LoadCatalog(int page)
        {
            try
            {
                ApiClient<CatalogViewModel> client = new ApiClient<CatalogViewModel>();
                client.Scheme = "https";
                client.Host = "localhost";
                client.Port = 7059;
                client.Path = "api/Guest/GetCatalogViewModel";

                string categories = "";
                for (int i = 0; i < selectedCategoryIds.Count; i++)
                {
                    if (i > 0) categories += ",";
                    categories += selectedCategoryIds[i];
                }

                if (categories != "") client.AddParameter("categories", categories);

                client.AddParameter("page", page.ToString());

                double minPrice = ParseDouble(MinPriceTextBox.Text);
                double maxPrice = ParseDouble(MaxPriceTextBox.Text);
                int deliveryTimeId = ParseInt(GetComboTag(DeliveryComboBox));
                int languageId = ParseInt(GetComboTag(LanguageComboBox));
                int minRating = ParseInt(GetComboTag(RatingComboBox));

                if (minPrice > 0) client.AddParameter("min_price", minPrice.ToString(CultureInfo.InvariantCulture));
                if (maxPrice > 0) client.AddParameter("max_price", maxPrice.ToString(CultureInfo.InvariantCulture));
                if (deliveryTimeId > 0) client.AddParameter("delivery_time_id", deliveryTimeId.ToString());
                if (languageId > 0) client.AddParameter("language_id", languageId.ToString());
                if (minRating > 0) client.AddParameter("min_rating", minRating.ToString());

                catalogViewModel = await client.GetAsync();
                if (catalogViewModel == null)
                {
                    MessageBox.Show("Could not load catalog data from server.", "GigNova");
                    return;
                }

                SyncSelectedCategories();
                FillCategoryFilters();
                FillDropDownFilters();
                ShowGigs();
                ShowPagination();
            }
            catch
            {
                MessageBox.Show("Error loading catalog data from server.", "GigNova");
            }
        }

        private void SyncSelectedCategories()
        {
            selectedCategoryIds.Clear();
            if (catalogViewModel.GigCategories == null || catalogViewModel.GigCategories.Trim() == "") return;
            string[] split = catalogViewModel.GigCategories.Split(',');
            for (int i = 0; i < split.Length; i++)
            {
                string id = split[i].Trim();
                if (id != "") selectedCategoryIds.Add(id);
            }
        }

        private void FillCategoryFilters()
        {
            CategoryWrapPanel.Children.Clear();
            if (catalogViewModel.Categories == null) return;

            foreach (Category category in catalogViewModel.Categories)
            {
                bool isSelected = false;
                foreach (string id in selectedCategoryIds)
                {
                    if (id == category.Category_id) isSelected = true;
                }

                CheckBox checkBox = new CheckBox();
                checkBox.Content = category.Category_name;
                checkBox.Tag = category.Category_id;
                checkBox.Margin = new Thickness(0, 0, 10, 10);
                checkBox.Padding = new Thickness(10, 6, 10, 6);
                checkBox.Foreground = Brushes.White;
                checkBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A"));
                checkBox.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                checkBox.BorderThickness = new Thickness(1);
                checkBox.IsChecked = isSelected;
                CategoryWrapPanel.Children.Add(checkBox);
            }
        }

        private void FillDropDownFilters()
        {
            if (catalogViewModel == null) return;

            if (catalogViewModel.min_price > 0) MinPriceTextBox.Text = catalogViewModel.min_price.ToString(CultureInfo.InvariantCulture);
            if (catalogViewModel.max_price > 0) MaxPriceTextBox.Text = catalogViewModel.max_price.ToString(CultureInfo.InvariantCulture);

            DeliveryComboBox.Items.Clear();
            DeliveryComboBox.Items.Add(new ComboBoxItem { Content = "Any Time", Tag = "0" });
            if (catalogViewModel.Delivery_Times != null)
            {
                foreach (Delivery_time delivery in catalogViewModel.Delivery_Times)
                {
                    DeliveryComboBox.Items.Add(new ComboBoxItem { Content = delivery.Delivery_time_name, Tag = delivery.Delivery_time_id });
                }
            }
            SelectComboByTag(DeliveryComboBox, catalogViewModel.delivery_time_id.ToString());

            LanguageComboBox.Items.Clear();
            LanguageComboBox.Items.Add(new ComboBoxItem { Content = "All Languages", Tag = "0" });
            if (catalogViewModel.Languages != null)
            {
                foreach (Language language in catalogViewModel.Languages)
                {
                    LanguageComboBox.Items.Add(new ComboBoxItem { Content = language.Language_name, Tag = language.Language_id });
                }
            }
            SelectComboByTag(LanguageComboBox, catalogViewModel.language_id.ToString());

            RatingComboBox.Items.Clear();
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "All Ratings", Tag = "0" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "5 stars & up", Tag = "5" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "4 stars & up", Tag = "4" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "3 stars & up", Tag = "3" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "2 stars & up", Tag = "2" });
            RatingComboBox.Items.Add(new ComboBoxItem { Content = "1 star & up", Tag = "1" });
            SelectComboByTag(RatingComboBox, ((int)catalogViewModel.min_rating).ToString());
        }

        private void SelectComboByTag(ComboBox comboBox, string tagValue)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                ComboBoxItem item = comboBox.Items[i] as ComboBoxItem;
                if (item != null)
                {
                    string tag = item.Tag == null ? "0" : item.Tag.ToString();
                    if (tag == tagValue) { comboBox.SelectedIndex = i; return; }
                }
            }
            comboBox.SelectedIndex = 0;
        }

        private void ShowGigs()
        {
            GigsWrapPanel.Children.Clear();
            int count = catalogViewModel.Gigs == null ? 0 : catalogViewModel.Gigs.Count;
            ResultsTextBlock.Text = "Available Gigs (" + count + " on this page)";

            if (catalogViewModel.Gigs == null || catalogViewModel.Gigs.Count == 0)
            {
                Border emptyState = new Border();
                emptyState.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
                emptyState.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                emptyState.BorderThickness = new Thickness(1);
                emptyState.CornerRadius = new CornerRadius(10);
                emptyState.Padding = new Thickness(20);
                emptyState.Width = 860;
                TextBlock txt = new TextBlock();
                txt.Text = "No gigs match your filters.";
                txt.Foreground = Brushes.White;
                txt.FontSize = 24;
                txt.FontWeight = FontWeights.Bold;
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                emptyState.Child = txt;
                GigsWrapPanel.Children.Add(emptyState);
                return;
            }

            for (int i = 0; i < catalogViewModel.Gigs.Count; i++)
            {
                Gig gig = catalogViewModel.Gigs[i];
                string categoryNamesText = "";
                if (catalogViewModel.GigCategoryNames != null && catalogViewModel.GigCategoryNames.Count > i)
                {
                    categoryNamesText = catalogViewModel.GigCategoryNames[i];
                }

                Button cardButton = new Button();
                cardButton.Width = 260;
                cardButton.Margin = new Thickness(0, 0, 16, 16);
                cardButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));
                cardButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                cardButton.BorderThickness = new Thickness(1);
                cardButton.Padding = new Thickness(0);
                cardButton.Cursor = System.Windows.Input.Cursors.Hand;
                cardButton.ToolTip = "Open gig";
                string gigId = gig.Gig_id;
                cardButton.Click += (s, e) => { if (GigSelected != null) GigSelected(gigId); };

                Border inner = new Border();
                inner.CornerRadius = new CornerRadius(10);
                inner.Padding = new Thickness(12);
                inner.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111111"));

                StackPanel panel = new StackPanel();
                TextBlock title = new TextBlock();
                title.Text = gig.Gig_name;
                title.Foreground = Brushes.White;
                title.FontSize = 22;
                title.FontWeight = FontWeights.Bold;
                title.TextWrapping = TextWrapping.Wrap;
                panel.Children.Add(title);

                TextBlock description = new TextBlock();
                description.Text = gig.Gig_description;
                description.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D3D3D3"));
                description.FontSize = 15;
                description.TextWrapping = TextWrapping.Wrap;
                description.Margin = new Thickness(0, 8, 0, 8);
                panel.Children.Add(description);

                if (categoryNamesText != null && categoryNamesText.Trim() != "")
                {
                    WrapPanel chipWrap = new WrapPanel();
                    chipWrap.Margin = new Thickness(0, 0, 0, 8);
                    string[] categoryParts = categoryNamesText.Split(',');
                    foreach (string part in categoryParts)
                    {
                        string trimmed = part.Trim();
                        if (trimmed != "")
                        {
                            Border chip = new Border();
                            chip.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A"));
                            chip.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                            chip.BorderThickness = new Thickness(1);
                            chip.CornerRadius = new CornerRadius(8);
                            chip.Padding = new Thickness(6, 3, 6, 3);
                            chip.Margin = new Thickness(0, 0, 6, 6);
                            TextBlock chipText = new TextBlock();
                            chipText.Text = trimmed;
                            chipText.Foreground = Brushes.White;
                            chipText.FontSize = 12;
                            chipText.FontWeight = FontWeights.Bold;
                            chip.Child = chipText;
                            chipWrap.Children.Add(chip);
                        }
                    }
                    panel.Children.Add(chipWrap);
                }

                TextBlock priceLabel = new TextBlock();
                priceLabel.Text = "Starting at";
                priceLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BFBFBF"));
                priceLabel.FontSize = 13;
                priceLabel.FontWeight = FontWeights.Bold;
                panel.Children.Add(priceLabel);

                TextBlock priceValue = new TextBlock();
                priceValue.Text = "$" + gig.Gig_price.ToString("0", CultureInfo.InvariantCulture);
                priceValue.Foreground = Brushes.White;
                priceValue.FontSize = 28;
                priceValue.FontWeight = FontWeights.Bold;
                panel.Children.Add(priceValue);

                inner.Child = panel;
                cardButton.Content = inner;
                GigsWrapPanel.Children.Add(cardButton);
            }
        }

        private void ShowPagination()
        {
            PaginationPanel.Children.Clear();
            if (catalogViewModel == null || catalogViewModel.TotalPages <= 1) return;

            Button prev = CreatePageButton("Previous", catalogViewModel.Page > 1);
            prev.Click += (s, e) => LoadCatalog(catalogViewModel.Page - 1);
            PaginationPanel.Children.Add(prev);

            for (int i = 1; i <= catalogViewModel.TotalPages; i++)
            {
                int pageNumber = i;
                Button pageBtn = CreatePageButton(pageNumber.ToString(), true);
                if (catalogViewModel.Page == pageNumber)
                {
                    pageBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                    pageBtn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                }
                pageBtn.Click += (s, e) => LoadCatalog(pageNumber);
                PaginationPanel.Children.Add(pageBtn);
            }

            Button next = CreatePageButton("Next", catalogViewModel.Page < catalogViewModel.TotalPages);
            next.Click += (s, e) => LoadCatalog(catalogViewModel.Page + 1);
            PaginationPanel.Children.Add(next);
        }

        private Button CreatePageButton(string text, bool isEnabled)
        {
            Button button = new Button();
            button.Content = text;
            button.Margin = new Thickness(4, 0, 4, 0);
            button.Padding = new Thickness(12, 6, 12, 6);
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A3A"));
            button.Foreground = Brushes.White;
            button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
            button.BorderThickness = new Thickness(1);
            button.IsEnabled = isEnabled;
            button.Opacity = isEnabled ? 1.0 : 0.5;
            button.Cursor = System.Windows.Input.Cursors.Hand;
            return button;
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            CollectSelectedCategoriesFromUI();
            LoadCatalog(1);
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            selectedCategoryIds.Clear();
            MinPriceTextBox.Text = "Min $";
            MaxPriceTextBox.Text = "Max $";
            DeliveryComboBox.SelectedIndex = 0;
            LanguageComboBox.SelectedIndex = 0;
            RatingComboBox.SelectedIndex = 0;
            LoadCatalog(1);
        }

        private double ParseDouble(string text)
        {
            if (text == "Min $" || text == "Max $") return 0;
            double value;
            bool ok = double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            return ok ? value : 0;
        }

        private int ParseInt(string text)
        {
            int value;
            bool ok = int.TryParse(text, out value);
            return ok ? value : 0;
        }

        private string GetComboTag(ComboBox comboBox)
        {
            ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
            if (item != null && item.Tag != null) return item.Tag.ToString();
            return "0";
        }
    }
}
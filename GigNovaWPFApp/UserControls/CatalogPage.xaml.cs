using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GigNovaWPFApp.UserControls
{
    public partial class CatalogPage : UserControl
    {
        public event Action<string> GigSelected;

        CatalogViewModel catalogViewModel;
        List<string> selectedCategoryIds;
        bool isEditMode;


        // ============================== Initialization ==============================

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


        // ============================== Edit Mode (admin) ==============================

        // Toggle between normal viewing mode and admin edit mode.
        // In edit mode, "Add Category" and "View Blocked" buttons appear, and gig/category styles change.
        private void EditModeButton_Click(object sender, RoutedEventArgs e)
        {
            isEditMode = !isEditMode;
            if (isEditMode)
            {
                EditModeButton.Content = "Exit Edit Mode";
                EditModeButton.Background = WpfHelpers.Brush("#E94560");
                AddCategoryButton.Visibility = Visibility.Visible;
                ViewBlockedButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditModeButton.Content = "Edit Mode";
                EditModeButton.Background = WpfHelpers.Brush("#3A3A3A");
                AddCategoryButton.Visibility = Visibility.Collapsed;
                ViewBlockedButton.Visibility = Visibility.Collapsed;
            }
            UpdateGigStyles();
            UpdateCategoryStyles();
        }

        // Re-skin all gig cards based on the current edit mode.
        private void UpdateGigStyles()
        {
            Style s;
            if (isEditMode)
            {
                s = (Style)FindResource("GigCardEditStyle");
            }
            else
            {
                s = (Style)FindResource("GigCardStyle");
            }
            foreach (object child in GigsWrapPanel.Children)
            {
                Button btn = child as Button;
                if (btn != null)
                {
                    btn.Style = s;
                }
            }
        }

        // Re-skin all category chips based on the current edit mode.
        private void UpdateCategoryStyles()
        {
            Style s;
            if (isEditMode)
            {
                s = (Style)FindResource("CategoryChipEditStyle");
            }
            else
            {
                s = (Style)FindResource("CategoryChipStyle");
            }
            foreach (object child in CategoryWrapPanel.Children)
            {
                CheckBox cb = child as CheckBox;
                if (cb != null)
                {
                    cb.Style = s;
                }
            }
        }

        // Open the "Blocked items" dialog. If something was unblocked, reload the catalog.
        private void ViewBlockedButton_Click(object sender, RoutedEventArgs e)
        {
            BlockedListDialog dialog = new BlockedListDialog();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
            if (dialog.DidChange)
            {
                LoadCatalog(1);
            }
        }

        // Open the "Create Category" dialog. If one was created, reload the catalog so it appears.
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryDialog dialog = new CategoryDialog();
            dialog.Owner = Window.GetWindow(this);
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                LoadCatalog(1);
            }
        }


        // ============================== Filter Inputs ==============================

        // Clear the placeholder text ("Min $" / "Max $") when the user clicks into the box.
        private void PriceBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Text == "Min $" || box.Text == "Max $")
            {
                box.Text = "";
            }
        }

        // Restore the placeholder if the user leaves the box empty.
        private void PriceBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Text.Trim() == "")
            {
                if (box == MinPriceTextBox)
                {
                    box.Text = "Min $";
                }
                if (box == MaxPriceTextBox)
                {
                    box.Text = "Max $";
                }
            }
        }

        // Reads which category chips are checked right now into selectedCategoryIds.
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

        // Apply button: pick up the chip selections and reload from page 1.
        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            CollectSelectedCategoriesFromUI();
            LoadCatalog(1);
        }

        // Clear button: reset all filters back to defaults and reload.
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


        // ============================== Loading Data ==============================

        // Calls the WS for the catalog view model, then refreshes the chips / dropdowns / gig grid / pagination.
        private async void LoadCatalog(int page)
        {
            try
            {
                ApiClient<CatalogViewModel> client = WpfHelpers.BuildClient<CatalogViewModel>("api/Guest/GetCatalogViewModel");

                // Build the categories CSV from the currently selected ids.
                string categories = "";
                for (int i = 0; i < selectedCategoryIds.Count; i++)
                {
                    if (i > 0)
                    {
                        categories += ",";
                    }
                    categories += selectedCategoryIds[i];
                }
                if (categories != "")
                {
                    client.AddParameter("categories", categories);
                }

                client.AddParameter("page", page.ToString());

                // Read the other filter values from the UI.
                double minPrice = ParseDouble(MinPriceTextBox.Text);
                double maxPrice = ParseDouble(MaxPriceTextBox.Text);
                int deliveryTimeId = ParseInt(GetComboTag(DeliveryComboBox));
                int languageId = ParseInt(GetComboTag(LanguageComboBox));
                int minRating = ParseInt(GetComboTag(RatingComboBox));

                if (minPrice > 0)
                {
                    client.AddParameter("min_price", minPrice.ToString(CultureInfo.InvariantCulture));
                }
                if (maxPrice > 0)
                {
                    client.AddParameter("max_price", maxPrice.ToString(CultureInfo.InvariantCulture));
                }
                if (deliveryTimeId > 0)
                {
                    client.AddParameter("delivery_time_id", deliveryTimeId.ToString());
                }
                if (languageId > 0)
                {
                    client.AddParameter("language_id", languageId.ToString());
                }
                if (minRating > 0)
                {
                    client.AddParameter("min_rating", minRating.ToString());
                }

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

        // After a reload, refresh selectedCategoryIds from what the server says is selected (string from URL).
        private void SyncSelectedCategories()
        {
            selectedCategoryIds.Clear();
            if (catalogViewModel.GigCategories == null || catalogViewModel.GigCategories.Trim() == "")
            {
                return;
            }
            string[] split = catalogViewModel.GigCategories.Split(',');
            for (int i = 0; i < split.Length; i++)
            {
                string id = split[i].Trim();
                if (id != "")
                {
                    selectedCategoryIds.Add(id);
                }
            }
        }


        // ============================== Rendering ==============================

        // Build the category chip row using checkboxes. Chips are checked if their id is in selectedCategoryIds.
        private void FillCategoryFilters()
        {
            CategoryWrapPanel.Children.Clear();
            if (catalogViewModel.Categories == null)
            {
                return;
            }

            Style chipStyle;
            if (isEditMode)
            {
                chipStyle = (Style)FindResource("CategoryChipEditStyle");
            }
            else
            {
                chipStyle = (Style)FindResource("CategoryChipStyle");
            }

            foreach (Category category in catalogViewModel.Categories)
            {
                bool isSelected = false;
                foreach (string id in selectedCategoryIds)
                {
                    if (id == category.Category_id)
                    {
                        isSelected = true;
                        break;
                    }
                }

                CheckBox checkBox = new CheckBox();
                checkBox.Style = chipStyle;
                checkBox.Content = category.Category_name;
                checkBox.Tag = category.Category_id;
                checkBox.IsChecked = isSelected;
                checkBox.PreviewMouseLeftButtonDown += Category_PreviewMouseLeftButtonDown;
                CategoryWrapPanel.Children.Add(checkBox);
            }
        }

        // Fill the three dropdowns (Delivery / Language / Rating) and pre-select whatever the server says is active.
        private void FillDropDownFilters()
        {
            if (catalogViewModel == null)
            {
                return;
            }

            if (catalogViewModel.min_price > 0)
            {
                MinPriceTextBox.Text = catalogViewModel.min_price.ToString(CultureInfo.InvariantCulture);
            }
            if (catalogViewModel.max_price > 0)
            {
                MaxPriceTextBox.Text = catalogViewModel.max_price.ToString(CultureInfo.InvariantCulture);
            }

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

        // Helper: find the item in a ComboBox whose Tag matches tagValue and select it. Falls back to index 0.
        private void SelectComboByTag(ComboBox comboBox, string tagValue)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                ComboBoxItem item = comboBox.Items[i] as ComboBoxItem;
                if (item != null)
                {
                    string tag = "0";
                    if (item.Tag != null)
                    {
                        tag = item.Tag.ToString();
                    }
                    if (tag == tagValue)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }
                }
            }
            comboBox.SelectedIndex = 0;
        }

        // Build the gig cards grid. Each card is a clickable Button styled as a card.
        private void ShowGigs()
        {
            GigsWrapPanel.Children.Clear();
            int count = 0;
            if (catalogViewModel.Gigs != null)
            {
                count = catalogViewModel.Gigs.Count;
            }
            ResultsTextBlock.Text = "Available Gigs (" + count + " on this page)";

            // Empty state - one big "no results" card.
            if (catalogViewModel.Gigs == null || catalogViewModel.Gigs.Count == 0)
            {
                Border emptyState = new Border();
                emptyState.Background = WpfHelpers.Brush("#111111");
                emptyState.BorderBrush = WpfHelpers.Brush("#E94560");
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

            // Style depends on edit mode (normal cards vs edit-mode cards).
            Style cardStyle;
            if (isEditMode)
            {
                cardStyle = (Style)FindResource("GigCardEditStyle");
            }
            else
            {
                cardStyle = (Style)FindResource("GigCardStyle");
            }

            // Build one card per gig.
            for (int i = 0; i < catalogViewModel.Gigs.Count; i++)
            {
                Gig gig = catalogViewModel.Gigs[i];

                string categoryNamesText = "";
                if (catalogViewModel.GigCategoryNames != null && catalogViewModel.GigCategoryNames.Count > i)
                {
                    categoryNamesText = catalogViewModel.GigCategoryNames[i];
                }

                Button cardButton = new Button();
                cardButton.Style = cardStyle;
                cardButton.Width = 260;
                cardButton.Margin = new Thickness(0, 0, 16, 16);
                cardButton.ToolTip = "Open gig";
                cardButton.Tag = gig.Gig_id;
                cardButton.Click += GigCard_Click;

                Border inner = new Border();
                inner.CornerRadius = new CornerRadius(10);
                inner.Padding = new Thickness(12);

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
                description.Foreground = WpfHelpers.Brush("#D3D3D3");
                description.FontSize = 15;
                description.TextWrapping = TextWrapping.Wrap;
                description.Margin = new Thickness(0, 8, 0, 8);
                panel.Children.Add(description);

                // Category chips strip (only if this gig actually has categories).
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
                            chip.Background = WpfHelpers.Brush("#2A2A2A");
                            chip.BorderBrush = WpfHelpers.Brush("#E94560");
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
                priceLabel.Foreground = WpfHelpers.Brush("#BFBFBF");
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

        // Clicking a gig card: in normal mode, open the gig; in edit mode, ask if the admin wants to block it.
        private async void GigCard_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string gigId = btn.Tag.ToString();

            if (isEditMode)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to block this gig?",
                    "Confirm Block",
                    MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                ApiClient<bool> client = WpfHelpers.BuildClient<bool>("api/Admin/BlockGig");
                client.AddParameter("gig_id", gigId);

                bool ok = await client.PostAsync(false);
                if (ok == false)
                {
                    MessageBox.Show("Failed to block gig.", "GigNova");
                    return;
                }
                LoadCatalog(catalogViewModel.Page);
            }
            else
            {
                if (GigSelected != null)
                {
                    GigSelected(gigId);
                }
            }
        }


        // ============================== Pagination ==============================

        // Build Previous / page-number / Next buttons. Click handlers go through PageButton_Click
        // and read the target page from the button's Tag (avoids lambda event handlers).
        private void ShowPagination()
        {
            PaginationPanel.Children.Clear();
            if (catalogViewModel == null || catalogViewModel.TotalPages <= 1)
            {
                return;
            }

            Button prev = CreatePageButton("Previous", catalogViewModel.Page > 1);
            prev.Tag = catalogViewModel.Page - 1;
            prev.Click += PageButton_Click;
            PaginationPanel.Children.Add(prev);

            for (int i = 1; i <= catalogViewModel.TotalPages; i++)
            {
                Button pageBtn = CreatePageButton(i.ToString(), true);
                pageBtn.Tag = i;
                if (catalogViewModel.Page == i)
                {
                    pageBtn.Background = WpfHelpers.Brush("#E94560");
                    pageBtn.BorderBrush = WpfHelpers.Brush("#E94560");
                }
                pageBtn.Click += PageButton_Click;
                PaginationPanel.Children.Add(pageBtn);
            }

            Button next = CreatePageButton("Next", catalogViewModel.Page < catalogViewModel.TotalPages);
            next.Tag = catalogViewModel.Page + 1;
            next.Click += PageButton_Click;
            PaginationPanel.Children.Add(next);
        }

        // Shared click handler for every page button. The target page is stored in the button's Tag.
        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int page = (int)btn.Tag;
            LoadCatalog(page);
        }

        // Helper that creates a styled pagination button. isEnabled=false greys it out.
        private Button CreatePageButton(string text, bool isEnabled)
        {
            Button button = new Button();
            button.Style = (Style)FindResource("PageButtonStyle");
            button.Content = text;
            button.Margin = new Thickness(4, 0, 4, 0);
            button.IsEnabled = isEnabled;
            if (isEnabled)
            {
                button.Opacity = 1.0;
            }
            else
            {
                button.Opacity = 0.5;
            }
            return button;
        }


        // ============================== Admin: long-press a category to edit or block it ==============================

        // In edit mode, clicking a category chip opens a Yes/No/Cancel popup:
        //   Yes -> rename the category   No -> block the category   Cancel -> do nothing
        private async void Category_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isEditMode == false)
            {
                return;
            }
            e.Handled = true;

            CheckBox cb = sender as CheckBox;
            string id = cb.Tag.ToString();
            string name = cb.Content.ToString();

            MessageBoxResult choice = MessageBox.Show(
                "Category '" + name + "'\n\nYes  =  Edit name\nNo  =  Block category\nCancel  =  Cancel",
                "Edit or Block",
                MessageBoxButton.YesNoCancel);

            if (choice == MessageBoxResult.Yes)
            {
                CategoryDialog dialog = new CategoryDialog(id, name);
                dialog.Owner = Window.GetWindow(this);
                bool? result = dialog.ShowDialog();
                if (result == true)
                {
                    LoadCatalog(1);
                }
                return;
            }

            if (choice == MessageBoxResult.No)
            {
                MessageBoxResult confirm = MessageBox.Show(
                    "Are you sure you want to block the category '" + name + "'?",
                    "Confirm Block",
                    MessageBoxButton.YesNo);
                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }

                ApiClient<bool> client = WpfHelpers.BuildClient<bool>("api/Admin/BlockCategory");
                client.AddParameter("category_id", id);

                bool ok = await client.PostAsync(false);
                if (ok == false)
                {
                    MessageBox.Show("Failed to block category.", "GigNova");
                    return;
                }
                LoadCatalog(1);
            }
        }


        // ============================== Parsing Helpers ==============================

        // Parses a price text box. Returns 0 if the text is the "Min $" / "Max $" placeholder or otherwise invalid.
        private double ParseDouble(string text)
        {
            if (text == "Min $" || text == "Max $")
            {
                return 0;
            }
            double value;
            bool ok = double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            if (ok)
            {
                return value;
            }
            return 0;
        }

        // Parses any int-like string. Returns 0 if the parse fails.
        private int ParseInt(string text)
        {
            int value;
            bool ok = int.TryParse(text, out value);
            if (ok)
            {
                return value;
            }
            return 0;
        }

        // Reads the Tag of the currently selected item in a ComboBox. Returns "0" if nothing is selected.
        private string GetComboTag(ComboBox comboBox)
        {
            ComboBoxItem item = comboBox.SelectedItem as ComboBoxItem;
            if (item != null && item.Tag != null)
            {
                return item.Tag.ToString();
            }
            return "0";
        }
    }
}


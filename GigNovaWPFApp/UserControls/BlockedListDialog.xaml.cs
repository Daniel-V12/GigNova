using GigNovaModels.Models;
using GigNovaModels.ViewModels;
using GigNovaWSClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GigNovaWPFApp.UserControls
{
    public partial class BlockedListDialog : Window
    {
        BlockedListViewModel viewModel;
        string currentType;
        bool didChange;
        bool isInitialized;


        // ============================== Initialization ==============================

        public BlockedListDialog()
        {
            InitializeComponent();
            currentType = "gig";
            didChange = false;
            isInitialized = true;
            LoadList(1);
        }

        // Read-only flag used by the caller (CatalogPage) to know if it should refresh after this dialog closes.
        public bool DidChange
        {
            get { return didChange; }
        }


        // ============================== Top Controls (type switcher + search + close) ==============================

        // Switch between "Gigs" and "Categories" view.
        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignore the SelectionChanged fired by the initial XAML setup (before ctor finished).
            if (isInitialized == false)
            {
                return;
            }
            ComboBoxItem item = TypeComboBox.SelectedItem as ComboBoxItem;
            if (item == null)
            {
                return;
            }
            string text = item.Content.ToString();
            if (text == "Gigs")
            {
                currentType = "gig";
            }
            else
            {
                currentType = "category";
            }
            LoadList(1);
        }

        // Search button: just reload from page 1 with whatever is in the search box.
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            LoadList(1);
        }

        // Close the dialog.
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        // ============================== Loading + Rendering ==============================

        // Fetches the blocked list for the current type (gig/category), filtered by search, paged.
        private async void LoadList(int page)
        {
            try
            {
                ApiClient<BlockedListViewModel> client = WpfHelpers.BuildClient<BlockedListViewModel>("api/Admin/GetBlockedListViewModel");
                client.AddParameter("type", currentType);
                client.AddParameter("page", page.ToString());
                client.AddParameter("search", SearchTextBox.Text.Trim());

                viewModel = await client.GetAsync();
                if (viewModel == null)
                {
                    MessageBox.Show("Could not load blocked list from server.", "GigNova");
                    return;
                }
                ShowItems();
                ShowPagination();
            }
            catch
            {
                MessageBox.Show("Error loading blocked list.", "GigNova");
            }
        }

        // Render one row per blocked item (gig OR category, depending on currentType).
        private void ShowItems()
        {
            ItemsPanel.Children.Clear();

            if (currentType == "gig")
            {
                if (viewModel.BlockedGigs == null || viewModel.BlockedGigs.Count == 0)
                {
                    AddEmptyMessage("No blocked gigs found.");
                    return;
                }
                foreach (Gig gig in viewModel.BlockedGigs)
                {
                    AddRow(gig.Gig_id, gig.Gig_name);
                }
            }
            else
            {
                if (viewModel.BlockedCategories == null || viewModel.BlockedCategories.Count == 0)
                {
                    AddEmptyMessage("No blocked categories found.");
                    return;
                }
                foreach (Category category in viewModel.BlockedCategories)
                {
                    AddRow(category.Category_id, category.Category_name);
                }
            }
        }

        // Add a centered "no results" message to the items panel.
        private void AddEmptyMessage(string message)
        {
            TextBlock txt = new TextBlock();
            txt.Text = message;
            txt.Foreground = Brushes.White;
            txt.FontSize = 16;
            txt.HorizontalAlignment = HorizontalAlignment.Center;
            txt.Margin = new Thickness(0, 30, 0, 0);
            ItemsPanel.Children.Add(txt);
        }

        // Add one row: name on the left, "Unblock" button on the right. Stores the id in the button's Tag.
        private void AddRow(string id, string name)
        {
            Border row = new Border();
            row.Background = WpfHelpers.Brush("#171717");
            row.BorderBrush = WpfHelpers.Brush("#3A3A3A");
            row.BorderThickness = new Thickness(1);
            row.CornerRadius = new CornerRadius(8);
            row.Padding = new Thickness(12, 8, 12, 8);
            row.Margin = new Thickness(0, 0, 0, 8);

            Grid grid = new Grid();
            ColumnDefinition nameCol = new ColumnDefinition();
            nameCol.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition buttonCol = new ColumnDefinition();
            buttonCol.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(nameCol);
            grid.ColumnDefinitions.Add(buttonCol);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = name;
            textBlock.Foreground = Brushes.White;
            textBlock.FontSize = 15;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextWrapping = TextWrapping.Wrap;
            Grid.SetColumn(textBlock, 0);
            grid.Children.Add(textBlock);

            Button button = new Button();
            button.Content = "Unblock";
            button.Style = (Style)FindResource("DialogButtonStyle");
            button.Width = 90;
            button.Height = 30;
            button.Background = WpfHelpers.Brush("#19A64B");
            button.Tag = id;
            button.Click += UnblockButton_Click;
            Grid.SetColumn(button, 1);
            grid.Children.Add(button);

            row.Child = grid;
            ItemsPanel.Children.Add(row);
        }

        // "Unblock" click handler. Asks the WS to unblock either a gig or a category (based on currentType).
        private async void UnblockButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string id = btn.Tag.ToString();

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to unblock this item?",
                "Confirm Unblock",
                MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            ApiClient<bool> client;
            if (currentType == "gig")
            {
                client = WpfHelpers.BuildClient<bool>("api/Admin/UnblockGig");
                client.AddParameter("gig_id", id);
            }
            else
            {
                client = WpfHelpers.BuildClient<bool>("api/Admin/UnblockCategory");
                client.AddParameter("category_id", id);
            }

            bool ok = await client.PostAsync(false);
            if (ok == false)
            {
                MessageBox.Show("Failed to unblock item.", "GigNova");
                return;
            }
            didChange = true;
            LoadList(viewModel.Page);
        }


        // ============================== Pagination ==============================

        // Build Previous / page-number / Next buttons. Each button's target page is stored in its Tag,
        // and the shared PageButton_Click handler reads it (no lambda event handlers).
        private void ShowPagination()
        {
            PaginationPanel.Children.Clear();
            if (viewModel == null || viewModel.TotalPages <= 1)
            {
                return;
            }

            Button prev = CreatePageButton("Previous", viewModel.Page > 1);
            prev.Tag = viewModel.Page - 1;
            prev.Click += PageButton_Click;
            PaginationPanel.Children.Add(prev);

            for (int i = 1; i <= viewModel.TotalPages; i++)
            {
                Button pageBtn = CreatePageButton(i.ToString(), true);
                pageBtn.Tag = i;
                if (viewModel.Page == i)
                {
                    pageBtn.Background = WpfHelpers.Brush("#E94560");
                }
                pageBtn.Click += PageButton_Click;
                PaginationPanel.Children.Add(pageBtn);
            }

            Button next = CreatePageButton("Next", viewModel.Page < viewModel.TotalPages);
            next.Tag = viewModel.Page + 1;
            next.Click += PageButton_Click;
            PaginationPanel.Children.Add(next);
        }

        // Shared click handler for every page button. The target page is in the button's Tag.
        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int page = (int)btn.Tag;
            LoadList(page);
        }

        // Helper that creates a styled pagination button. isEnabled=false greys it out.
        private Button CreatePageButton(string text, bool isEnabled)
        {
            Button button = new Button();
            button.Content = text;
            button.Style = (Style)FindResource("DialogButtonStyle");
            button.Background = WpfHelpers.Brush("#3A3A3A");
            button.Foreground = Brushes.White;
            button.Width = 70;
            button.Height = 30;
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
    }
}

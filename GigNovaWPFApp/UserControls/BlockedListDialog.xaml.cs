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

        public BlockedListDialog()
        {
            InitializeComponent();
            currentType = "gig";
            didChange = false;
            isInitialized = true;
            LoadList(1);
        }

        public bool DidChange
        {
            get { return didChange; }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            ComboBoxItem item = TypeComboBox.SelectedItem as ComboBoxItem;
            if (item == null) return;
            string text = item.Content.ToString();
            if (text == "Gigs") currentType = "gig";
            else currentType = "category";
            LoadList(1);
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            LoadList(1);
        }

        private async void LoadList(int page)
        {
            try
            {
                ApiClient<BlockedListViewModel> client = new ApiClient<BlockedListViewModel>();
                client.Scheme = "https";
                client.Host = "localhost";
                client.Port = 7059;
                client.Path = "api/Admin/GetBlockedListViewModel";
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

        private void AddRow(string id, string name)
        {
            Border row = new Border();
            row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#171717"));
            row.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A3A"));
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
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19A64B"));
            button.Tag = id;
            button.Click += UnblockButton_Click;
            Grid.SetColumn(button, 1);
            grid.Children.Add(button);

            row.Child = grid;
            ItemsPanel.Children.Add(row);
        }

        private async void UnblockButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string id = btn.Tag.ToString();

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to unblock this item?",
                "Confirm Unblock",
                MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            ApiClient<bool> client = new ApiClient<bool>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;
            if (currentType == "gig")
            {
                client.Path = "api/Admin/UnblockGig";
                client.AddParameter("gig_id", id);
            }
            else
            {
                client.Path = "api/Admin/UnblockCategory";
                client.AddParameter("category_id", id);
            }

            bool ok = await client.PostAsync(false);
            if (!ok)
            {
                MessageBox.Show("Failed to unblock item.", "GigNova");
                return;
            }
            didChange = true;
            LoadList(viewModel.Page);
        }

        private void ShowPagination()
        {
            PaginationPanel.Children.Clear();
            if (viewModel == null || viewModel.TotalPages <= 1) return;

            Button prev = CreatePageButton("Previous", viewModel.Page > 1);
            int prevPage = viewModel.Page - 1;
            prev.Click += (s, e) => LoadList(prevPage);
            PaginationPanel.Children.Add(prev);

            for (int i = 1; i <= viewModel.TotalPages; i++)
            {
                int pageNumber = i;
                Button pageBtn = CreatePageButton(pageNumber.ToString(), true);
                if (viewModel.Page == pageNumber)
                {
                    pageBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E94560"));
                }
                pageBtn.Click += (s, e) => LoadList(pageNumber);
                PaginationPanel.Children.Add(pageBtn);
            }

            Button next = CreatePageButton("Next", viewModel.Page < viewModel.TotalPages);
            int nextPage = viewModel.Page + 1;
            next.Click += (s, e) => LoadList(nextPage);
            PaginationPanel.Children.Add(next);
        }

        private Button CreatePageButton(string text, bool isEnabled)
        {
            Button button = new Button();
            button.Content = text;
            button.Style = (Style)FindResource("DialogButtonStyle");
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A3A3A"));
            button.Foreground = Brushes.White;
            button.Width = 70;
            button.Height = 30;
            button.Margin = new Thickness(4, 0, 4, 0);
            button.IsEnabled = isEnabled;
            if (isEnabled) button.Opacity = 1.0;
            else button.Opacity = 0.5;
            return button;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
using GigNovaWSClient;
using System.Windows;

namespace GigNovaWPFApp.UserControls
{
    public partial class CategoryDialog : Window
    {
        private string editCategoryId;

        public CategoryDialog()
        {
            InitializeComponent();
        }

        public CategoryDialog(string categoryId, string currentName)
        {
            InitializeComponent();
            editCategoryId = categoryId;
            Title = "Edit Category";
            HeaderText.Text = "Edit Category";
            ActionButton.Content = "Save";
            NameTextBox.Text = currentName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            if (name == "")
            {
                MessageBox.Show("Please enter a category name.", "GigNova");
                return;
            }

            ApiClient<bool> client = new ApiClient<bool>();
            client.Scheme = "https";
            client.Host = "localhost";
            client.Port = 7059;

            if (editCategoryId == null)
            {
                client.Path = "api/Admin/AddCategory";
                client.AddParameter("category_name", name);
            }
            else
            {
                client.Path = "api/Admin/UpdateCategory";
                client.AddParameter("category_id", editCategoryId);
                client.AddParameter("category_name", name);
            }

            bool ok = await client.PostAsync(false);
            if (!ok)
            {
                if (editCategoryId == null) MessageBox.Show("Failed to add category.", "GigNova");
                else MessageBox.Show("Failed to update category.", "GigNova");
                return;
            }
            DialogResult = true;
        }
    }
}
using GigNovaModels.Models;
using GigNovaWSClient;
using System.Collections.Generic;
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

            Category category = new Category();
            category.Category_name = name;
            category.Validate();

            if (category.HasErrors)
            {
                string errorMessage = "";
                foreach (KeyValuePair<string, List<string>> entry in category.AllErrors())
                {
                    if (entry.Value == null)
                    {
                        continue;
                    }
                    foreach (string message in entry.Value)
                    {
                        if (errorMessage != "")
                        {
                            errorMessage += "\n";
                        }
                        errorMessage += message;
                    }
                }
                MessageBox.Show(errorMessage, "GigNova");
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
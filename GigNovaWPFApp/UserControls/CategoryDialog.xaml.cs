using GigNovaModels.Models;
using GigNovaWSClient;
using System.Collections.Generic;
using System.Windows;

namespace GigNovaWPFApp.UserControls
{
    public partial class CategoryDialog : Window
    {
        // Null when this dialog is being used to CREATE a new category.
        // Set to a real id when EDITING an existing category.
        private string editCategoryId;


        // ============================== Constructors ==============================

        // Create-mode constructor.
        public CategoryDialog()
        {
            InitializeComponent();
        }

        // Edit-mode constructor. Pre-fills the name, swaps the header/button text.
        public CategoryDialog(string categoryId, string currentName)
        {
            InitializeComponent();
            editCategoryId = categoryId;
            Title = "Edit Category";
            HeaderText.Text = "Edit Category";
            ActionButton.Content = "Save";
            NameTextBox.Text = currentName;
        }


        // ============================== Buttons ==============================

        // Cancel button: close the dialog with a "no" result.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // Create / Save button: validate the name, then call the right WS endpoint.
        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();

            // Build a temporary Category just to run its validation attributes.
            Category category = new Category();
            category.Category_name = name;
            category.Validate();

            if (category.HasErrors)
            {
                // Collect all error messages (one per failing field) into a single string.
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

            // Pick the right endpoint: AddCategory if creating, UpdateCategory if editing.
            ApiClient<bool> client;
            if (editCategoryId == null)
            {
                client = WpfHelpers.BuildClient<bool>("api/Admin/AddCategory");
                client.AddParameter("category_name", name);
            }
            else
            {
                client = WpfHelpers.BuildClient<bool>("api/Admin/UpdateCategory");
                client.AddParameter("category_id", editCategoryId);
                client.AddParameter("category_name", name);
            }

            bool ok = await client.PostAsync(false);
            if (ok == false)
            {
                if (editCategoryId == null)
                {
                    MessageBox.Show("Failed to add category.", "GigNova");
                }
                else
                {
                    MessageBox.Show("Failed to update category.", "GigNova");
                }
                return;
            }
            DialogResult = true;
        }
    }
}

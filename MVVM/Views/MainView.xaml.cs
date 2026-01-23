using TaskApp.MVVM.ViewModels;
using TaskApp.MVVM.Models;

namespace TaskApp.MVVM.Views;

public partial class MainView : ContentPage
{
    private MainViewModel mainViewModel = new MainViewModel();

    public MainView()
    {
        InitializeComponent();
        BindingContext = mainViewModel;
    }

    private void checkBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        mainViewModel.UpdateData();
        // IMPORTANT: Re-sort tasks when checkbox is changed
        mainViewModel.SortTasks();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await AddTaskWithCategory();
    }

    private async Task<bool> AddTaskWithCategory()
    {
        // STEP 1: Choose category first
        var categoryOptions = mainViewModel.Categories
            .Select(c => c.CategoryName)
            .ToList();
        categoryOptions.Add("➕ Add New Category");

        string selectedCategory = await DisplayActionSheet(
            "Choose Category",
            "Cancel",
            null,
            categoryOptions.ToArray());

        if (selectedCategory == "Cancel" || string.IsNullOrEmpty(selectedCategory))
            return false;

        Category category = null;

        // STEP 2: If user wants to add a new category
        if (selectedCategory == "➕ Add New Category")
        {
            // FIXED: Returns the new category directly
            category = await AddNewCategory();

            if (category == null)
                return false;

            // No need to ask for category selection again - we already have the new category!
        }
        else
        {
            // Find the selected category
            category = mainViewModel.Categories
                .FirstOrDefault(c => c.CategoryName == selectedCategory);

            if (category == null)
                return false;
        }

        // STEP 3: Now ask for task name (after category is chosen)
        string taskName = await DisplayPromptAsync(
            "Task",
            "Enter task name:",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(taskName))
            return false;

        // Create and add the new task
        var newTask = new MyTask
        {
            TaskName = taskName,
            CategoryId = category.Id,
            Completed = false
        };

        mainViewModel.Tasks.Add(newTask);

        await DisplayAlert("Success", $"Task '{taskName}' added to {category.CategoryName}!", "OK");

        return true;
    }

    private async Task<Category> AddNewCategory()
    {
        // Extended pastel colors that will loop (8 colors now)
        string[] pastelColors = {
            "#B3D9FF", // Pastel Blue
            "#E6D9FF", // Pastel Purple
            "#FFD9D9", // Pastel Pink
            "#D9F2E6", // Pastel Green
            "#FFE6CC", // Pastel Orange
            "#E6CCFF", // Pastel Violet
            "#CCFFE6", // Pastel Mint
            "#FFCCF2"  // Pastel Magenta
        };

        string categoryName = await DisplayPromptAsync(
            "Category",
            "Enter category name:",
            maxLength: 50,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        // Get the next color by cycling through the pastel colors
        int colorIndex = (mainViewModel.Categories.Count) % pastelColors.Length;
        string categoryColor = pastelColors[colorIndex];

        var newCategory = new Category
        {
            Id = mainViewModel.Categories.Any()
                ? mainViewModel.Categories.Max(c => c.Id) + 1
                : 1,
            CategoryName = categoryName,
            Color = categoryColor,
            PendingTasks = 0,
            Percentage = 0,
            IsSelected = false
        };

        mainViewModel.Categories.Add(newCategory);

        // FIXED: Return the category object instead of showing success dialog
        // Success dialog will be shown after task is added
        return newCategory;
    }
}
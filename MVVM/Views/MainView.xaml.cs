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
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        var result = await AddTaskWithCategory();
    }

    private async Task<bool> AddTaskWithCategory()
    {
        // Prompt for task name
        string taskName = await DisplayPromptAsync(
            "New Task",
            "Enter task name:",
            placeholder: "Task name",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(taskName))
            return false;

        // Create category selection options
        var categoryOptions = mainViewModel.Categories
            .Select(c => c.CategoryName)
            .ToList();
        categoryOptions.Add("➕ Add New Category");

        // Prompt for category selection
        string selectedCategory = await DisplayActionSheet(
            "Choose Category",
            "Cancel",
            null,
            categoryOptions.ToArray());

        if (selectedCategory == "Cancel" || string.IsNullOrEmpty(selectedCategory))
            return false;

        // If user wants to add a new category
        if (selectedCategory == "➕ Add New Category")
        {
            bool categoryAdded = await AddNewCategory();

            if (!categoryAdded)
                return false;

            // After adding category, ask again for category selection
            categoryOptions = mainViewModel.Categories
                .Select(c => c.CategoryName)
                .ToList();

            selectedCategory = await DisplayActionSheet(
                "Choose Category",
                "Cancel",
                null,
                categoryOptions.ToArray());

            if (selectedCategory == "Cancel" || string.IsNullOrEmpty(selectedCategory))
                return false;
        }

        // Find the selected category
        var category = mainViewModel.Categories
            .FirstOrDefault(c => c.CategoryName == selectedCategory);

        if (category == null)
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

    private async Task<bool> AddNewCategory()
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
            "New Category",
            "Enter category name:",
            placeholder: "Category name",
            maxLength: 50,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(categoryName))
            return false;

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

        await DisplayAlert("Success", $"Category '{categoryName}' added!", "OK");

        return true;
    }
}
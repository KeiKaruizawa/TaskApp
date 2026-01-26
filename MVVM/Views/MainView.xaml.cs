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

    // Handle category tap for selection/filtering
    private void Category_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Category category)
        {
            mainViewModel.ToggleCategorySelection(category);
        }
    }

    // NEW: Handle category double-tap for editing
    private async void Category_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Category category)
        {
            string result = await DisplayPromptAsync(
                "Edit Category",
                "Enter new category name:",
                initialValue: category.CategoryName,
                maxLength: 50,
                keyboard: Keyboard.Text,
                placeholder: "Category name");

            if (!string.IsNullOrWhiteSpace(result))
            {
                mainViewModel.UpdateCategoryName(category, result);
            }
        }
    }

    // NEW: Handle task double-tap for editing
    private async void Task_DoubleTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is MyTask task)
        {
            string result = await DisplayPromptAsync(
                "Edit Task",
                "Enter new task name:",
                initialValue: task.TaskName,
                maxLength: 100,
                keyboard: Keyboard.Text,
                placeholder: "Task name");

            if (!string.IsNullOrWhiteSpace(result))
            {
                mainViewModel.UpdateTaskName(task, result);
            }
        }
    }

    private void checkBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            mainViewModel.UpdateData();
            mainViewModel.SortTasks();
            mainViewModel.ApplyFilter(); // Re-apply filter when task completion changes
        });
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            await AddTaskWithCategory();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
        }
    }

    private async Task<bool> AddTaskWithCategory()
    {
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

        if (selectedCategory == "➕ Add New Category")
        {
            category = await AddNewCategory();
            if (category == null)
                return false;
        }
        else
        {
            category = mainViewModel.Categories
                .FirstOrDefault(c => c.CategoryName == selectedCategory);
            if (category == null)
                return false;
        }

        string taskName = await DisplayPromptAsync(
            "Task",
            "Enter task name:",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(taskName))
            return false;

        var newTask = new MyTask
        {
            TaskName = taskName,
            CategoryId = category.Id,
            Completed = false
        };

        await mainViewModel.AddTaskAsync(newTask);
        await DisplayAlert("Success", $"Task '{taskName}' added to {category.CategoryName}!", "OK");

        return true;
    }

    private async Task<Category> AddNewCategory()
    {
        string[] pastelColors = {
            "#B3D9FF", "#E6D9FF", "#FFD9D9", "#D9F2E6",
            "#FFE6CC", "#E6CCFF", "#CCFFE6", "#FFCCF2"
        };

        string categoryName = await DisplayPromptAsync(
            "Category",
            "Enter category name:",
            maxLength: 50,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

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

        return newCategory;
    }
}
using TaskApp.MVVM.ViewModels;
using TaskApp.MVVM.Models;

namespace TaskApp.MVVM.Views;

public partial class NewTaskView : ContentPage
{
    public NewTaskView()
    {
        InitializeComponent();
    }

    private async void AddTaskClicked(object sender, EventArgs e)
    {
        var viewModel = (NewTaskViewModel)BindingContext;

        if (string.IsNullOrWhiteSpace(viewModel.Task))
        {
            await DisplayAlert("Error", "Please enter a task name", "OK");
            return;
        }

        var selectedCategory = viewModel.Categories.FirstOrDefault(c => c.IsSelected);
        if (selectedCategory == null)
        {
            await DisplayAlert("Error", "Please select a category", "OK");
            return;
        }

        var newTask = new MyTask
        {
            TaskName = viewModel.Task,
            CategoryId = selectedCategory.Id,
            Completed = false
        };

        viewModel.Tasks.Add(newTask);
        await Navigation.PopAsync();
    }

    private async void AddCategoryClicked(object sender, EventArgs e)
    {
        var viewModel = (NewTaskViewModel)BindingContext;

        string categoryName = await DisplayPromptAsync(
            "New Category",
            "Enter category name:",
            placeholder: "Category name",
            maxLength: 50,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(categoryName))
            return;

        string categoryColor = await DisplayPromptAsync(
            "Category Color",
            "Enter hex color (e.g., #B3D9FF):",
            initialValue: "#E0E0E0",
            placeholder: "#RRGGBB",
            maxLength: 7,
            keyboard: Keyboard.Text);

        if (string.IsNullOrWhiteSpace(categoryColor))
            categoryColor = "#E0E0E0";

        // Ensure color starts with #
        if (!categoryColor.StartsWith("#"))
            categoryColor = "#" + categoryColor;

        var newCategory = new Category
        {
            Id = viewModel.Categories.Max(c => c.Id) + 1,
            CategoryName = categoryName,
            Color = categoryColor,
            PendingTasks = 0,
            Percentage = 0,
            IsSelected = false
        };

        viewModel.Categories.Add(newCategory);

        await DisplayAlert("Success", $"Category '{categoryName}' added!", "OK");
    }
}
using TaskApp.MVVM.ViewModels;
using TaskApp.MVVM.Models;

namespace TaskApp.MVVM.Views;

public partial class MainView : ContentPage
{
    private MainViewModel mainViewModel = new MainViewModel();
    private DateTime _pressStartTime;
    private Category? _pressedCategory;
    private bool _isLongPress = false;
    private const int LongPressDurationMs = 600; // 600ms for long press
    private System.Threading.CancellationTokenSource? _longPressCts;

    public MainView()
    {
        InitializeComponent();
        BindingContext = mainViewModel;
    }

    // Handle category tap for selection/filtering (only used for quick taps now)
    private void Category_Tapped(object sender, TappedEventArgs e)
    {
        // This will be handled by Category_Released for consistency
        // Keeping this for double-tap to still work
    }

    // Handle category double-tap for editing
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

    // NEW: Handle category press start (for long press detection)
    private async void Category_Pressed(object sender, EventArgs e)
    {
        if (sender is Button button && button.Parent is Grid grid && grid.BindingContext is Category category)
        {
            _pressedCategory = category;
            _pressStartTime = DateTime.Now;
            _isLongPress = false;

            // Cancel any existing long press detection
            _longPressCts?.Cancel();
            _longPressCts = new System.Threading.CancellationTokenSource();

            try
            {
                // Wait for long press duration
                await Task.Delay(LongPressDurationMs, _longPressCts.Token);

                // If we made it here without cancellation, it's a long press
                _isLongPress = true;

                // Vibrate for haptic feedback (optional)
                try
                {
#if ANDROID || IOS
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
#endif
                }
                catch { }

                // Show the menu
                await ShowCategoryDeleteMenu(category);
            }
            catch (TaskCanceledException)
            {
                // Press was released before long press duration - not a long press
            }
        }
    }

    // NEW: Handle category press release
    private void Category_Released(object sender, EventArgs e)
    {
        // Cancel long press detection if released early
        _longPressCts?.Cancel();

        // If it was a short press (not long press), handle as normal tap
        if (!_isLongPress && _pressedCategory != null)
        {
            var timeSincePress = (DateTime.Now - _pressStartTime).TotalMilliseconds;
            if (timeSincePress < LongPressDurationMs)
            {
                // This was a short press - toggle selection
                mainViewModel.ToggleCategorySelection(_pressedCategory);
            }
        }

        _pressedCategory = null;
    }

    // Empty click handler to prevent button click from interfering
    private void Category_ClickedEmpty(object sender, EventArgs e)
    {
        // Do nothing - we handle press/release separately
    }

    // Show category delete menu with confirmation
    private async Task ShowCategoryDeleteMenu(Category category)
    {
        var action = await DisplayActionSheet(
            $"Category: {category.CategoryName}",
            "Cancel",
            "Delete Category",
            "Edit Name");

        if (action == "Delete Category")
        {
            await DeleteCategoryWithConfirmation(category);
        }
        else if (action == "Edit Name")
        {
            // Reuse existing edit functionality
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

    // Delete category with confirmation
    private async Task DeleteCategoryWithConfirmation(Category category)
    {
        // Count tasks in this category
        var tasksInCategory = mainViewModel.Tasks.Count(t => t.CategoryId == category.Id);

        string message = tasksInCategory > 0
            ? $"Delete '{category.CategoryName}'? This will also delete {tasksInCategory} task(s) in this category."
            : $"Delete '{category.CategoryName}'?";

        bool confirm = await DisplayAlert(
            "Confirm Delete",
            message,
            "Delete",
            "Cancel");

        if (confirm)
        {
            mainViewModel.DeleteCategory(category);
            await DisplayAlert("Deleted", $"'{category.CategoryName}' has been deleted.", "OK");
        }
    }

    // Handle task double-tap for editing
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

    // NEW: Handle task swipe to delete
    private async void Task_SwipeDelete(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is MyTask task)
        {
            bool confirm = await DisplayAlert(
                "Confirm Delete",
                $"Delete task '{task.TaskName}'?",
                "Delete",
                "Cancel");

            if (confirm)
            {
                mainViewModel.DeleteTask(task);
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
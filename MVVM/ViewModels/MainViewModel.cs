using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApp.MVVM.Models;
using PropertyChanged;

namespace TaskApp.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<MyTask> Tasks { get; set; }

        // Separate collections for pending and completed tasks
        public ObservableCollection<MyTask> PendingTasks { get; set; }
        public ObservableCollection<MyTask> CompletedTasks { get; set; }

        // Property to show/hide "Completed Tasks" label
        public bool HasCompletedTasks => CompletedTasks?.Count > 0;

        private bool _isSorting = false;
        private bool _isUpdating = false;

        public MainViewModel()
        {
            PendingTasks = new ObservableCollection<MyTask>();
            CompletedTasks = new ObservableCollection<MyTask>();
            FillData();
            Tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isSorting || _isUpdating) return;

            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateData();
            });
        }

        private void FillData()
        {
            Categories = new ObservableCollection<Category>
            {
                new Category
                {
                    Id = 1,
                    CategoryName = "Assignment",
                    Color = "#B3D9FF",
                    IsSelected = false
                },
                new Category
                {
                    Id = 2,
                    CategoryName = "Quiz",
                    Color = "#E6D9FF",
                    IsSelected = false
                },
                new Category
                {
                    Id = 3,
                    CategoryName = "Personal",
                    Color = "#FFD9D9",
                    IsSelected = false
                },
                new Category
                {
                    Id = 4,
                    CategoryName = "Health",
                    Color = "#D9F2E6",
                    IsSelected = false
                }
            };

            Tasks = new ObservableCollection<MyTask>
            {
                new MyTask
                {
                    TaskName = "ITPRA - Application Letter",
                    Completed = false,
                    CategoryId = 1
                },
                new MyTask
                {
                    TaskName = "Capstone - Write RRS",
                    Completed = false,
                    CategoryId = 1
                },
                new MyTask
                {
                    TaskName = "PROEL3 - online quiz (Monday)",
                    Completed = false,
                    CategoryId = 2
                },
                new MyTask
                {
                    TaskName = "SOCPRO - short quiz (Tuesday)",
                    Completed = false,
                    CategoryId = 2
                },
                new MyTask
                {
                    TaskName = "Do laundry",
                    Completed = true,
                    CategoryId = 3
                },
                new MyTask
                {
                    TaskName = "Organize files and folders",
                    Completed = false,
                    CategoryId = 3
                },
                new MyTask
                {
                    TaskName = "Track expenses",
                    Completed = false,
                    CategoryId = 3
                },
                new MyTask
                {
                    TaskName = "Stretch after long screen time",
                    Completed = false,
                    CategoryId = 4
                }
            };

            UpdateData();
            ApplyFilter(); // Apply initial filter (shows all)
        }

        public void UpdateData()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            try
            {
                foreach (var c in Categories)
                {
                    var tasks = from t in Tasks
                                where t.CategoryId == c.Id
                                select t;

                    var completed = from t in tasks
                                    where t.Completed == true
                                    select t;

                    var notCompleted = from t in tasks
                                       where t.Completed == false
                                       select t;

                    c.PendingTasks = notCompleted.Count();

                    if (tasks.Count() > 0)
                        c.Percentage = (float)completed.Count() / (float)tasks.Count();
                    else
                        c.Percentage = 0;
                }

                foreach (var t in Tasks)
                {
                    var catColor =
                         (from c in Categories
                          where c.Id == t.CategoryId
                          select c.Color).FirstOrDefault();
                    t.TaskColor = catColor;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        public async Task AddTaskAsync(MyTask task)
        {
            Tasks.Add(task);

            await Task.Delay(50);

            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
            {
                UpdateData();
                ApplyFilter(); // Re-apply filter after adding task
            });
        }

        // Keep this method for backward compatibility with MainView.cs
        public void SortTasks()
        {
            // Sorting is now handled in ApplyFilter(), but keeping this for compatibility
            ApplyFilter();
        }

        // Toggle category selection and apply filter
        public void ToggleCategorySelection(Category category)
        {
            category.IsSelected = !category.IsSelected;

            // Defer filter update to avoid RecyclerView conflicts
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                ApplyFilter();
            });
        }

        // UPDATED: Filter tasks and split into pending and completed
        public void ApplyFilter()
        {
            // Get selected category IDs
            var selectedCategoryIds = Categories
                .Where(c => c.IsSelected)
                .Select(c => c.Id)
                .ToList();

            // Determine which tasks should be visible
            List<MyTask> tasksToShow;

            // If no categories are selected, show all tasks
            if (selectedCategoryIds.Count == 0)
            {
                tasksToShow = Tasks.ToList();
            }
            else
            {
                // Show only tasks from selected categories
                tasksToShow = Tasks
                    .Where(t => selectedCategoryIds.Contains(t.CategoryId))
                    .ToList();
            }

            // Split into pending and completed
            var pendingList = tasksToShow.Where(t => !t.Completed).ToList();
            var completedList = tasksToShow.Where(t => t.Completed).ToList();

            // Update PendingTasks
            UpdateCollection(PendingTasks, pendingList);

            // Update CompletedTasks
            UpdateCollection(CompletedTasks, completedList);
        }

        // Helper method to efficiently update an ObservableCollection
        private void UpdateCollection(ObservableCollection<MyTask> collection, List<MyTask> newItems)
        {
            // Remove tasks that shouldn't be visible
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (!newItems.Contains(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }

            // Add or reorder tasks
            for (int i = 0; i < newItems.Count; i++)
            {
                var task = newItems[i];
                var currentIndex = collection.IndexOf(task);

                if (currentIndex == -1)
                {
                    // Task not in collection, add it at correct position
                    collection.Insert(i, task);
                }
                else if (currentIndex != i)
                {
                    // Task exists but in wrong position, move it
                    collection.Move(currentIndex, i);
                }
            }
        }

        // NEW: Delete a task
        public void DeleteTask(MyTask task)
        {
            if (task != null && Tasks.Contains(task))
            {
                Tasks.Remove(task);

                // Update UI
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateData();
                    ApplyFilter();
                });
            }
        }

        // NEW: Delete a category and all its tasks
        public void DeleteCategory(Category category)
        {
            if (category != null && Categories.Contains(category))
            {
                // First, remove all tasks in this category
                var tasksToRemove = Tasks.Where(t => t.CategoryId == category.Id).ToList();
                foreach (var task in tasksToRemove)
                {
                    Tasks.Remove(task);
                }

                // Then remove the category
                Categories.Remove(category);

                // Update UI
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateData();
                    ApplyFilter();
                });
            }
        }

        // Edit category name
        public void UpdateCategoryName(Category category, string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                category.CategoryName = newName;
            }
        }

        // Edit task name
        public void UpdateTaskName(MyTask task, string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                task.TaskName = newName;
            }
        }
    }
}
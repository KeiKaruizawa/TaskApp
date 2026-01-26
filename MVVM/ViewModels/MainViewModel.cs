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

        // Filtered tasks that will be displayed
        public ObservableCollection<MyTask> FilteredTasks { get; set; }

        private bool _isSorting = false;
        private bool _isUpdating = false;

        public MainViewModel()
        {
            FilteredTasks = new ObservableCollection<MyTask>();
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
            SortTasks();
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
                SortTasks();
                ApplyFilter(); // Re-apply filter after adding task
            });
        }

        public void SortTasks()
        {
            if (_isSorting) return;

            _isSorting = true;

            try
            {
                var sortedTasks = Tasks.OrderBy(t => t.Completed).ToList();

                for (int i = 0; i < sortedTasks.Count; i++)
                {
                    var task = sortedTasks[i];
                    var currentIndex = Tasks.IndexOf(task);

                    if (currentIndex != i && currentIndex >= 0)
                    {
                        Tasks.Move(currentIndex, i);
                    }
                }
            }
            finally
            {
                _isSorting = false;
            }
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

        // UPDATED: Filter tasks based on selected categories WITH SORTING
        public void ApplyFilter()
        {
            // Get selected category IDs
            var selectedCategoryIds = Categories
                .Where(c => c.IsSelected)
                .Select(c => c.Id)
                .ToList();

            // Determine which tasks should be visible (already sorted)
            List<MyTask> tasksToShow;

            // If no categories are selected, show all tasks
            if (selectedCategoryIds.Count == 0)
            {
                tasksToShow = Tasks.OrderBy(t => t.Completed).ToList(); // SORT HERE
            }
            else
            {
                // Show only tasks from selected categories
                tasksToShow = Tasks
                    .Where(t => selectedCategoryIds.Contains(t.CategoryId))
                    .OrderBy(t => t.Completed) // SORT HERE
                    .ToList();
            }

            // Update FilteredTasks efficiently to avoid RecyclerView issues
            // Remove tasks that shouldn't be visible
            for (int i = FilteredTasks.Count - 1; i >= 0; i--)
            {
                if (!tasksToShow.Contains(FilteredTasks[i]))
                {
                    FilteredTasks.RemoveAt(i);
                }
            }

            // Add or reorder tasks
            for (int i = 0; i < tasksToShow.Count; i++)
            {
                var task = tasksToShow[i];
                var currentIndex = FilteredTasks.IndexOf(task);

                if (currentIndex == -1)
                {
                    // Task not in FilteredTasks, add it at correct position
                    FilteredTasks.Insert(i, task);
                }
                else if (currentIndex != i)
                {
                    // Task exists but in wrong position, move it
                    FilteredTasks.Move(currentIndex, i);
                }
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
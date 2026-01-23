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

        // NEW: Toggle category selection and apply filter
        public void ToggleCategorySelection(Category category)
        {
            category.IsSelected = !category.IsSelected;

            // Defer filter update to avoid RecyclerView conflicts
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                ApplyFilter();
            });
        }

        // NEW: Filter tasks based on selected categories
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

            // Update FilteredTasks collection efficiently
            // Remove tasks that shouldn't be visible
            for (int i = FilteredTasks.Count - 1; i >= 0; i--)
            {
                if (!tasksToShow.Contains(FilteredTasks[i]))
                {
                    FilteredTasks.RemoveAt(i);
                }
            }

            // Add tasks that should be visible but aren't in the collection yet
            foreach (var task in tasksToShow)
            {
                if (!FilteredTasks.Contains(task))
                {
                    // Insert in correct position to maintain sort order
                    int insertIndex = FilteredTasks.Count;
                    for (int i = 0; i < FilteredTasks.Count; i++)
                    {
                        int taskIndex = Tasks.IndexOf(task);
                        int existingIndex = Tasks.IndexOf(FilteredTasks[i]);
                        if (taskIndex < existingIndex)
                        {
                            insertIndex = i;
                            break;
                        }
                    }
                    FilteredTasks.Insert(insertIndex, task);
                }
            }
        }
    }
}
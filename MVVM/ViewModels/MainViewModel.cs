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

        public MainViewModel()
        {
            FillData();
            Tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateData();
        }

        private void FillData()
        {
            Categories = new ObservableCollection<Category>
            {
                new Category
                {
                    Id = 1,
                    CategoryName = "Assignment",
                    Color = "#B3D9FF" // Pastel Blue
                },
                new Category
                {
                    Id = 2,
                    CategoryName = "Quiz",
                    Color = "#E6D9FF" // Pastel Purple
                },
                new Category
                {
                    Id = 3,
                    CategoryName = "Personal",
                    Color = "#FFD9D9" // Pastel Peach/Pink
                },
                new Category
                {
                    Id = 4,
                    CategoryName = "Health",
                    Color = "#D9F2E6" // Pastel Green
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
        }

        public void UpdateData()
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
                c.Percentage = (float)completed.Count() / (float)tasks.Count();
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
    }
}
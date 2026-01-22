using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace TaskApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Color { get; set; }
        public int PendingTasks { get; set; }
        public float Percentage { get; set; }
        public bool IsSelected { get; set; }

        // Property for darker progress bar color
        public string DarkerColor
        {
            get
            {
                var upperColor = Color?.ToUpper();
                return upperColor switch
                {
                    "#B3D9FF" => "#6BA3E0", // Darker blue for pastel blue
                    "#E6D9FF" => "#B89FE0", // Darker purple for pastel purple
                    "#FFD9D9" => "#E09999", // Darker peach for pastel pink
                    "#D9F2E6" => "#8FD4B3", // Darker green for pastel green
                    "#FFE6CC" => "#E0B380", // Darker orange for pastel orange
                    "#E6CCFF" => "#B380E0", // Darker violet for pastel violet
                    "#CCFFE6" => "#80E0B3", // Darker mint for pastel mint
                    "#FFCCF2" => "#E080C4", // Darker pink for pastel magenta
                    _ => "#999999" // Default darker gray
                };
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SudokuSolver.WindowsForms.Helpers
{
    public static class ControlHelper
    {
        public static IEnumerable<T> GetDescendants<T>(this Control control)
        {
            IEnumerable<Control> controls = control.Controls.Cast<Control>();
            return controls
                .OfType<T>()
                .Concat(controls.SelectMany(ctrl => GetDescendants<T>(ctrl)));
        }
    }
}
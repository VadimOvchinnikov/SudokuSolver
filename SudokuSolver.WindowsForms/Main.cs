using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SudokuSolver.WindowsForms.Helpers;

namespace SudokuSolver.WindowsForms
{
    public partial class Main : Form
    {
        private IEnumerable<NumericUpDown> NumericUpDownControls => this.GetDescendants<NumericUpDown>();

        public Main()
        {
            InitializeComponent();

            TilesTableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            foreach (NumericUpDown numericUpDown in NumericUpDownControls)
                numericUpDown.Enter += (o, s) => numericUpDown.Select(0, numericUpDown.Text.Length);
        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            var groupedByRow = NumericUpDownControls
                .Select(n => TilesTableLayoutPanel.GetCellPosition(n))
                .OrderBy(p => p.Row)
                .ThenBy(p => p.Column)
                .GroupBy(p => p.Row);

            string[] tileDefinitions = groupedByRow.Select(g => new string(
                g.Select(p =>
                {
                    NumericUpDown n = (NumericUpDown)TilesTableLayoutPanel.GetControlFromPosition(p.Column, p.Row);
                    return n.Value == 0 ? '.' : n.Value.ToString().Single();
                }).ToArray()
            )).ToArray();

            SudokuBoard board = SudokuFactory.ClassicWith3x3Boxes(tileDefinitions);

            IEnumerable<SudokuBoard> solutions = board.Solve();

            if (!solutions.Any())
            {
                MessageBox.Show("No solution available!");
                return;
            }

            if (solutions.Count() > 1)
            {
                MessageBox.Show("Multiple solutions available!");
                return;
            }

            string[] tileDefinion = solutions.Single().TileDefinitions;

            foreach (NumericUpDown numericUpDown in NumericUpDownControls)
            {
                var position = TilesTableLayoutPanel.GetCellPosition(numericUpDown);
                numericUpDown.Value = (int)char.GetNumericValue(tileDefinion[position.Row][position.Column]);
            }
        }
    }
}
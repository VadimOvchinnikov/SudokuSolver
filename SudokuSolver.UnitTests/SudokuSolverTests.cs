using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SudokuSolver.UnitTests
{
    public class SudokuSolverTests
    {
        [Fact]
        public void SudokuBoard_Solve_NoSolutionFound()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.SizeAndBoxes(4, 4, 2, 2);
            board.AddRow("0003");
            board.AddRow("0204"); // the 2 must be a 1 on this row to be solvable
            board.AddRow("1000");
            board.AddRow("4000");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.False(solutions.Any());
        }

        [Fact]
        public void SudokuBoard_Solve_ClassicWithSolution()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.ClassicWith3x3Boxes();
            board.AddRow("...84...9");
            board.AddRow("..1.....5");
            board.AddRow("8...2146.");
            board.AddRow("7.8....9.");
            board.AddRow(".........");
            board.AddRow(".5....3.1");
            board.AddRow(".2491...7");
            board.AddRow("9.....5..");
            board.AddRow("3...84...");

            SudokuBoard solution = SudokuFactory.ClassicWith3x3Boxes();
            solution.AddRow("632845179");
            solution.AddRow("471369285");
            solution.AddRow("895721463");
            solution.AddRow("748153692");
            solution.AddRow("163492758");
            solution.AddRow("259678341");
            solution.AddRow("524916837");
            solution.AddRow("986237514");
            solution.AddRow("317584926");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(solutions.First(), solution);
        }

        [Fact]
        public void SudokoBoard_Solve_ClassicWithMultipleSolutions()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.ClassicWith3x3Boxes();
            board.AddRow("...84...9");
            board.AddRow("..1.....5");
            board.AddRow("8...2.46."); // Removed a "1" on this line
            board.AddRow("7.8....9.");
            board.AddRow(".........");
            board.AddRow(".5....3.1");
            board.AddRow(".2491...7");
            board.AddRow("9.....5..");
            board.AddRow("3...84...");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Equal(20, solutions.Count());
        }

        [Fact]
        public void SudukoBoard_Solve_SmallWithSolution()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.SizeAndBoxes(4, 4, 2, 2);
            board.AddRow("0003");
            board.AddRow("0004");
            board.AddRow("1000");
            board.AddRow("4000");

            SudokuBoard solution = SudokuFactory.SizeAndBoxes(4, 4, 2, 2);
            solution.AddRow("2413");
            solution.AddRow("3124");
            solution.AddRow("1342");
            solution.AddRow("4231");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(solutions.First(), solution);
        }

        [Fact]
        public void SudokoBoard_Solve_ExtraZonesWithSolution()
        {
            // Arrange
            // http://en.wikipedia.org/wiki/File:Oceans_Hypersudoku18_Puzzle.svg
            SudokuBoard board = SudokuFactory.ClassicWith3x3BoxesAndHyperRegions();
            board.AddRow(".......1.");
            board.AddRow("..2....34");
            board.AddRow("....51...");
            board.AddRow(".....65..");
            board.AddRow(".7.3...8.");
            board.AddRow("..3......");
            board.AddRow("....8....");
            board.AddRow("58....9..");
            board.AddRow("69.......");

            SudokuBoard solution = SudokuFactory.ClassicWith3x3BoxesAndHyperRegions();
            solution.AddRow("946832715");
            solution.AddRow("152697834");
            solution.AddRow("738451296");
            solution.AddRow("819726543");
            solution.AddRow("475319682");
            solution.AddRow("263548179");
            solution.AddRow("327985461");
            solution.AddRow("584163927");
            solution.AddRow("691274358");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(solutions.First(), solution);
        }

        [Fact]
        public void SudokoBoard_Solve_HyperWithSolution()
        {
            // Arrange
            // http://en.wikipedia.org/wiki/File:A_nonomino_sudoku.svg
            string[] areas = new string[]
            {
                "111233333",
                "111222333",
                "144442223",
                "114555522",
                "444456666",
                "775555688",
                "977766668",
                "999777888",
                "999997888"
            };
            SudokuBoard board = SudokuFactory.ClassicWithSpecialBoxes(areas);
            board.AddRow("3.......4");
            board.AddRow("..2.6.1..");
            board.AddRow(".1.9.8.2.");
            board.AddRow("..5...6..");
            board.AddRow(".2.....1.");
            board.AddRow("..9...8..");
            board.AddRow(".8.3.4.6.");
            board.AddRow("..4.1.9..");
            board.AddRow("5.......7");

            SudokuBoard solution = SudokuFactory.ClassicWith3x3BoxesAndHyperRegions();
            solution.AddRow("358196274");
            solution.AddRow("492567138");
            solution.AddRow("613978425");
            solution.AddRow("175842693");
            solution.AddRow("826453719");
            solution.AddRow("249731856");
            solution.AddRow("987324561");
            solution.AddRow("734615982");
            solution.AddRow("561289347");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(solutions.First(), solution);
        }

        [Fact]
        public void SudokoBoard_Solve_SamuraiWithSolution()
        {
            // Arrange
            // http://www.freesamuraisudoku.com/1001HardSamuraiSudokus.aspx?puzzle=42
            SudokuBoard board = SudokuFactory.Samurai();
            board.AddRow("6..8..9..///.....38..");
            board.AddRow("...79....///89..2.3..");
            board.AddRow("..2..64.5///...1...7.");
            board.AddRow(".57.1.2..///..5....3.");
            board.AddRow(".....731.///.1.3..2..");
            board.AddRow("...3...9.///.7..429.5");
            board.AddRow("4..5..1...5....5.....");
            board.AddRow("8.1...7...8.2..768...");
            board.AddRow(".......8.23...4...6..");
            board.AddRow("//////.12.4..9.//////");
            board.AddRow("//////......82.//////");
            board.AddRow("//////.6.....1.//////");
            board.AddRow(".4...1....76...36..9.");
            board.AddRow("2.....9..8..5.34...81");
            board.AddRow(".5.873......9.8..23..");
            board.AddRow("...2....9///.25.4....");
            board.AddRow("..3.64...///31.8.....");
            board.AddRow("..75.8.12///...6.14..");
            board.AddRow(".......2.///.31...9..");
            board.AddRow("..17.....///..7......");
            board.AddRow(".7.6...84///8...7..5.");

            SudokuBoard solution = SudokuFactory.Samurai();
            solution.AddRow("674825931000142673859");
            solution.AddRow("513794862000897425361");
            solution.AddRow("982136475000563189472");
            solution.AddRow("357619248000425916738");
            solution.AddRow("298457316000918357246");
            solution.AddRow("146382597000376842915");
            solution.AddRow("469578123457689534127");
            solution.AddRow("821963754689231768594");
            solution.AddRow("735241689231754291683");
            solution.AddRow("000000512748396000000");
            solution.AddRow("000000497163825000000");
            solution.AddRow("000000368592417000000");
            solution.AddRow("746921835976142368597");
            solution.AddRow("238456971824563497281");
            solution.AddRow("159873246315978152346");
            solution.AddRow("815237469000625749813");
            solution.AddRow("923164758000314825769");
            solution.AddRow("467598312000789631425");
            solution.AddRow("694385127000431586972");
            solution.AddRow("581742693000257914638");
            solution.AddRow("372619584000896273154");

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(solutions.First(), solution);
        }
    }
}
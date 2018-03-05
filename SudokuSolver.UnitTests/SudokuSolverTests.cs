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
            SudokuBoard board = SudokuFactory.SizeAndBoxes(4, 4, 2, 2, new[]
            {
                "0003",
                "0204", // the 2 must be a 1 on this row to be solvable
                "1000",
                "4000"
            });

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.False(solutions.Any());
        }

        [Fact]
        public void SudokuBoard_Solve_ClassicWithSolution()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.ClassicWith3x3Boxes(new[]
            {
                "...84...9",
                "..1.....5",
                "8...2146.",
                "7.8....9.",
                ".........",
                ".5....3.1",
                ".2491...7",
                "9.....5..",
                "3...84..."
            });

            string[] tileDefinitions = new[]
            {
                "632845179",
                "471369285",
                "895721463",
                "748153692",
                "163492758",
                "259678341",
                "524916837",
                "986237514",
                "317584926",
            };

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(tileDefinitions, solutions.First().TileDefinitions);
        }

        [Fact]
        public void SudokoBoard_Solve_ClassicWithMultipleSolutions()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.ClassicWith3x3Boxes(new[]
            {
                "...84...9",
                "..1.....5",
                "8...2.46.", // Removed a "1" on this line
                "7.8....9.",
                ".........",
                ".5....3.1",
                ".2491...7",
                "9.....5..",
                "3...84..."
            });

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Equal(20, solutions.Count());
        }

        [Fact]
        public void SudukoBoard_Solve_SmallWithSolution()
        {
            // Arrange
            SudokuBoard board = SudokuFactory.SizeAndBoxes(4, 4, 2, 2, new[]
            {
                "0003",
                "0004",
                "1000",
                "4000"
            });

            string[] tileDefinitions = new[]
            {
                "2413",
                "3124",
                "1342",
                "4231"
            };

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(tileDefinitions, solutions.Single().TileDefinitions);
        }

        [Fact]
        public void SudokoBoard_Solve_ExtraZonesWithSolution()
        {
            // Arrange
            // http://en.wikipedia.org/wiki/File:Oceans_Hypersudoku18_Puzzle.svg
            SudokuBoard board = SudokuFactory.ClassicWith3x3BoxesAndHyperRegions(new[]
            {
                ".......1.",
                "..2....34",
                "....51...",
                ".....65..",
                ".7.3...8.",
                "..3......",
                "....8....",
                "58....9..",
                "69......."
            });

            string[] tileDefinitions = new[]
            {
                "946832715",
                "152697834",
                "738451296",
                "819726543",
                "475319682",
                "263548179",
                "327985461",
                "584163927",
                "691274358"
            };

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(tileDefinitions, solutions.First().TileDefinitions);
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
            SudokuBoard board = SudokuFactory.ClassicWithSpecialBoxes(areas, new[]
            {
                "3.......4",
                "..2.6.1..",
                ".1.9.8.2.",
                "..5...6..",
                ".2.....1.",
                "..9...8..",
                ".8.3.4.6.",
                "..4.1.9..",
                "5.......7"
            });

            string[] tileDefinitions = new[]
            {
                "358196274",
                "492567138",
                "613978425",
                "175842693",
                "826453719",
                "249731856",
                "987324561",
                "734615982",
                "561289347"
            };

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(tileDefinitions, solutions.First().TileDefinitions);
        }

        [Fact]
        public void SudokoBoard_Solve_SamuraiWithSolution()
        {
            // Arrange
            // http://www.freesamuraisudoku.com/1001HardSamuraiSudokus.aspx?puzzle=42
            SudokuBoard board = SudokuFactory.Samurai(new[]
            {
                "6..8..9..///.....38..",
                "...79....///89..2.3..",
                "..2..64.5///...1...7.",
                ".57.1.2..///..5....3.",
                ".....731.///.1.3..2..",
                "...3...9.///.7..429.5",
                "4..5..1...5....5.....",
                "8.1...7...8.2..768...",
                ".......8.23...4...6..",
                "//////.12.4..9.//////",
                "//////......82.//////",
                "//////.6.....1.//////",
                ".4...1....76...36..9.",
                "2.....9..8..5.34...81",
                ".5.873......9.8..23..",
                "...2....9///.25.4....",
                "..3.64...///31.8.....",
                "..75.8.12///...6.14..",
                ".......2.///.31...9..",
                "..17.....///..7......",
                ".7.6...84///8...7..5."
            });

            string[] tileDefinitions = new[]
            {
                "674825931000142673859",
                "513794862000897425361",
                "982136475000563189472",
                "357619248000425916738",
                "298457316000918357246",
                "146382597000376842915",
                "469578123457689534127",
                "821963754689231768594",
                "735241689231754291683",
                "000000512748396000000",
                "000000497163825000000",
                "000000368592417000000",
                "746921835976142368597",
                "238456971824563497281",
                "159873246315978152346",
                "815237469000625749813",
                "923164758000314825769",
                "467598312000789631425",
                "694385127000431586972",
                "581742693000257914638",
                "372619584000896273154"
            };

            // Act
            IEnumerable<SudokuBoard> solutions = board.Solve();

            // Assert
            Assert.Single(solutions);
            Assert.Equal(tileDefinitions, solutions.First().TileDefinitions);
        }
    }
}
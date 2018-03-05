using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    internal static class Program
    {
        private static void Main()
        {
            SolveFail();
            SolveClassic();
            SolveSmall();
            SolveExtraZones();
            SolveHyper();
            SolveSamurai();
            SolveIncompleteClassic();
        }

        private static void SolveFail()
        {
            SudokuBoard board = SudokuFactory.SizeAndBoxes(4, 4, 2, 2, new[]
            {
                "0003",
                "0204", // the 2 must be a 1 on this row to be solvable
                "1000",
                "4000"
            });
            CompleteSolve(board);
        }

        private static void SolveExtraZones()
        {
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
            CompleteSolve(board);
        }

        private static void SolveSmall()
        {
            SudokuBoard board = SudokuFactory.SizeAndBoxes(4, 4, 2, 2, new[]
            {
                "0003",
                "0004",
                "1000",
                "4000"
            });
            CompleteSolve(board);
        }

        private static void SolveHyper()
        {
            // http://en.wikipedia.org/wiki/File:A_nonomino_sudoku.svg
            string[] areas = new[]
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
            CompleteSolve(board);
        }

        private static void SolveSamurai()
        {
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
            CompleteSolve(board);
        }

        private static void SolveClassic()
        {
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
            CompleteSolve(board);
        }

        private static void SolveIncompleteClassic()
        {
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
            CompleteSolve(board);
        }

        private static void CompleteSolve(SudokuBoard board)
        {
            Console.WriteLine("Board:");
            Output(board);
            List<SudokuBoard> solutions = board.Solve().ToList();
            Console.WriteLine($"All {solutions.Count} solutions:");
            int i = 1;
            foreach (SudokuBoard solution in solutions)
            {
                Console.WriteLine("----------------");
                Console.WriteLine($"Solution {i++} / {solutions.Count}:");
                Output(solution);
            }
        }

        private static void Output(SudokuBoard board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Console.Write(board[x, y].ToStringSimple());
                }
                Console.WriteLine();
            }
        }
    }
}
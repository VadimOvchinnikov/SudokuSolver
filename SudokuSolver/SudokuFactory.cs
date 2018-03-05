using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuFactory
    {
        private const int DefaultSize = 9;
        private const int SamuraiAreas = 7;
        private const int BoxSize = 3;
        private const int HyperMargin = 1;

        public static IEnumerable<Point> Box(int sizeX, int sizeY)
        {
            return
                from x in Enumerable.Range(0, sizeX)
                from y in Enumerable.Range(0, sizeY)
                select new Point(x, y);
        }

        public static SudokuBoard Samurai()
        {
            SudokuBoard board = new SudokuBoard(SamuraiAreas * BoxSize, SamuraiAreas * BoxSize, DefaultSize);
            // Removed the empty areas where there are no tiles
            List<IEnumerable<SudokuTile>> queriesForBlocked = new List<IEnumerable<SudokuTile>>
            {
                from pos in Box(BoxSize, BoxSize * 2) select board[pos.X + DefaultSize, pos.Y],
                from pos in Box(BoxSize, BoxSize * 2) select board[pos.X + DefaultSize, pos.Y + DefaultSize * 2 - BoxSize],
                from pos in Box(BoxSize * 2, BoxSize) select board[pos.X, pos.Y + DefaultSize],
                from pos in Box(BoxSize * 2, BoxSize) select board[pos.X + DefaultSize * 2 - BoxSize, pos.Y + DefaultSize]
            };
            foreach (IEnumerable<SudokuTile> query in queriesForBlocked)
            {
                foreach (SudokuTile tile in query) tile.Block();
            }

            // Select the tiles in the 3 x 3 area (area.X, area.Y) and create rules for them
            foreach (Point area in Box(SamuraiAreas, SamuraiAreas))
            {
                IEnumerable<SudokuTile> tilesInArea = from pos in Box(BoxSize, BoxSize) select board[area.X * BoxSize + pos.X, area.Y * BoxSize + pos.Y];
                if (tilesInArea.First().IsBlocked)
                    continue;
                board.CreateRule($"Area {area.X}, {area.Y}", tilesInArea);
            }

            // Select all rows and create columns for them
            foreach (int posSet in Enumerable.Range(0, board.Width))
            {
                board.CreateRule($"Column Upper {posSet}", from pos in Box(1, DefaultSize) select board[posSet, pos.Y]);
                board.CreateRule($"Column Lower {posSet}", from pos in Box(1, DefaultSize) select board[posSet, pos.Y + DefaultSize + BoxSize]);

                board.CreateRule($"Row Left {posSet}", from pos in Box(DefaultSize, 1) select board[pos.X, posSet]);
                board.CreateRule($"Row Right {posSet}", from pos in Box(DefaultSize, 1) select board[pos.X + DefaultSize + BoxSize, posSet]);

                if (posSet >= BoxSize * 2 && posSet < BoxSize * 2 + DefaultSize)
                {
                    // Create rules for the middle sudoku
                    board.CreateRule($"Column Middle {posSet}", from pos in Box(1, 9) select board[posSet, pos.Y + BoxSize * 2]);
                    board.CreateRule($"Row Middle {posSet}", from pos in Box(9, 1) select board[pos.X + BoxSize * 2, posSet]);
                }
            }
            return board;
        }

        public static SudokuBoard SizeAndBoxes(int width, int height, int boxCountX, int boxCountY)
        {
            SudokuBoard board = new SudokuBoard(width, height);
            board.AddBoxesCount(boxCountX, boxCountY);
            return board;
        }

        public static SudokuBoard ClassicWith3x3Boxes() => SizeAndBoxes(DefaultSize, DefaultSize, DefaultSize / BoxSize, DefaultSize / BoxSize);

        public static SudokuBoard ClassicWith3x3BoxesAndHyperRegions()
        {
            SudokuBoard board = ClassicWith3x3Boxes();
            const int HyperSecond = HyperMargin + BoxSize + HyperMargin;
            // Create the four extra hyper regions
            board.CreateRule("HyperA", from pos in Box(3, 3) select board[pos.X + HyperMargin, pos.Y + HyperMargin]);
            board.CreateRule("HyperB", from pos in Box(3, 3) select board[pos.X + HyperSecond, pos.Y + HyperMargin]);
            board.CreateRule("HyperC", from pos in Box(3, 3) select board[pos.X + HyperMargin, pos.Y + HyperSecond]);
            board.CreateRule("HyperD", from pos in Box(3, 3) select board[pos.X + HyperSecond, pos.Y + HyperSecond]);
            return board;
        }

        public static SudokuBoard ClassicWithSpecialBoxes(string[] areas)
        {
            int sizeX = areas[0].Length;
            int sizeY = areas.Length;
            SudokuBoard board = new SudokuBoard(sizeX, sizeY);
            string joinedString = string.Join("", areas);
            IEnumerable<char> grouped = joinedString.Distinct();

            // Loop through all the unique characters
            foreach (char ch in grouped)
            {
                // Select the rule tiles based on the index of the character
                IEnumerable<SudokuTile> ruleTiles = from i in Enumerable.Range(0, joinedString.Length)
                                where joinedString[i] == ch // filter out any non-matching characters
                                select board[i % sizeX, i / sizeY];
                board.CreateRule($"Area {ch}", ruleTiles);
            }

            return board;
        }
    }
}
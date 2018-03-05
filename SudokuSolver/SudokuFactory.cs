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

        public static SudokuBoard Samurai(string[] tileDefinitions)
        {
            SudokuBoard board = new SudokuBoard(SamuraiAreas * BoxSize, SamuraiAreas * BoxSize, DefaultSize, tileDefinitions);
            // Removed the empty areas where there are no tiles
            IEnumerable<SudokuTile> tiles = new[]
            {
                Box(BoxSize, BoxSize * 2).Select(pos => board[pos.X + DefaultSize, pos.Y]),
                Box(BoxSize, BoxSize * 2).Select(pos => board[pos.X + DefaultSize, pos.Y + DefaultSize * 2 - BoxSize]),
                Box(BoxSize * 2, BoxSize).Select(pos => board[pos.X, pos.Y + DefaultSize]),
                Box(BoxSize * 2, BoxSize).Select(pos => board[pos.X + DefaultSize * 2 - BoxSize, pos.Y + DefaultSize])
            }.SelectMany(t => t);

            foreach (SudokuTile tile in tiles) tile.Block();

            // Select the tiles in the 3 x 3 area (area.X, area.Y) and create rules for them
            foreach (Point area in Box(SamuraiAreas, SamuraiAreas))
            {
                IEnumerable<SudokuTile> tilesInArea = Box(BoxSize, BoxSize)
                    .Select(pos => board[area.X * BoxSize + pos.X, area.Y * BoxSize + pos.Y]);
                if (tilesInArea.First().IsBlocked)
                    continue;
                board.CreateRule($"Area {area.X}, {area.Y}", tilesInArea);
            }

            // Select all rows and create columns for them
            foreach (int posSet in Enumerable.Range(0, board.Width))
            {
                board.CreateRule($"Column Upper {posSet}", Box(1, DefaultSize).Select(pos => board[posSet, pos.Y]));
                board.CreateRule($"Column Lower {posSet}", Box(1, DefaultSize).Select(pos => board[posSet, pos.Y + DefaultSize + BoxSize]));

                board.CreateRule($"Row Left {posSet}", Box(DefaultSize, 1).Select(pos => board[pos.X, posSet]));
                board.CreateRule($"Row Right {posSet}", Box(DefaultSize, 1).Select(pos => board[pos.X + DefaultSize + BoxSize, posSet]));

                if (posSet >= BoxSize * 2 && posSet < BoxSize * 2 + DefaultSize)
                {
                    // Create rules for the middle sudoku
                    board.CreateRule($"Column Middle {posSet}", Box(1, 9).Select(pos => board[posSet, pos.Y + BoxSize * 2]));
                    board.CreateRule($"Row Middle {posSet}", Box(9, 1).Select(pos => board[pos.X + BoxSize * 2, posSet]));
                }
            }
            return board;
        }

        public static SudokuBoard SizeAndBoxes(int width, int height, int boxCountX, int boxCountY, string[] tileDefinitions)
        {
            SudokuBoard board = new SudokuBoard(width, height, tileDefinitions);
            board.AddBoxesCount(boxCountX, boxCountY);
            return board;
        }

        public static SudokuBoard ClassicWith3x3Boxes(string[] tileDefinitions) => SizeAndBoxes(DefaultSize, DefaultSize, DefaultSize / BoxSize, DefaultSize / BoxSize, tileDefinitions);

        public static SudokuBoard ClassicWith3x3BoxesAndHyperRegions(string[] tileDefinitions)
        {
            SudokuBoard board = ClassicWith3x3Boxes(tileDefinitions);
            const int HyperSecond = HyperMargin + BoxSize + HyperMargin;
            // Create the four extra hyper regions
            board.CreateRule("HyperA", Box(3, 3).Select(pos => board[pos.X + HyperMargin, pos.Y + HyperMargin]));
            board.CreateRule("HyperB", Box(3, 3).Select(pos => board[pos.X + HyperSecond, pos.Y + HyperMargin]));
            board.CreateRule("HyperC", Box(3, 3).Select(pos => board[pos.X + HyperMargin, pos.Y + HyperSecond]));
            board.CreateRule("HyperD", Box(3, 3).Select(pos => board[pos.X + HyperSecond, pos.Y + HyperSecond]));
            return board;
        }

        public static SudokuBoard ClassicWithSpecialBoxes(string[] areas, string[] tileDefinitions)
        {
            int sizeX = areas[0].Length;
            int sizeY = areas.Length;
            SudokuBoard board = new SudokuBoard(sizeX, sizeY, tileDefinitions);
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
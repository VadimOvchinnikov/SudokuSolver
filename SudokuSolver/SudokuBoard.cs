using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuBoard
    {
        private readonly List<SudokuRule> rules = new List<SudokuRule>();
        private readonly SudokuTile[,] tiles;
        private readonly int _maxValue;

        public SudokuBoard(SudokuBoard copy)
        {
            _maxValue = copy._maxValue;
            tiles = new SudokuTile[copy.Width, copy.Height];
            CreateTiles();
            // Copy the tile values
            foreach (Point pos in SudokuFactory.Box(Width, Height))
            {
                tiles[pos.X, pos.Y] = new SudokuTile(pos.X, pos.Y, _maxValue)
                {
                    Value = copy[pos.X, pos.Y].Value
                };
            }

            // Copy the rules
            rules = copy.rules
                .Select(rule => new SudokuRule(rule.Select(tile => tiles[tile.X, tile.Y]), rule.Description))
                .ToList();
        }

        public SudokuBoard(int width, int height, int maxValue, string[] tileDefinitions)
        {
            _maxValue = maxValue;
            tiles = new SudokuTile[width, height];
            CreateTiles();
            if (_maxValue == width || _maxValue == height) // If maxValue is not width or height, then adding line rules would be stupid
                SetupLineRules();

            Populate(tileDefinitions);
        }

        public SudokuBoard(int width, int height, string[] tileDefinitions) : this(width, height, Math.Max(width, height), tileDefinitions)
        {
        }

        private void CreateTiles()
        {
            foreach (Point pos in SudokuFactory.Box(tiles.GetLength(0), tiles.GetLength(1)))
            {
                tiles[pos.X, pos.Y] = new SudokuTile(pos.X, pos.Y, _maxValue);
            }
        }

        private void SetupLineRules()
        {
            // Create rules for rows and columns
            for (int x = 0; x < Width; x++)
            {
                IEnumerable<SudokuTile> row = Enumerable.Range(0, tiles.GetLength(1)).Select(i => tiles[x, i]);
                rules.Add(new SudokuRule(row, $"Row {x}"));
            }
            for (int y = 0; y < Height; y++)
            {
                IEnumerable<SudokuTile> col = Enumerable.Range(0, tiles.GetLength(0)).Select(i => tiles[i, y]);
                rules.Add(new SudokuRule(col, $"Col {y}"));
            }
        }

        internal IEnumerable<SudokuTile> TileBox(int startX, int startY, int sizeX, int sizeY) =>
            SudokuFactory.Box(sizeX, sizeY).Select(pos => tiles[startX + pos.X, startY + pos.Y]);

        public SudokuTile this[int x, int y] => tiles[x, y];

        public int Width => tiles.GetLength(0);

        public int Height => tiles.GetLength(1);

        internal void CreateRule(string description, IEnumerable<SudokuTile> tiles) => rules.Add(new SudokuRule(tiles, description));

        public bool CheckValid() => rules.All(rule => rule.CheckValid());

        public string[] TileDefinitions => tiles
            .Cast<SudokuTile>()
            .OrderBy(t => t.X)
            .ThenBy(t => t.Y)
            .GroupBy(t => t.Y)
            .Select(g => string.Join(string.Empty, g.Select(t => t.Value)))
            .ToArray();

        public IEnumerable<SudokuBoard> Solve()
        {
            // reset solution
            foreach (SudokuTile tile in tiles)
                tile.ResetPossibles();

            SudokuProgress simplify = SudokuProgress.PROGRESS;
            while (simplify == SudokuProgress.PROGRESS) simplify = Simplify();

            if (simplify == SudokuProgress.FAILED)
                yield break;

            // Find one of the values with the least number of alternatives, but that still has at least 2 alternatives
            IEnumerable<SudokuTile> query = from rule in rules
                                            from tile in rule
                                            where tile.PossibleCount > 1
                                            orderby tile.PossibleCount ascending
                                            select tile;

            SudokuTile chosen = query.FirstOrDefault();
            if (chosen == null)
            {
                // The board has been completed, we're done!
                yield return this;
                yield break;
            }

            foreach (int value in Enumerable.Range(1, _maxValue))
            {
                // Iterate through all the valid possibles on the chosen square and pick a number for it
                if (!chosen.IsValuePossible(value))
                    continue;
                SudokuBoard copy = new SudokuBoard(this);
                copy[chosen.X, chosen.Y].Fix(value, "Trial and error");
                foreach (SudokuBoard innerSolution in copy.Solve())
                    yield return innerSolution;
            }
            yield break;
        }

        private void Populate(string[] tileDefinitions)
        {
            int rowIndex = 0;

            foreach (string s in tileDefinitions)
            {
                // Method for initializing a board from string
                for (int i = 0; i < s.Length; i++)
                {
                    SudokuTile tile = tiles[i, rowIndex];
                    if (s[i] == '/')
                    {
                        tile.Block();
                        continue;
                    }
                    int value = s[i] == '.' ? SudokuTile.CLEARED : (int)char.GetNumericValue(s[i]);
                    tile.Value = value;
                }

                rowIndex++;
            }
        }

        internal SudokuProgress Simplify()
        {
            bool valid = CheckValid();
            if (!valid)
                return SudokuProgress.FAILED;

            return rules.Aggregate(SudokuProgress.NO_PROGRESS,
                (progress, rule) => SudokuTile.CombineSolvedState(progress, rule.Solve()));
        }

        internal void AddBoxesCount(int boxesX, int boxesY)
        {
            int sizeX = Width / boxesX;
            int sizeY = Height / boxesY;

            IEnumerable<Point> boxes = SudokuFactory.Box(sizeX, sizeY);
            foreach (Point pos in boxes)
            {
                IEnumerable<SudokuTile> boxTiles = TileBox(pos.X * sizeX, pos.Y * sizeY, sizeX, sizeY);
                CreateRule($"Box at ({pos.X}, {pos.Y})", boxTiles);
            }
        }
    }
}
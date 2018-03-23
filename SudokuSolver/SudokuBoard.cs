using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuBoard
    {
        private readonly List<SudokuRule> _rules = new List<SudokuRule>();
        private readonly SudokuTile[,] _tiles;
        private readonly int _maxValue;

        public SudokuBoard(SudokuBoard copy)
        {
            _maxValue = copy._maxValue;
            _tiles = new SudokuTile[copy.Width, copy.Height];
            CreateTiles();
            // Copy the tile values
            foreach (Point pos in SudokuFactory.Box(Width, Height))
            {
                _tiles[pos.X, pos.Y] = new SudokuTile(pos.X, pos.Y, _maxValue)
                {
                    Value = copy[pos.X, pos.Y].Value
                };
            }

            // Copy the rules
            _rules = copy._rules
                .Select(rule => new SudokuRule(rule.Select(tile => _tiles[tile.X, tile.Y]), rule.Description))
                .ToList();
        }

        public SudokuBoard(int width, int height, int maxValue, string[] tileDefinitions)
        {
            _maxValue = maxValue;
            _tiles = new SudokuTile[width, height];
            CreateTiles();
            if (_maxValue == width || _maxValue == height) // If maxValue is not width or height, then adding line rules would be stupid
            {
                // Create rules for rows and columns
                for (int x = 0; x < Width; x++)
                    _rules.Add(new SudokuRule(Enumerable.Range(0, _tiles.GetLength(1)).Select(i => _tiles[x, i]), $"Row {x}"));

                for (int y = 0; y < Height; y++)
                    _rules.Add(new SudokuRule(Enumerable.Range(0, _tiles.GetLength(0)).Select(i => _tiles[i, y]), $"Col {y}"));
            }

            int rowIndex = 0;

            foreach (string s in tileDefinitions)
            {
                // Method for initializing a board from string
                for (int i = 0; i < s.Length; i++)
                {
                    SudokuTile tile = _tiles[i, rowIndex];
                    if (s[i] == '/')
                    {
                        tile.Block();
                        continue;
                    }
                    tile.Value = s[i] == '.' ? SudokuTile.CLEARED : (int)char.GetNumericValue(s[i]);
                }

                rowIndex++;
            }
        }

        public SudokuBoard(int width, int height, string[] tileDefinitions) : this(width, height, Math.Max(width, height), tileDefinitions)
        {
        }

        private void CreateTiles()
        {
            foreach (Point pos in SudokuFactory.Box(_tiles.GetLength(0), _tiles.GetLength(1)))
            {
                _tiles[pos.X, pos.Y] = new SudokuTile(pos.X, pos.Y, _maxValue);
            }
        }

        public SudokuTile this[int x, int y] => _tiles[x, y];

        public int Width => _tiles.GetLength(0);

        public int Height => _tiles.GetLength(1);

        internal void CreateRule(string description, IEnumerable<SudokuTile> tiles) => _rules.Add(new SudokuRule(tiles, description));

        public string[] TileDefinitions => _tiles
            .Cast<SudokuTile>()
            .OrderBy(t => t.X)
            .ThenBy(t => t.Y)
            .GroupBy(t => t.Y)
            .Select(g => string.Join(string.Empty, g.Select(t => t.Value)))
            .ToArray();

        public IEnumerable<SudokuBoard> Solve()
        {
            SudokuProgress Simplify()
            {
                bool valid = _rules.All(rule => rule.CheckValid());
                if (!valid)
                    return SudokuProgress.FAILED;

                return _rules.Aggregate(SudokuProgress.NO_PROGRESS,
                    (progress, rule) => SudokuTile.CombineSolvedState(progress, rule.Solve()));
            }

            // reset solution
            foreach (SudokuTile tile in _tiles)
                tile.ResetPossibles();

            SudokuProgress simplify = SudokuProgress.PROGRESS;
            while (simplify == SudokuProgress.PROGRESS) simplify = Simplify();

            if (simplify == SudokuProgress.FAILED)
                yield break;

            // Find one of the values with the least number of alternatives, but that still has at least 2 alternatives
            IEnumerable<SudokuTile> query = from rule in _rules
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

        internal void AddBoxesCount(int boxesX, int boxesY)
        {
            int sizeX = Width / boxesX;
            int sizeY = Height / boxesY;

            IEnumerable<SudokuTile> TileBox(int startX, int startY) =>
                SudokuFactory.Box(sizeX, sizeY).Select(pos => _tiles[startX + pos.X, startY + pos.Y]);

            IEnumerable<Point> boxes = SudokuFactory.Box(sizeX, sizeY);
            foreach (Point pos in boxes)
                CreateRule($"Box at ({pos.X}, {pos.Y})", TileBox(pos.X * sizeX, pos.Y * sizeY));
        }
    }
}
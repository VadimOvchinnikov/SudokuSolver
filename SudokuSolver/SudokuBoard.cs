using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuBoard : IEquatable<SudokuBoard>
    {
        private readonly ISet<SudokuRule> rules = new HashSet<SudokuRule>();
        private readonly SudokuTile[,] tiles;
        private readonly int _maxValue;

        public SudokuBoard(SudokuBoard copy)
        {
            _maxValue = copy._maxValue;
            tiles = new SudokuTile[copy.Width, copy.Height];
            CreateTiles();
            // Copy the tile values
            foreach (var pos in SudokuFactory.Box(Width, Height))
            {
                tiles[pos.X, pos.Y] = new SudokuTile(pos.X, pos.Y, _maxValue)
                {
                    Value = copy.tiles[pos.X, pos.Y].Value
                };
            }

            // Copy the rules
            foreach (SudokuRule rule in copy.rules)
            {
                var ruleTiles = new HashSet<SudokuTile>();
                foreach (SudokuTile tile in rule)
                {
                    ruleTiles.Add(tiles[tile.X, tile.Y]);
                }
                rules.Add(new SudokuRule(ruleTiles, rule.Description));
            }
        }

        public SudokuBoard(int width, int height, int maxValue)
        {
            _maxValue = maxValue;
            tiles = new SudokuTile[width, height];
            CreateTiles();
            if (_maxValue == width || _maxValue == height) // If maxValue is not width or height, then adding line rules would be stupid
                SetupLineRules();
        }

        public SudokuBoard(int width, int height) : this(width, height, Math.Max(width, height))
        {
        }

        private void CreateTiles()
        {
            foreach (var pos in SudokuFactory.Box(tiles.GetLength(0), tiles.GetLength(1)))
            {
                tiles[pos.X, pos.Y] = new SudokuTile(pos.X, pos.Y, _maxValue);
            }
        }

        private void SetupLineRules()
        {
            // Create rules for rows and columns
            for (int x = 0; x < Width; x++)
            {
                IEnumerable<SudokuTile> row = Enumerable.Range(0, tiles.GetLength(1)).Select(i => tiles[i, x]);
                rules.Add(new SudokuRule(row, $"Row {x}"));
            }
            for (int y = 0; y < Height; y++)
            {
                IEnumerable<SudokuTile> col = Enumerable.Range(0, tiles.GetLength(0)).Select(i => tiles[y, i]);
                rules.Add(new SudokuRule(col, $"Col {y}"));
            }
        }

        internal IEnumerable<SudokuTile> TileBox(int startX, int startY, int sizeX, int sizeY) =>
            from pos in SudokuFactory.Box(sizeX, sizeY) select tiles[startX + pos.X, startY + pos.Y];


        public int Width => tiles.GetLength(0);

        public int Height => tiles.GetLength(1);

        public void CreateRule(string description, params SudokuTile[] tiles) => rules.Add(new SudokuRule(tiles, description));

        public void CreateRule(string description, IEnumerable<SudokuTile> tiles) => rules.Add(new SudokuRule(tiles, description));

        public bool CheckValid() => rules.All(rule => rule.CheckValid());

        public IEnumerable<SudokuBoard> Solve()
        {
            ResetSolutions();
            SudokuProgress simplify = SudokuProgress.PROGRESS;
            while (simplify == SudokuProgress.PROGRESS) simplify = Simplify();

            if (simplify == SudokuProgress.FAILED)
                yield break;

            // Find one of the values with the least number of alternatives, but that still has at least 2 alternatives
            var query = from rule in rules
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

            Console.WriteLine($"SudokuTile: {chosen}");

            foreach (var value in Enumerable.Range(1, _maxValue))
            {
                // Iterate through all the valid possibles on the chosen square and pick a number for it
                if (!chosen.IsValuePossible(value))
                    continue;
                var copy = new SudokuBoard(this);
                copy[chosen.X, chosen.Y].Fix(value, "Trial and error");
                foreach (SudokuBoard innerSolution in copy.Solve())
                    yield return innerSolution;
            }
            yield break;
        }

        public void Output()
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    Console.Write(tiles[x, y].ToStringSimple());
                }
                Console.WriteLine();
            }
        }

        public SudokuTile this[int x, int y] => tiles[x, y];

        private int _rowAddIndex;

        public void AddRow(string s)
        {
            // Method for initializing a board from string
            for (int i = 0; i < s.Length; i++)
            {
                SudokuTile tile = tiles[i, _rowAddIndex];
                if (s[i] == '/')
                {
                    tile.Block();
                    continue;
                }
                int value = s[i] == '.' ? SudokuTile.CLEARED : (int)char.GetNumericValue(s[i]);
                tile.Value = value;
            }
            _rowAddIndex++;
        }

        internal void ResetSolutions()
        {
            foreach (SudokuTile tile in tiles)
                tile.ResetPossibles();
        }

        internal SudokuProgress Simplify()
        {
            SudokuProgress result = SudokuProgress.NO_PROGRESS;
            bool valid = CheckValid();
            if (!valid)
                return SudokuProgress.FAILED;

            foreach (SudokuRule rule in rules)
                result = SudokuTile.CombineSolvedState(result, rule.Solve());

            return result;
        }

        internal void AddBoxesCount(int boxesX, int boxesY)
        {
            int sizeX = Width / boxesX;
            int sizeY = Height / boxesY;

            var boxes = SudokuFactory.Box(sizeX, sizeY);
            foreach (var pos in boxes)
            {
                IEnumerable<SudokuTile> boxTiles = TileBox(pos.X * sizeX, pos.Y * sizeY, sizeX, sizeY);
                CreateRule($"Box at ({pos.X}, {pos.Y})", boxTiles);
            }
        }

        public void OutputRules()
        {
            foreach (var rule in rules)
            {
                Console.WriteLine($"{String.Join(",", rule)} - {rule}");
            }
        }

        public bool Equals(SudokuBoard other)
        {
            if (other == null) return false;
            return tiles.Cast<SudokuTile>().SequenceEqual(other.tiles.Cast<SudokuTile>());
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as SudokuTile);
        }
    }
}
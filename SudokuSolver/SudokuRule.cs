using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuRule : IEnumerable<SudokuTile>
    {
        private readonly IEnumerable<SudokuTile> _tiles;
        private readonly string _description;

        internal SudokuRule(IEnumerable<SudokuTile> tiles, string description)
        {
            _tiles = tiles.ToArray();
            _description = description;
        }

        public bool CheckValid()
        {
            IEnumerable<SudokuTile> filtered = _tiles.Where(tile => tile.HasValue);
            IEnumerable<IGrouping<int, SudokuTile>> groupedByValue = filtered.GroupBy(tile => tile.Value);
            return groupedByValue.All(group => group.Count() == 1);
        }

        internal SudokuProgress RemovePossibles()
        {
            // Tiles that has a number already
            IEnumerable<SudokuTile> withNumber = _tiles.Where(tile => tile.HasValue);

            // Tiles without a number
            IEnumerable<SudokuTile> withoutNumber = _tiles.Where(tile => !tile.HasValue);

            // The existing numbers in this rule
            IEnumerable<int> existingNumbers = withNumber.Select(tile => tile.Value).Distinct();

            return withoutNumber.Aggregate(
                SudokuProgress.NO_PROGRESS,
                (result, tile) => SudokuTile.CombineSolvedState(result, tile.RemovePossibles(existingNumbers)));
        }

        internal SudokuProgress CheckForOnlyOnePossibility()
        {
            // Check if there is only one number within this rule that can have a specific value
            IEnumerable<int> existingNumbers = _tiles.Select(tile => tile.Value).Distinct();
            SudokuProgress result = SudokuProgress.NO_PROGRESS;

            foreach (int value in Enumerable.Range(1, _tiles.Count()))
            {
                if (existingNumbers.Contains(value)) // this rule already has the value, skip checking for it
                    continue;
                List<SudokuTile> possibles = _tiles.Where(tile => !tile.HasValue && tile.IsValuePossible(value)).ToList();
                if (possibles.Count == 0)
                    return SudokuProgress.FAILED;
                if (possibles.Count == 1)
                {
                    possibles.First().Fix(value, $"Only possible in rule {ToString()}");
                    result = SudokuProgress.PROGRESS;
                }
            }
            return result;
        }

        internal SudokuProgress Solve()
        {
            // If both are null, return null (indicating no change). If one is null, return the other. Else return result1 && result2
            SudokuProgress result1 = RemovePossibles();
            SudokuProgress result2 = CheckForOnlyOnePossibility();
            return SudokuTile.CombineSolvedState(result1, result2);
        }

        public override string ToString() => _description;

        public IEnumerator<SudokuTile> GetEnumerator() => _tiles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string Description => _description;
    }
}
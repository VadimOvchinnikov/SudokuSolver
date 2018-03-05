using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuTile : IEquatable<SudokuTile>
    {
        internal static SudokuProgress CombineSolvedState(SudokuProgress a, SudokuProgress b)
        {
            switch (a)
            {
                case SudokuProgress.FAILED:
                    return a;

                case SudokuProgress.NO_PROGRESS:
                    return b;

                case SudokuProgress.PROGRESS:
                    return b == SudokuProgress.FAILED ? b : a;
            }
            throw new InvalidOperationException($"Invalid value for {nameof(a)}");
        }

        public const int CLEARED = 0;
        private int _maxValue;
        private int _value;
        private int _x;
        private int _y;
        private ISet<int> possibleValues;
        private bool _blocked;

        public SudokuTile(int x, int y, int maxValue)
        {
            _x = x;
            _y = y;
            _blocked = false;
            _maxValue = maxValue;
            possibleValues = new HashSet<int>();
            _value = 0;
        }

        public int Value
        {
            get => _value;
            set
            {
                if (value > _maxValue)
                    throw new ArgumentOutOfRangeException($"SudokuTile Value cannot be greater than {_maxValue}. Was {value}");
                if (value < CLEARED)
                    throw new ArgumentOutOfRangeException($"SudokuTile Value cannot be smaller than zero. Was {value}");
                _value = value;
            }
        }

        public bool HasValue => Value != CLEARED;

        public string ToStringSimple() => Value.ToString();

        public override string ToString() => $"Value {Value} at pos {_x}, {_y}. ";

        internal void ResetPossibles()
        {
            possibleValues.Clear();
            foreach (int i in Enumerable.Range(1, _maxValue))
            {
                if (!HasValue || Value == i)
                    possibleValues.Add(i);
            }
        }

        public void Block() => _blocked = true;

        internal void Fix(int value, string reason)
        {
            Console.WriteLine($"Fixing {value} on pos {_x}, {_y}: {reason}");
            Value = value;
            ResetPossibles();
        }

        internal SudokuProgress RemovePossibles(IEnumerable<int> existingNumbers)
        {
            if (_blocked)
                return SudokuProgress.NO_PROGRESS;
            // Takes the current possible values and removes the ones existing in `existingNumbers`
            possibleValues = new HashSet<int>(possibleValues.Where(x => !existingNumbers.Contains(x)));
            SudokuProgress result = SudokuProgress.NO_PROGRESS;
            if (possibleValues.Count == 1)
            {
                Fix(possibleValues.First(), "Only one possibility");
                result = SudokuProgress.PROGRESS;
            }
            if (possibleValues.Count == 0)
                return SudokuProgress.FAILED;
            return result;
        }

        public bool IsValuePossible(int i) => possibleValues.Contains(i);

        public bool Equals(SudokuTile other)
        {
            if (other == null) return false;
            return X == other.X
                && Y == other.Y
                && IsBlocked == other.IsBlocked
                && Value == Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as SudokuTile);
        }

        public override int GetHashCode() => Tuple.Create(X, Y, IsBlocked, Value).GetHashCode();

        public int X => _x;
        public int Y => _y;
        public bool IsBlocked => _blocked;  // A blocked field can not contain a value — used for creating 'holes' in the map

        public int PossibleCount => IsBlocked ? 1 : possibleValues.Count;
    }
}
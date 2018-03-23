using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver
{
    public class SudokuTile
    {
        internal const int CLEARED = 0;
        private readonly int _maxValue;
        private readonly int _x;
        private readonly int _y;

        private IEnumerable<int> _possibleValues = Enumerable.Empty<int>();
        private int _value = 0;
        private bool _blocked = false;

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

        public SudokuTile(int x, int y, int maxValue)
        {
            _x = x;
            _y = y;
            _maxValue = maxValue;
        }

        public int Value
        {
            get => _value;
            internal set
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
            if (HasValue)
                _possibleValues = Enumerable.Repeat(Value, 1);
            else
                _possibleValues = Enumerable.Range(1, _maxValue);
        }

        internal void Block() => _blocked = true;

        internal void Fix(int value, string reason)
        {
            Value = value;
            ResetPossibles();
        }

        internal SudokuProgress RemovePossibles(IEnumerable<int> existingNumbers)
        {
            if (_blocked)
                return SudokuProgress.NO_PROGRESS;
            // Takes the current possible values and removes the ones existing in `existingNumbers`
            _possibleValues = _possibleValues.Where(x => !existingNumbers.Contains(x));
            SudokuProgress result = SudokuProgress.NO_PROGRESS;
            if (_possibleValues.Count() == 1)
            {
                Fix(_possibleValues.First(), "Only one possibility");
                result = SudokuProgress.PROGRESS;
            }
            if (_possibleValues.Count() == 0)
                return SudokuProgress.FAILED;
            return result;
        }

        internal bool IsValuePossible(int i) => _possibleValues.Contains(i);

        public int X => _x;
        public int Y => _y;
        public bool IsBlocked => _blocked;  // A blocked field can not contain a value — used for creating 'holes' in the map

        internal int PossibleCount => IsBlocked ? 1 : _possibleValues.Count();
    }
}
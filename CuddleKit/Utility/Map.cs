using System;
using CuddleKit.Internal;

namespace CuddleKit.Utility
{
	public struct Map<TValue> : IDisposable
	{
		private MultiVector<char> _keys;
		private Vector<TValue> _values;

		public void Dispose()
		{
			using var keys = _keys;
			using var values = _values;
			_keys = default;
			_values = default;
		}

		public ref TValue Insert(ReadOnlySpan<char> key)
		{
			if (key.Length == 0)
				throw new InvalidOperationException("Key shouldn't be empty");

			var position = GetLowerBound(key);
			if (position < _keys.RowsCount && _keys[position].SequenceEqual(key))
				return ref _values[position];

			_keys.InsertRow(position, key);
			return ref _values.Insert(position);
		}

		public readonly ref readonly TValue Lookup(ReadOnlySpan<char> annotation, in TValue fallbackValue)
		{
			if (annotation.Length == 0)
				return ref fallbackValue;

			var position = GetLowerBound(annotation);
			return ref position < _keys.RowsCount && _keys[position].SequenceEqual(annotation)
				? ref _values[position]
				: ref fallbackValue;
		}

		private readonly int GetLowerBound(ReadOnlySpan<char> key)
		{
			var annotationsCount = _keys.RowsCount;
			Span<int> bounds = stackalloc int[2] { 0, annotationsCount };

			while (bounds[0] < bounds[1])
			{
				var middle = (bounds[0] + bounds[1]) >> 1;
				var less = _keys[middle].SequenceCompareTo(key) < 0 ? 1 : 0;
				bounds[1 - less] = middle + less;
			}

			return bounds[0];
		}
	}
}

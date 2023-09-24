using System;
using System.Runtime.CompilerServices;

namespace CuddleKit.Internal
{
	internal struct MultiVector<T>
	{
		private Vector<Row> _rows;
		private Vector<T> _data;

		public MultiVector(int capacity)
		{
			_rows = new Vector<Row>(capacity);
			_data = new Vector<T>(4 * capacity);
		}

		public void Dispose()
		{
			_rows.Dispose();
			_data.Dispose();
		}

		public readonly int RowsCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _rows.Length;
		}

		public readonly Span<T> this[int rowIndex]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				ref readonly var row = ref _rows.ReadOnlyBuffer[rowIndex];
				return _data.Buffer.Slice(row.Start, row.Length);
			}
		}

		public void PushRow(int capacity)
		{
			_rows.Push() = new Row { Start = _data.Length };
			_data.PushMany(capacity);
		}

		public void InsertRow(int rowIndex, int capacity)
		{
			var rows = _rows.Buffer;
			if (rowIndex == _rows.Length)
			{
				PushRow(capacity);
				return;
			}

			var dataIndex = rowIndex > 0 ? rows[rowIndex - 1].Start : 0;
			_data.Insert(dataIndex, capacity);

			for (var i = rowIndex; i < rows.Length; ++i)
				rows[i].Start += capacity;

			_rows.Insert(rowIndex) = new Row { Start = dataIndex };
		}

		public void PushRow(ReadOnlySpan<T> rowData)
		{
			var rowIndex = _rows.Length;
			PushRow(rowData.Length);
			rowData.CopyTo(Push(rowIndex, rowData.Length));
		}

		public void InsertRow(int rowIndex, ReadOnlySpan<T> rowData)
		{
			InsertRow(rowIndex, rowData.Length);
			rowData.CopyTo(Push(rowIndex, rowData.Length));
		}

		public Span<T> Push(int rowIndex, int count)
		{
			ExtendRow(rowIndex, count);
			ref readonly var row = ref _rows[rowIndex];
			return _data.Buffer.Slice(row.Start + row.Length - count, count);
		}

		public Span<T> Insert(int rowIndex, int index, int count)
		{
			ExtendRow(rowIndex, count);
			ref readonly var row = ref _rows[rowIndex];

			var range = _data.Slice(row.Start + index, count);
			range.CopyTo(_data.Slice(row.Start + index + count, count));

			return range;
		}

		private void ExtendRow(int rowIndex, int count)
		{
			var rows = _rows.Buffer;
			ref var row = ref rows[rowIndex];

			var capacity = rowIndex + 1 < rows.Length
				? rows[rowIndex + 1].Start - row.Start
				: _data.Length - row.Start;

			if (row.Length + count > capacity)
			{
				var capacityDelta = Math.Max(capacity * 2, row.Length + count) - capacity;
				_data.Insert(row.Start + capacity, capacityDelta);

				for (var i = rowIndex + 1; i < rows.Length; ++i)
					rows[i].Start += capacityDelta;
			}

			row.Length += count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(int rowIndex, ReadOnlySpan<T> data) =>
			data.CopyTo(Push(rowIndex, data.Length));

		public void Clear()
		{
			_rows.Clear();
			_data.Clear();
		}

		private struct Row
		{
			public int Start;
			public int Length;
		}
	}
}

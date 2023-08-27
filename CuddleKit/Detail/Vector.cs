using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CuddleKit.Detail
{
	[DebuggerDisplay("Length = {" + nameof(Length) + "}")]
	[DebuggerTypeProxy(typeof(Vector<>.DebuggerView))]
	internal struct Vector<T> : IDisposable
	{
		private static readonly System.Buffers.ArrayPool<T> Pool = System.Buffers.ArrayPool<T>.Shared;

		private T[] _buffer;
		private int _length;

		public void Dispose()
		{
			if (_buffer == null)
				return;

			Pool.Return(_buffer);
			_buffer = null;
			_length = 0;
		}

		public readonly int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _length;
		}

		public readonly Span<T> Buffer
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(_buffer, 0, _length);
		}

		public readonly ReadOnlySpan<T> ReadOnlyBuffer
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(_buffer, 0, _length);
		}

		public Vector(int capacity) =>
			(_buffer, _length) = (Pool.Rent(Math.Max(4, capacity)), 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> Slice(int start, int count) =>
			new(_buffer, start, count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Span<T> Take(int count) =>
			new(_buffer, 0, count);

		public readonly ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _buffer.AsSpan(0, _length)[index];
		}

		public Span<T> PushMany(int count)
		{
			if (count < 0)
				throw new IndexOutOfRangeException("");

			if (count == 0)
				return default;

			var minLength = _length + count;

			if (_buffer == null)
			{
				var capacity = Math.Max(4, minLength);
				_buffer = Pool.Rent(capacity);
			}
			else if (minLength > _buffer.Length)
			{
				var capacity = Math.Max(4, Math.Max(_length * 2, minLength));
				var buffer = Pool.Rent(capacity);
				_buffer.AsSpan(0, _length).CopyTo(buffer);
				Pool.Return(_buffer);
				_buffer = buffer;
			}

			_length += count;
			return _buffer.AsSpan(_length - count, count);
		}

		public Span<T> Insert(int index, int count)
		{
			if (index < 0 || index > _length)
				throw new IndexOutOfRangeException("");

			if (index == _length)
				return PushMany(count);

			if (count < 0)
				throw new IndexOutOfRangeException("");

			if (count == 0)
				return default;

			var minLength = _length + count;

			if (minLength > _buffer.Length)
			{
				var capacity = Math.Max(4, Math.Max(_length * 2, minLength));
				var buffer = Pool.Rent(capacity);
				_buffer.AsSpan(0, index).CopyTo(buffer);
				_buffer.AsSpan(index, _length - index).CopyTo(buffer.AsSpan(index + count));
				Pool.Return(_buffer);
				_buffer = buffer;
			}
			else
			{
				_buffer
					.AsSpan(index, _length - index)
					.CopyTo(_buffer.AsSpan(index + count));
			}

			_length += count;
			return _buffer.AsSpan(index, count);
		}

		public void Erase(int index, int count)
		{
			if (index < 0 || count < 0 || index + count > _length)
				throw new IndexOutOfRangeException("");

			for (var i = index; i + count < _length; ++i)
				_buffer[i] = _buffer[i + count];

			_length -= count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Push() =>
			ref PushMany(1)[0];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Insert(int index) =>
			ref Insert(index, 1)[0];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(ReadOnlySpan<T> data) =>
			data.CopyTo(PushMany(data.Length));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset(ReadOnlySpan<T> data)
		{
			_length = 0;
			data.CopyTo(PushMany(data.Length));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear() =>
			_length = 0;

		private readonly ref struct DebuggerView
		{
			private readonly Vector<T> _vector;

			public DebuggerView(Vector<T> vector) =>
				_vector = vector;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public ReadOnlySpan<T> Elements =>
				_vector.ReadOnlyBuffer;
		}
	}
}

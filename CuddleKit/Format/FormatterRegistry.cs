using System;
using System.Collections.Generic;
using CuddleKit.Detail;

namespace CuddleKit.Format
{
	public sealed class FormatterRegistry : IDisposable
	{
		public static readonly IReadOnlyList<IFormatter> DefaultFormatters = new IFormatter[]
		{
			Int8Formatter.Default,
			Int16Formatter.Default,
			Int32Formatter.Default,
			Int64Formatter.Default,
			Uint8Formatter.Default,
			Uint16Formatter.Default,
			Uint32Formatter.Default,
			Uint64Formatter.Default,
			SingleFormatter.Default,
			DoubleFormatter.Default,
			StringFormatter.Default,
			GuidFormatter.Default,
			// TODO: add more, DateTime, Url, Base64, Regex etc.
		};

		public static readonly FormatterRegistry Default = new(DefaultFormatters);

		private Registry<Type> _systemTypeRegistry;
		private Registry<ValueType> _documentTypeRegistry;

		public FormatterRegistry(IReadOnlyList<IFormatter> formatters)
		{
			_systemTypeRegistry = new Registry<Type>(FormatterFlags.FallbackSystemType, formatters.Count);
			_documentTypeRegistry = new Registry<ValueType>(FormatterFlags.FallbackDocumentType, formatters.Count);

			for (int i = 0, length = formatters.Count; i < length; ++i)
			{
				var formatter = formatters[i];
				_systemTypeRegistry.Add(formatter.Specification.SystemType, formatter);
				_documentTypeRegistry.Add(formatter.Specification.DocumentType, formatter);
			}
		}

		void IDisposable.Dispose()
		{
			_systemTypeRegistry.Dispose();
			_documentTypeRegistry.Dispose();
		}

		public IFormatter Lookup(Type type, ReadOnlySpan<char> annotation) =>
			_systemTypeRegistry.Lookup(type, annotation);

		public IFormatter Lookup(ValueType valueType, ReadOnlySpan<char> annotation) =>
			_documentTypeRegistry.Lookup(valueType, annotation);

		private struct Registry<TKey>
		{
			private readonly FormatterFlags _fallbackFlag;
			private readonly Dictionary<TKey, int> _map;

			private Vector<FormattersMap> _formatters;

			public Registry(FormatterFlags fallbackFlag, int capacity)
			{
				_fallbackFlag = fallbackFlag;
				_map = new Dictionary<TKey, int>(capacity);
				_formatters = new Vector<FormattersMap>(capacity);
			}

			public void Dispose() =>
				_formatters.Dispose();

			public void Add(TKey key, IFormatter formatter)
			{
				if (!_map.TryGetValue(key, out var index))
				{
					index = _formatters.Length;
					_formatters.Push() = new FormattersMap();
					_map.Add(key, index);
				}

				_formatters[index].Insert(formatter, formatter.Specification.HasFlag(_fallbackFlag));
			}

			public readonly IFormatter Lookup(TKey key, ReadOnlySpan<char> annotation) =>
				_map.TryGetValue(key, out var index)
					? _formatters[index].Lookup(annotation)
					: null;
		}

		private struct FormattersMap
		{
			private MultiVector<char> _annotations;
			private Vector<IFormatter> _formatters;
			private IFormatter _fallbackFormatter;

			public void Insert(IFormatter formatter, bool fallback)
			{
				if (fallback)
					_fallbackFormatter = formatter;

				var annotation = formatter.Specification.Annotation;
				if (annotation.Length == 0)
					return;

				var position = GetLowerBound(annotation);
				if (position < _annotations.RowsCount && _annotations[position].SequenceEqual(annotation))
				{
					_formatters[position] = formatter;
				}
				else
				{
					_annotations.InsertRow(position, annotation);
					_formatters.Insert(position) = formatter;
				}
			}

			public readonly IFormatter Lookup(ReadOnlySpan<char> annotation)
			{
				var position = GetLowerBound(annotation);
				return position < _annotations.RowsCount && _annotations[position].SequenceEqual(annotation)
					? _formatters[position]
					: _fallbackFormatter;
			}

			private readonly int GetLowerBound(ReadOnlySpan<char> annotation)
			{
				var annotationsCount = _annotations.RowsCount;
				Span<int> bounds = stackalloc int[2] { 0, annotationsCount };

				while (bounds[0] < bounds[1])
				{
					var middle = (bounds[0] + bounds[1]) >> 1;
					var less = _annotations[middle].SequenceCompareTo(annotation) < 0 ? 1 : 0;
					bounds[1 - less] = middle + less;
				}

				return bounds[0];
			}
		}
	}
}

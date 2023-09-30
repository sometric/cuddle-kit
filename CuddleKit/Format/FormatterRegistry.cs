using System;
using System.Collections.Generic;
using CuddleKit.Serialization;
using CuddleKit.Utility;

namespace CuddleKit.Format
{
	using Internal;

	public struct FormatterRegistry : IDisposable
	{
		public static readonly IReadOnlyList<IFormatter> DefaultFormatters = new IFormatter[]
		{
			BooleanFormatter.Default,
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
		private Registry<DataType> _documentTypeRegistry;

		public FormatterRegistry(IReadOnlyList<IFormatter> formatters)
		{
			_systemTypeRegistry = new Registry<Type>(FormatterFlags.FallbackSystemType, formatters.Count);
			_documentTypeRegistry = new Registry<DataType>(FormatterFlags.FallbackDocumentType, formatters.Count);

			for (int i = 0, length = formatters.Count; i < length; ++i)
			{
				var formatter = formatters[i];
				_systemTypeRegistry.Add(formatter.Specification.SystemType, formatter);
				_documentTypeRegistry.Add(formatter.Specification.DataType, formatter);
			}
		}

		public void Dispose()
		{
			using var systemTypeRegistry = _systemTypeRegistry;
			using var documentTypeRegistry = _documentTypeRegistry;
			_systemTypeRegistry = default;
			_documentTypeRegistry = default;
		}

		public readonly IFormatter Lookup(Type type, ReadOnlySpan<char> annotation) =>
			_systemTypeRegistry.Lookup(type, annotation);

		public readonly IFormatter Lookup(DataType dataType, ReadOnlySpan<char> annotation) =>
			_documentTypeRegistry.Lookup(dataType, annotation);

		private struct Registry<TKey> : IDisposable
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

			public void Dispose()
			{
				using var formatters = _formatters;
				_formatters = default;

				// todo: do it as much safe as possible
				for (int i = 0, length = formatters.Length; i < length; ++i)
					formatters[i].Dispose();
			}

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

		private struct FormattersMap : IDisposable
		{
			private Map<IFormatter> _formattersMap;
			private IFormatter _fallbackFormatter;

			public void Dispose()
			{
				using var map = _formattersMap;
				_formattersMap = default;
			}

			public void Insert(IFormatter formatter, bool fallback)
			{
				if (fallback)
					_fallbackFormatter = formatter;

				var annotation = formatter.Specification.Annotation;
				if (annotation.Length == 0)
					return;

				_formattersMap.Insert(annotation) = formatter;
			}

			public readonly IFormatter Lookup(ReadOnlySpan<char> annotation) =>
				_formattersMap.Lookup(annotation, _fallbackFormatter);
		}
	}
}

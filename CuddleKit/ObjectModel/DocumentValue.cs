using System;
using System.Collections.Generic;
using System.Diagnostics;
using CuddleKit.Detail;

namespace CuddleKit.ObjectModel
{
	/// <summary>
	/// Represents a value associated with a <see cref="DocumentObjectModel"/>.
	/// </summary>
	[DebuggerDisplay("{IsValid ? BoxedValue : string.Empty}", Type = "{IsValid ? Type : typeof(void)}")]
	[DebuggerTypeProxy(typeof(DebuggerView))]
	public readonly struct DocumentValue : IEquatable<DocumentValue>
	{
		private readonly DocumentObjectModel _parent;
		private readonly int _index;
		private readonly int _version;

		internal DocumentValue(DocumentObjectModel parent, SafeIndex index) =>
			(_parent, _index, _version) = (parent, index, parent.Values[index].Version);

		/// <summary>
		/// Gets a value indicating whether the current value is valid.
		/// </summary>
		public bool IsValid
		{
			get
			{
				var values = _parent != null ? _parent.Values : default;
				return 
					(_index >= 0) & (_index < values.Length) &&
					values[_index].Version == _version;
			}
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type Type =>
			GetData().TypeInfo.Type;

		/// <summary>
		/// Gets boxed representation of the value.
		/// </summary>
		public object BoxedValue
		{
			get
			{
				Visit(new BoxValueVisitor(), out object boxedValue);
				return boxedValue;
			}
		}

		/// <summary>
		/// Gets or sets the annotation of the value.
		/// </summary>
		public ReadOnlySpan<char> Annotation 
		{
			get => _parent.GetLiteral(GetData().Annotation);
			set => GetData().Annotation = _parent.AddLiteral(value);
		}

		/// <summary>
		/// Gets the value casted to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to cast the value to.</typeparam>
		/// <returns>The value casted to the specified type.</returns>
		/// <exception cref="InvalidCastException">Thrown if the value is not of the specified type.</exception>
		public T GetValue<T>() =>
			GetData().GetValue<T>(_parent);

		public void SetValue<T>(T value)
		{
			ref var valueData = ref GetData();

			if (valueData.TypeInfo.Type == typeof(T))
			{
				valueData.UpdateValue(_parent, value);
			}
			else
			{
				var annotation = valueData.Annotation;
				valueData = DocumentObjectModel.ValueData.AddValue(_parent, value);
				valueData.Annotation = annotation;
			}
		}

		/// <summary>
		/// Checks if the value is equal to the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value to compare.</typeparam>
		/// <param name="value">The value to compare.</param>
		/// <returns><c>true</c> if the value is equal to the specified value; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// This method compares the value to the specified value using the default equality comparison for the value type.
		/// </remarks>
		public bool ValueEquals<T>(T value)
			where T : struct, IEquatable<T>
		{
			ref readonly var valueData = ref GetData();
			return
				typeof(T) == valueData.TypeInfo.Type &&
				value.Equals(valueData.GetValue<T>(_parent));
		}

		/// <summary>
		/// Checks if the value is equal to the specified value using a custom equality comparer.
		/// </summary>
		/// <typeparam name="T">The type of the value to compare.</typeparam>
		/// <param name="value">The value to compare.</param>
		/// <param name="comparer">The equality comparer to use. If <c>null</c>, the default equality comparer for the type is used.</param>
		/// <returns><c>true</c> if the value is equal to the specified value; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// This method compares the value to the specified value using the specified equality comparer.
		/// If no custom comparer is provided, the default equality comparer for the value type is used.
		/// </remarks>
		public bool Equals<T>(T value, IEqualityComparer<T> comparer = null)
		{
			ref readonly var valueData = ref GetData();
			return
				typeof(T) == valueData.TypeInfo.Type &&
				(comparer ?? EqualityComparer<T>.Default)
				.Equals(value, valueData.GetValue<T>(_parent));
		}

		/// <summary>
		/// Determines whether the current value is equal to the specified value.
		/// </summary>
		/// <param name="otherValue">The value to compare.</param>
		/// <returns><c>true</c> if the current value is equal to the specified value; otherwise, <c>false</c>.</returns>
		public bool Equals(DocumentValue otherValue)
		{
			Visit(new EqualityComparisonVisitor { OtherValue = otherValue }, out bool equals);
			return equals;
		}

		/// <summary>
		/// Visits the current value using the specified visitor.
		/// </summary>
		/// <typeparam name="TVisitor">The type of the visitor.</typeparam>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="visitor">The visitor to use.</param>
		/// <param name="result">When this method returns, contains the result of the visitor.</param>
		public void Visit<TVisitor, TResult>(TVisitor visitor, out TResult result)
			where TVisitor : struct, IDocumentValueVisitor<TResult> =>
			GetData().TypeInfo.Visit(this, visitor, out result);

		private ref DocumentObjectModel.ValueData GetData()
		{
			ref var data = ref _parent.Values[_index];

			if (data.Version != _version)
				throw new Exception();

			return ref data;
		}

		private struct EqualityComparisonVisitor : IDocumentValueVisitor<bool>
		{
			public DocumentValue OtherValue;

			public bool Visit<T>(in T value, ReadOnlySpan<char> annotation) =>
				OtherValue.Equals(value);
		}

		private struct BoxValueVisitor : IDocumentValueVisitor<object>
		{
			public object Visit<T>(in T value, ReadOnlySpan<char> annotation) =>
				value;
		}

		private readonly ref struct DebuggerView
		{
			private readonly DocumentValue _value;

			public DebuggerView(DocumentValue value) =>
				_value = value;

			public string Annotation =>
				_value is { IsValid: true, Annotation: { IsEmpty: false } }
					? _value.Annotation.ToString()
					: null;

			public Type Type =>
				_value.IsValid ? _value.Type : null;
		}
	}
}

using System;
using System.Diagnostics;
using CuddleKit.Detail;

namespace CuddleKit.ObjectModel
{
	/// <summary>
	/// Represents a property associated with a <see cref="DocumentObjectModel"/>.
	/// </summary>
	[DebuggerDisplay("{IsValid ? Value : default}", Name = "{IsValid ? Key : string.Empty,nq}")]
	[DebuggerTypeProxy(typeof(DebuggerView))]
	public readonly struct DocumentProperty
	{
		private readonly DocumentObjectModel _parent;
		private readonly SafeIndex _index;
		private readonly int _version;

		internal DocumentProperty(DocumentObjectModel parent, SafeIndex index) =>
			(_parent, _index, _version) = (parent, index, parent.Properties[index].Version);

		/// <summary>
		/// Gets a value indicating whether the current property is valid.
		/// </summary>
		public bool IsValid
		{
			get
			{
				var properties = _parent != null ? _parent.Properties : default;
				return 
					_index.IsValidForRange(properties.Length) &&
					properties[_index].Version == _version;
			}
		}

		/// <summary>
		/// Gets or sets the key of the property.
		/// </summary>
		public ReadOnlySpan<char> Key
		{
			get => _parent.GetLiteral(GetData().Key);
			set => GetData().Key = _parent.AddLiteral(value);
        }

		/// <summary>
		/// Gets the value associated with the property.
		/// </summary>
		public DocumentValue Value =>
			new(_parent, GetData().ValueIndex);

		/// <summary>
		/// Gets or sets the value annotation of the property.
		/// </summary>
		public ReadOnlySpan<char> Annotation
		{
			get => _parent.GetLiteral(_parent.Values[GetData().ValueIndex].Annotation);
			set => _parent.Values[GetData().ValueIndex].Annotation = _parent.AddLiteral(value);
		}

		private ref DocumentObjectModel.PropertyData GetData()
		{
			ref var data = ref _parent.Properties[_index];

			if (data.Version != _version)
				throw new Exception();

			return ref data;
		}

		private readonly ref struct DebuggerView
		{
			private readonly DocumentProperty _property;

			public DebuggerView(DocumentProperty property) =>
				_property = property;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DocumentValue Value =>
				_property.IsValid ? _property.Value : default;
		}
	}
}

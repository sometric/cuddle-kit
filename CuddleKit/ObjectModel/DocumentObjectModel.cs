using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CuddleKit.ObjectModel
{
	using Detail;

	/// <summary>
	/// Represents a document object model that manages a tree data structure.
	/// </summary>
	[DebuggerTypeProxy(typeof(DebuggerView))]
	public sealed class DocumentObjectModel : IDisposable
	{
		private readonly Dictionary<Type, IList> _valuesMap = new();

		private IDocumentObjectModelFactory _factory;
		private Vector<NodeData> _nodes;
		private Vector<ValueData> _values;
		private Vector<PropertyData> _properties;
		private MultiVector<char> _literals;

		/// <summary>
		/// Gets the number of nodes in the document object model.
		/// </summary>
		public int NodesCount =>
			_nodes.Length;

		/// <summary>
		/// Creates a new <see cref="DocumentObjectModel"/> instance with the specified children capacity.
		/// </summary>
		/// <param name="childrenCapacity">The initial capacity for child nodes.</param>
		/// <returns>The created <see cref="DocumentObjectModel"/> instance.</returns>
		public static DocumentObjectModel Create(int childrenCapacity = 4, IDocumentObjectModelFactory factory = null)
		{
			factory ??= ConcurrentDocumentObjectModelFactory.Shared;
			var document = factory.Retain();

			document._factory = factory;
			document._nodes = new Vector<NodeData>(childrenCapacity);
			document._values = new Vector<ValueData>();
			document._properties = new Vector<PropertyData>();
			document._literals = new MultiVector<char>();

			return document;
		}

		/// <summary>
		/// Disposes the <see cref="DocumentObjectModel"/> and returns it to the object pool for reuse.
		/// </summary>
		public void Dispose()
		{
			for (int i = 0, length = _nodes.Length; i < length; ++i)
				_nodes[i].Dispose();

			_nodes.Dispose();
			_values.Dispose();
			_properties.Dispose();
			_literals.Dispose();

			foreach (var (_, collection) in _valuesMap)
				collection.Clear();

			if (_factory == null)
				return;

			_factory.Release(this);
			_factory = null;
		}

		/// <summary>
		/// Creates a deep copy of the DocumentObject instance.
		/// </summary>
		/// <returns>A deep copy of the DocumentObject instance.</returns>
		public DocumentObjectModel Clone()
		{
			var document = _factory?.Retain() ?? new DocumentObjectModel();

			document._factory = _factory;
			document._nodes = new Vector<NodeData>(_nodes.Length);
			document._values = new Vector<ValueData>(_values.Length);
			document._properties = new Vector<PropertyData>(_properties.Length);
			document._literals = new MultiVector<char>(_literals.RowsCount);

			var addValueVisitor = new AddValueVisitor { TargetDocument = document };

			for (int i = 0, length = _nodes.Length; i < length; ++i)
			{
				ref readonly var node = ref _nodes[i];
				ref var nodeClone = ref document._nodes.Push();

				nodeClone.Name = document.AddLiteral(GetLiteral(node.Name));
				nodeClone.Annotation = node.Annotation.IsValid
					? document.AddLiteral(GetLiteral(node.Annotation))
					: default;

				var argumentsCount = node.Arguments.Length;
				nodeClone.Arguments = new Vector<int>(node.Arguments.Length);

				for (var j = 0; j < argumentsCount; ++j)
				{
					var value = new DocumentValue(this, node.Arguments[j]);

					value.Visit(addValueVisitor, out nodeClone.Arguments.Push());
				}

				var propertiesCount = node.Properties.Length;
				nodeClone.Properties = new Vector<int>(propertiesCount);

				for (var j = 0; j < propertiesCount; ++j)
				{
					ref readonly var property = ref _properties[node.Properties[j]];
					var value = new DocumentValue(this, property.ValueIndex);

					value.Visit(addValueVisitor, out int valueIndex);
					nodeClone.Properties.Push() = document.AddProperty(GetLiteral(property.Key), valueIndex);
				}

				nodeClone.Children = node.Children.Clone();
			}

			return document;
		}

		/// <summary>
		/// Adds a new node to the document object model with the specified name.
		/// </summary>
		/// <param name="name">The name of the node.</param>
		/// <param name="childrenCapacity">The initial capacity for child nodes of the new node.</param>
		/// <returns>The newly added <see cref="DocumentNode"/>.</returns>
		public DocumentNode AddNode(ReadOnlySpan<char> name, int childrenCapacity = 4) =>
			AddNode(AddLiteral(name), default, childrenCapacity);

		/// <summary>
		/// Retrieves a node from the document object model with the specified index.
		/// </summary>
		/// <param name="index">The index of the node to retrieve.</param>
		/// <returns>The <see cref="DocumentNode"/> with the specified index.</returns>
		public DocumentNode this[int index] =>
			new(this, index);

		/// <summary>
		/// Gets the node in the document object model with the specified name and index.
		/// </summary>
		/// <param name="name">The name of the node.</param>
		/// <param name="index">The index of the node.</param>
		/// <returns>The node in the document object model with the specified name and index, or default if not found.</returns>
		/// <remarks>
		/// This method searches for a node in the document object model with the specified name and index.
		/// The nameComparer parameter can be used to provide a custom comparer for node names. If not specified,
		/// the method uses the StringComparer.Ordinal comparer.
		/// </remarks>
		public DocumentNode GetNode(ReadOnlySpan<char> name, int index)
		{
			for (int i = 0, length = _nodes.Length; i < length; ++i)
			{
				if (GetLiteral(_nodes[i].Name).SequenceEqual(name) && index-- == 0)
					return new DocumentNode(this, i);
			}

			return default;
		}

		/// <summary>
		/// Removes a range of nodes from the document object model.
		/// </summary>
		/// <param name="startIndex">The starting index of the range to remove.</param>
		/// <param name="count">The number of nodes to remove.</param>
		public void RemoveNodes(int startIndex, int count)
		{
			if (startIndex < 0 || count < 0 || startIndex + count > _nodes.Length)
				throw new IndexOutOfRangeException();

			for (var i = 0; i < count; ++i)
				_nodes[startIndex + i].Dispose();

			_nodes.Erase(startIndex, count);

			for (int i = startIndex, length = _nodes.Length; i < length; ++i)
				++_nodes[i].Version;
		}

		internal Span<ValueData> Values =>
			_values.Buffer;

		internal Span<PropertyData> Properties =>
			_properties.Buffer;

		internal Span<NodeData> Nodes =>
			_nodes.Buffer;

		internal int AddValue<T>(T value)
		{
			_values.Push() = ValueData.AddValue(this, value);
			return _values.Length - 1;
		}

		internal int AddProperty(ReadOnlySpan<char> key, int valueIndex)
		{
			_properties.Push() = new PropertyData { Key = AddLiteral(key), ValueIndex = valueIndex };
			return _properties.Length - 1;
		}

		internal SafeIndex AddLiteral(ReadOnlySpan<char> value)
		{
			if (value.IsEmpty)
				return default;

			var index = _literals.RowsCount;
			_literals.PushRow(value);
			return index;
		}

		internal ReadOnlySpan<char> GetLiteral(SafeIndex index) =>
			index.IsValid ? _literals[index] : default;

		private DocumentNode AddNode(SafeIndex nameLiteral, SafeIndex annotationLiteral, int childrenCapacity)
		{
			ref var data = ref _nodes.Push();

			data.Version = 0;
			data.Name = nameLiteral;
			data.Annotation = annotationLiteral;
			data.Arguments = new Vector<int>();
			data.Properties = new Vector<int>();
			data.Children = Create(childrenCapacity);

			return new DocumentNode(this, _nodes.Length - 1);
		}

		internal abstract class TypeInfo
		{
			public readonly Type Type;

			protected TypeInfo(Type type) =>
				Type = type;

			public abstract void Visit<TVisitor, TResult>(in DocumentValue value, TVisitor visitor, out TResult result)
				where TVisitor : struct, IDocumentValueVisitor<TResult>;
		}

		internal sealed class TypeInfo<T> : TypeInfo
		{
			public static readonly TypeInfo<T> Instance = new();

			private TypeInfo() : base(typeof(T))
			{
			}

			public override void Visit<TVisitor, TResult>(in DocumentValue value, TVisitor visitor, out TResult result) =>
				result = visitor.Visit(value.GetValue<T>(), value.Annotation);
		}

		private struct AddValueVisitor : IDocumentValueVisitor<int>
		{
			public DocumentObjectModel TargetDocument;

			public int Visit<T>(in T value, ReadOnlySpan<char> annotation)
			{
				var valueIndex = TargetDocument.AddValue(value);
				TargetDocument._values[valueIndex].Annotation = TargetDocument.AddLiteral(annotation);
				return valueIndex;
			}
		}

		internal struct NodeData
		{
			public int Version;
			public SafeIndex Name;
			public SafeIndex Annotation;
			public Vector<int> Arguments;
			public Vector<int> Properties;
			public DocumentObjectModel Children;

			public void Dispose()
			{
				Arguments.Dispose();
				Properties.Dispose();
				Children.Dispose();
				Children = null;
			}
		}

		internal struct ValueData
		{
			private readonly int _index;
			public readonly TypeInfo TypeInfo;
			public SafeIndex Annotation;
			public int Version;

			private ValueData(int index, TypeInfo typeInfo)
			{
				_index = index;
				TypeInfo = typeInfo;
				Annotation = default;
				Version = 0;
			}

			public static ValueData AddValue<T>(DocumentObjectModel model, T value)
			{
				var type = typeof(T);
				if (!model._valuesMap.TryGetValue(type, out var collection))
					model._valuesMap.Add(type, collection = new List<T>());

				((List<T>) collection).Add(value);

				return new ValueData(collection.Count - 1, TypeInfo<T>.Instance);
			}

			public readonly T GetValue<T>(DocumentObjectModel model)
			{
				var type = typeof(T);
				if (type != TypeInfo.Type)
					throw new InvalidCastException();

				return model._valuesMap.TryGetValue(type, out var valuesArray)
					? ((List<T>) valuesArray)[_index]
					: throw new InvalidOperationException();
			}

			public readonly T UpdateValue<T>(DocumentObjectModel model, T value)
			{
				var type = typeof(T);
				if (type != TypeInfo.Type)
					throw new InvalidCastException();

				if (model._valuesMap.TryGetValue(type, out var collection))
					return ((List<T>) collection)[_index] = value;

				throw new InvalidOperationException();
			}
		}

		internal struct PropertyData
		{
			public int Version;
			public SafeIndex Key;
			public int ValueIndex;
		}

		private readonly ref struct DebuggerView
		{
			private readonly DocumentObjectModel _model;

			public DebuggerView(DocumentObjectModel model) =>
				_model = model;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public DocumentNode[] Nodes
			{
				get
				{
					var nodes = new DocumentNode[_model.NodesCount];

					for (int i = 0, length = nodes.Length; i < length; ++i)
						nodes[i] = _model[i];

					return nodes;
				}
			}
		}
	}
}

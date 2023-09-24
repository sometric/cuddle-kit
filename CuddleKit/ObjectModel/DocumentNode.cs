using System;
using System.Diagnostics;
using CuddleKit.Internal;

namespace CuddleKit.ObjectModel
{
	/// <summary>
	/// Represents a node in the document object model.
	/// </summary>
	[DebuggerDisplay("{IsValid ? Name : string.Empty,nq}")]
	[DebuggerTypeProxy(typeof(DebuggerView))]
	public readonly struct DocumentNode
	{
		private readonly DocumentObjectModel _parent;
		private readonly SafeIndex _index;
		private readonly int _version;

		internal DocumentNode(DocumentObjectModel parent, SafeIndex index) =>
			(_parent, _index, _version) = (parent, index, parent.Nodes[index].Version);

		/// <summary>
		/// Gets a value indicating whether the node is still valid and hasn't been modified or removed.
		/// </summary>
		public bool IsValid
		{
			get
			{
				var nodes = _parent != null ? _parent.Nodes : default;
				return 
					_index.IsValidForRange(nodes.Length) &&
					nodes[_index].Version == _version;
			}
		}

		/// <summary>
		/// Gets or sets the name of the node.
		/// </summary>
		public ReadOnlySpan<char> Name
		{
			get => _parent.GetLiteral(GetData().Name);
			set => GetData().Name = _parent.AddLiteral(value);
		}

		/// <summary>
		/// Gets or sets the annotation of the node.
		/// </summary>
		public ReadOnlySpan<char> Annotation 
		{
			get => _parent.GetLiteral(GetData().Annotation);
			set => GetData().Annotation = _parent.AddLiteral(value);
		}

		/// <summary>
		/// Gets the number of child nodes of the node.
		/// </summary>
		public int NodesCount =>
			GetData().Children.NodesCount;

		/// <summary>
		/// Gets the number of arguments of the node.
		/// </summary>
		public int ArgumentsCount =>
			GetData().Arguments.Length;

		/// <summary>
		/// Gets the number of properties of the node.
		/// </summary>
		public int PropertiesCount =>
			GetData().Properties.Length;

		/// <summary>
		/// Adds a child node to the current node with the specified name and annotation.
		/// </summary>
		/// <param name="name">The name of the child node.</param>
		/// <param name="childrenCapacity">The initial capacity for child nodes of the child node.</param>
		/// <returns>The newly added <see cref="DocumentNode"/>.</returns>
		public DocumentNode AddNode(ReadOnlySpan<char> name, int childrenCapacity = 4) =>
			GetData().Children.AddNode(name, childrenCapacity);

		/// <summary>
		/// Retrieves a child node from the current node with the specified index.
		/// </summary>
		/// <param name="index">The index of the child node to retrieve.</param>
		/// <returns>The <see cref="DocumentNode"/> with the specified index.</returns>
		public DocumentNode this[int index] =>
			GetData().Children[index];

		/// <summary>
		/// Removes a specified number of child nodes from the current node starting at the specified index.
		/// </summary>
		/// <param name="startIndex">The index of the first child node to remove.</param>
		/// <param name="count">The number of child nodes to remove.</param>
		public void RemoveNodes(int startIndex, int count) =>
			GetData().Children.RemoveNodes(startIndex, count);

		/// <summary>
		/// Adds an argument to the node with the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the argument value.</typeparam>
		/// <param name="value">The value of the argument.</param>
		public DocumentValue AddArgument<T>(T value)
		{
			ref var index = ref GetData().Arguments.Push();
			index = _parent.AddValue(value);
			return new DocumentValue(_parent, index);
		}

		/// <summary>
		/// Retrieves an argument value from the node with the specified index.
		/// </summary>
		/// <param name="index">The index of the argument value to retrieve.</param>
		/// <returns>The argument value at the specified index.</returns>
		public DocumentValue GetArgument(int index) =>
			new(_parent, GetData().Arguments[index]);

		/// <summary>
		/// Removes a specified number of arguments from the current node starting at the specified index.
		/// </summary>
		/// <param name="startIndex">The index of the first argument to remove.</param>
		/// <param name="count">The number of arguments to remove.</param>
		public void RemoveArguments(int startIndex, int count)
		{
			ref var arguments = ref GetData().Arguments;

			if (startIndex < 0 || count < 0 || startIndex + count > arguments.Length)
				throw new IndexOutOfRangeException();

			for (int i = startIndex, length = arguments.Length; i < length; ++i)
				++_parent.Values[arguments[i]].Version;

			arguments.Erase(startIndex, count);
		}

		/// <summary>
		/// Adds a property to the node with the specified key and value.
		/// </summary>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="key">The key of the property.</param>
		/// <param name="value">The value of the property.</param>
		public DocumentProperty AddProperty<T>(ReadOnlySpan<char> key, T value)
		{
			ref var index = ref GetData().Properties.Push();
			index = _parent.AddProperty(key, _parent.AddValue(value));
			return new DocumentProperty(_parent, index);
		}

		/// <summary>
		/// Retrieves a property from the node with the specified index.
		/// </summary>
		/// <param name="index">The index of the property to retrieve.</param>
		/// <returns>The <see cref="DocumentProperty"/> with the specified index.</returns>
		public DocumentProperty GetProperty(int index) =>
			new(_parent, GetData().Properties[index]);

		/// <summary>
		/// Removes a specified number of properties from the current node starting at the specified index.
		/// </summary>
		/// <param name="startIndex">The index of the first property to remove.</param>
		/// <param name="count">The number of properties to remove.</param>
		public void RemoveProperties(int startIndex, int count)
		{
			ref var properties = ref GetData().Properties;

			for (int i = startIndex, length = properties.Length; i < length; ++i)
				++_parent.Properties[properties[i]].Version;

			properties.Erase(startIndex, count);
		}

		private ref DocumentObjectModel.NodeData GetData()
		{
			ref var data = ref _parent.Nodes[_index];

			if (data.Version != _version)
				throw new Exception();

			return ref data;
		}


		private readonly ref struct DebuggerView
		{
			private readonly DocumentNode _node;

			public DebuggerView(DocumentNode node) =>
				_node = node;

			public string Annotation =>
				_node is { IsValid: true, Annotation: { IsEmpty: false } }
					? _node.Annotation.ToString()
					: null;

			public DocumentValue[] Arguments
			{
				get
				{
					if (!_node.IsValid)
						return null;

					var arguments = new DocumentValue[_node.ArgumentsCount];

					for (int i = 0, length = arguments.Length; i < length; ++i)
						arguments[i] = _node.GetArgument(i);

					return arguments;
				}
			}

			public DocumentProperty[] Properties
			{
				get
				{
					if (!_node.IsValid)
						return null;

					var properties = new DocumentProperty[_node.PropertiesCount];

					for (int i = 0, length = properties.Length; i < length; ++i)
						properties[i] = _node.GetProperty(i);

					return properties;
				}
			}

			public DocumentNode[] Nodes
			{
				get
				{
					if (!_node.IsValid)
						return null;

					var nodes = new DocumentNode[_node.NodesCount];

					for (int i = 0, length = nodes.Length; i < length; ++i)
						nodes[i] = _node[i];

					return nodes;
				}
			}
		}
	}
}

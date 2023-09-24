using System;

namespace CuddleKit.Serialization
{
	using Internal;

	public ref struct Document
	{
		private Vector<NodeDescriptor> _nodes;
		private Vector<ValueDescriptor> _values;
		private MultiVector<ValueReference> _arguments;
		private MultiVector<PropertyReference> _properties;
		private MultiVector<NodeReference> _children;
		private TokenTable _tokenTable;

		public static Document Deserialize(Reader reader)
		{
			var document = new Document();
			document.Read(reader);
			return document;
		}

		public void Dispose()
		{
			_nodes.Dispose();
			_values.Dispose();
			_children.Dispose();
			_arguments.Dispose();
			_properties.Dispose();
			_tokenTable.Dispose();
		}

		public void Clear()
		{
			_nodes.Clear();
			_values.Clear();
			_children.Clear();
			_arguments.Clear();
			_properties.Clear();
			_tokenTable.Clear();
		}

		public readonly bool Equals(in Document other)
		{
			var nodes = Nodes;
			var otherNodes = other.Nodes;

			if (nodes.Length != otherNodes.Length)
				return false;

			for (var i = 0; i < nodes.Length; ++i)
			{
				if (!Equals(other, nodes[i], otherNodes[i]))
					return false;
			}

			return true;
		}

		private readonly bool Equals(Document other, NodeReference node, NodeReference otherNode)
		{
			if (!GetName(node).SequenceEqual(other.GetName(otherNode)))
				return false;

			var hasAnnotation = TryGetAnnotation(node, out var annotation);
			var otherHasAnnotation = other.TryGetAnnotation(node, out var otherAnnotation);
			if (hasAnnotation != otherHasAnnotation)
				return false;

			if (hasAnnotation && !annotation.SequenceEqual(otherAnnotation))
				return false;

			var arguments = GetArguments(node);
			var otherArguments = other.GetArguments(otherNode);
			if (arguments.Length != otherArguments.Length)
				return false;

			for (var i = 0; i < arguments.Length; ++i)
			{
				if (!Equals(other, arguments[i], otherArguments[i]))
					return false;
			}

			var properties = GetProperties(node);
			var otherProperties = other.GetProperties(otherNode);
			if (properties.Length != otherProperties.Length)
				return false;

			for (var i = 0; i < properties.Length; ++i)
			{
				var property = properties[i];
				var otherProperty = otherProperties[i];

				if (!GetKey(property).SequenceEqual(other.GetKey(otherProperty)))
					return false;

				if (!Equals(other, property.Value, otherProperty.Value))
					return false;
			}

			var children = GetChildren(node);
			var otherChildren = other.GetChildren(otherNode);
			if (children.Length != otherChildren.Length)
				return false;

			for (var i = 0; i < children.Length; ++i)
			{
				if (!Equals(other, children[i], otherChildren[i]))
					return false;
			}

			return true;
		}

		private readonly bool Equals(in Document other, ValueReference value, ValueReference otherValue)
		{
			if (GetType(value) != other.GetType(otherValue))
				return false;

			if (!GetData(value).SequenceEqual(other.GetData(otherValue)))
				return false;

			var hasAnnotation = TryGetAnnotation(value, out var annotation);
			var otherHasAnnotation = other.TryGetAnnotation(otherValue, out var otherAnnotation);

			if (hasAnnotation != otherHasAnnotation)
				return false;

			return !hasAnnotation || annotation.SequenceEqual(otherAnnotation);
		}

		public readonly ReadOnlySpan<NodeReference> Nodes =>
			_children.RowsCount > 0 ? _children[0] : default;

		public readonly ReadOnlySpan<char> GetName(NodeReference node) =>
			_tokenTable.GetTokenData(_nodes[node.Index].NameToken);

		public readonly bool TryGetAnnotation(NodeReference node, out ReadOnlySpan<char> annotation)
		{
			var annotationToken = _nodes[node.Index].AnnotationToken;
			annotation = annotationToken.IsValid ? _tokenTable.GetTokenData(annotationToken) : default;
			return annotationToken.IsValid;
		}

		public readonly ReadOnlySpan<NodeReference> GetChildren(NodeReference node)
		{
			var index = _nodes[node.Index].ChildrenRow;
			return index.IsValid ? _children[index] : default;
		}

		public readonly ReadOnlySpan<ValueReference> GetArguments(NodeReference node)
		{
			var index = _nodes[node.Index].ArgumentsRow;
			return index.IsValid ? _arguments[index] : default;
		}

		public readonly ReadOnlySpan<PropertyReference> GetProperties(NodeReference node)
		{
			var index = _nodes[node.Index].PropertiesRow;
			return index.IsValid ? _properties[index] : default;
		}

		public readonly ReadOnlySpan<char> GetKey(PropertyReference property) =>
			_tokenTable.GetTokenData(property.Key);

		public readonly ReadOnlySpan<char> GetData(ValueReference value) =>
			_tokenTable.GetTokenData(_values[value.Index].ValueToken);

		public readonly DataType GetType(ValueReference value) =>
			_tokenTable.GetTokenType(_values[value.Index].ValueToken);

		public readonly bool TryGetAnnotation(ValueReference value, out ReadOnlySpan<char> annotation)
		{
			var annotationToken = _values[value.Index].AnnotationToken;
			annotation = annotationToken.IsValid ? _tokenTable.GetTokenData(annotationToken) : default;
			return annotationToken.IsValid;
		}

		public void Read(Reader reader)
		{
			reader.ScanAll(Literal.LineSpace);

			if (_children.RowsCount == 0)
				_children.PushRow(4);

			using var children = new Vector<NodeReference>();

			while (!reader.Done)
			{
				var child = ReadNode(ref reader, false);
				if (child.IsValid)
					children.Push() = child;
			}

			_children.Push(0, children.ReadOnlyBuffer);
		}

		private NodeReference ReadNode(ref Reader reader, bool parentEliminated)
		{
			var nodeEliminated = reader.Scan(Literal.SlashDashComment) | parentEliminated;
			var annotation = reader.ReadAnnotation(ref _tokenTable).GetValueOrDefault();
			var identifier =
				reader.ReadString(ref _tokenTable) ??
				reader.ReadIdentifier(ref _tokenTable) ??
				throw reader.MakeException("Unexpected node id token");

			using var values = new Vector<ValueDescriptor>();
			using var arguments = new Vector<int>();
			using var properties = new Vector<(TokenReference Key, int Value)>();

			var hasChildren = false;
			var childrenEliminated = false;

			while (true)
			{
				var hasSpace = reader.ScanAll(Literal.NodeSpace);

				if (reader.Scan(Literal.NodeTerminator))
					break;

				var contentEliminated = reader.Scan(Literal.SlashDashComment);

				if (reader.ScanSymbol('{'))
				{
					hasChildren = true;
					childrenEliminated = contentEliminated;
					break;
				}

				if (!hasSpace)
					throw reader.MakeExpectationException("WhiteSpace");

				var argumentAnnotation = reader.ReadAnnotation(ref _tokenTable).GetValueOrDefault();
				if (argumentAnnotation.IsValid)
				{
					var argumentValue =
						reader.ReadValue(ref _tokenTable) ??
						throw reader.MakeExpectationException("Value");

					if (!contentEliminated)
					{
						arguments.Push() = values.Length;
						values.Push() = new ValueDescriptor(argumentValue, argumentAnnotation);
					}

					continue;
				}

				TokenReference? identifierToken = default;
				var value =
					reader.ReadString(ref _tokenTable) ??
					reader.ReadNumber(ref _tokenTable) ??
					(identifierToken = reader.ReadIdentifier(ref _tokenTable)) ??
					reader.ReadKeyword(ref _tokenTable) ??
					throw reader.MakeException("Value or identifier expected");

				var valueTokenType = _tokenTable.GetTokenType(value);
				if (valueTokenType == DataType.String && reader.ScanSymbol('='))
				{
					var propertyAnnotation = reader
						.ReadAnnotation(ref _tokenTable)
						.GetValueOrDefault();

					var propertyValue =
						reader.ReadValue(ref _tokenTable) ??
						throw reader.MakeExpectationException("Value");

					if (!contentEliminated)
					{
						properties.Push() = (Key: value, Value: values.Length);
						values.Push() = new ValueDescriptor(propertyValue, propertyAnnotation);
					}
				}
				else if (!identifierToken.HasValue)
				{
					if (!contentEliminated)
					{
						arguments.Push() = values.Length;
						values.Push() = new ValueDescriptor(value);
					}
				}
				else
					throw reader.MakeException("Unexpected identifier as node argument");
			}

			var node = nodeEliminated
				? default
				: AddNode(identifier);

			if (node.IsValid)
				_nodes[node.Index].AnnotationToken = annotation;

			var valuesOffset = _values.Length;
			_values.Push(values.ReadOnlyBuffer);

			if (node.IsValid & (arguments.Length > 0))
			{
				var rowIndex = TouchArgumentsRow(node, arguments.Length);
				var nodeArguments = _arguments.Push(rowIndex, arguments.Length);

				for (var i = 0; i < arguments.Length; ++i)
					nodeArguments[i] = new ValueReference(valuesOffset + arguments[i]);
			}

			if (node.IsValid & (properties.Length > 0))
			{
				var rowIndex = TouchPropertiesRow(node, arguments.Length);
				foreach (var property in properties.ReadOnlyBuffer)
				{
					var value = new ValueReference(valuesOffset + property.Value);
					SetProperty(rowIndex, new PropertyReference(property.Key, value));
				}
			}

			if (hasChildren)
			{
				using var children = new Vector<NodeReference>();
				reader.ScanAll(Literal.LineSpace);

				while (!reader.ScanSymbol('}'))
				{
					var child = ReadNode(ref reader, nodeEliminated | childrenEliminated);
					if (child.IsValid)
						children.Push() = child;
				}

				reader.Scan(Literal.NodeTerminator);

				if (node.IsValid & (children.Length > 0))
				{
					_nodes[node.Index].ChildrenRow = _children.RowsCount;
					_children.PushRow(children.ReadOnlyBuffer);
				}
			}

			reader.ScanAll(Literal.LineSpace);

			return node;
		}

		public TokenReference AllocateString(in ReadOnlySpan<char> value) =>
			!value.IsEmpty
				? _tokenTable.AllocateToken(DataType.String, value)
				: default;

		public NodeReference AddNode(TokenReference token)
		{
			var node = new NodeReference(_nodes.Length);
			_nodes.Push() = new NodeDescriptor { NameToken = token };

			return node;
		}

		public ReadOnlySpan<NodeReference> AddNodes(int count, ReadOnlySpan<char> name = default) =>
			AddNodes(count, AllocateString(name));

		public ReadOnlySpan<NodeReference> AddNodes(int count, TokenReference nameToken)
		{
			if (_children.RowsCount == 0)
				_children.PushRow(count);

			return AddNodes(0, count, nameToken);
		}

		public ReadOnlySpan<NodeReference> AddNodes(NodeReference parent, int count, ReadOnlySpan<char> name = default) =>
			AddNodes(parent, count, AllocateString(name));

		public ReadOnlySpan<NodeReference> AddNodes(NodeReference parent, int count, TokenReference nameToken) =>
			parent.IsValid
				? AddNodes(TouchChildrenRow(parent, count), count, nameToken)
				: AddNodes(count, nameToken);

		public NodeReference AddNode(ReadOnlySpan<char> name = default) =>
			AddNodes(1, AllocateString(name))[0];

		public NodeReference AddNode(NodeReference parent, ReadOnlySpan<char> name = default) =>
			AddNodes(parent, 1, name)[0];

		public NodeReference AddNode(NodeReference parent, TokenReference nameToken) =>
			AddNodes(parent, 1, nameToken)[0];

		public void NameNodes(ReadOnlySpan<NodeReference> nodes, ReadOnlySpan<char> name)
		{
			var idToken = AllocateString(name);
			foreach (var node in nodes)
				_nodes[node.Index].NameToken = idToken;
		}

		public void NameNode(NodeReference node, ReadOnlySpan<char> name) =>
			_nodes[node.Index].NameToken = AllocateString(name);

		public void AnnotateNodes(ReadOnlySpan<NodeReference> nodes, ReadOnlySpan<char> annotation)
		{
			var annotationToken = AllocateString(annotation);
			foreach (var node in nodes)
				_nodes[node.Index].AnnotationToken = annotationToken;
		}

		public void Annotate(NodeReference node, ReadOnlySpan<char> annotation) =>
			_nodes[node.Index].AnnotationToken = AllocateString(annotation);

		public void AddArguments(NodeReference node, ReadOnlySpan<ValueReference> values) =>
			_arguments.Push(TouchArgumentsRow(node, values.Length), values);

		public void AddArgument(NodeReference node, ValueReference value) =>
			_arguments.Push(TouchArgumentsRow(node, 1), 1)[0] = value;

		public void SetProperty(NodeReference node, ReadOnlySpan<char> key, ValueReference value) =>
			SetProperty(node, _tokenTable.AllocateToken(DataType.String, key), value);

		public void SetProperty(NodeReference node, TokenReference keyToken, ValueReference value) =>
			SetProperty(TouchPropertiesRow(node, 1), new PropertyReference(keyToken, value));

		public ValueReference GetProperty(NodeReference node, ReadOnlySpan<char> key)
		{
			var index = _nodes[node.Index].PropertiesRow;
			return index.IsValid ? GetProperty(index, key) : default;
		}

		private SafeIndex TouchChildrenRow(NodeReference node, int initialCapacity)
		{
			ref var description = ref _nodes[node.Index];
			if (description.ChildrenRow.IsValid)
				return description.ChildrenRow;

			description.ChildrenRow = _children.RowsCount;
			_children.PushRow(initialCapacity);
			return description.ChildrenRow;
		}

		private SafeIndex TouchArgumentsRow(NodeReference node, int requiredCapacity)
		{
			ref var description = ref _nodes[node.Index];
			if (description.ArgumentsRow.IsValid)
				return description.ArgumentsRow;

			description.ArgumentsRow = _arguments.RowsCount;
			_arguments.PushRow(requiredCapacity);
			return description.ArgumentsRow;
		}

		private SafeIndex TouchPropertiesRow(NodeReference node, int initialCapacity)
		{
			ref var description = ref _nodes[node.Index];
			if (description.PropertiesRow.IsValid)
				return description.PropertiesRow;

			description.PropertiesRow = _properties.RowsCount;
			_properties.PushRow(initialCapacity);
			return description.PropertiesRow;
		}

		public ValueReference AddValue(DataType type, ReadOnlySpan<char> data, ReadOnlySpan<char> annotation = default)
		{
			var annotationToken = AllocateString(annotation);

			if (type == DataType.Keyword)
			{
				Span<char> keyword = stackalloc char[data.Length];
				data.ToLowerInvariant(keyword);

				for (int i = 0, length = Characters.Keywords.Length; i < length; ++i)
				{
					if (keyword.SequenceEqual(Characters.Keywords[i]))
					{
						var keywordToken = _tokenTable.AllocateKeywordToken(i);
						return AddValue(keywordToken, annotationToken);
					}
				}

				throw new InvalidOperationException("");
			}

			var valueToken = _tokenTable.AllocateToken(type, data);

			return AddValue(valueToken, annotationToken);
		}

		private ValueReference AddValue(TokenReference dataToken, TokenReference annotationToken)
		{
			_values.Push() = new ValueDescriptor(dataToken, annotationToken);
			return new ValueReference(_values.Length - 1);
		}

		private ReadOnlySpan<NodeReference> AddNodes(SafeIndex rowIndex, int count, TokenReference nameToken)
		{

			var template = new NodeDescriptor { NameToken = nameToken };

			var offset = _nodes.Length;
			_nodes.PushMany(count).Fill(template);

			var addedChildren = _children.Push(rowIndex, count);
			for (var i = 0; i < count; ++i)
				addedChildren[i] = new NodeReference(i + offset);

			return addedChildren;
		}

		private ValueReference GetProperty(SafeIndex rowIndex, ReadOnlySpan<char> key) =>
			TryGetPropertyPosition(rowIndex, key, out var position)
				? _properties[rowIndex][position].Value
				: default;

		private void SetProperty(SafeIndex rowIndex, in PropertyReference property)
		{
			var key = _tokenTable.GetTokenData(property.Key);
			if (TryGetPropertyPosition(rowIndex, key, out var position))
				_properties[rowIndex][position] = property;
			else if (position >= _properties[rowIndex].Length)
				_properties.Push(rowIndex, 1)[0] = property;
			else
				_properties.Insert(rowIndex, position, 1)[0] = property;
		}

		private bool TryGetPropertyPosition(SafeIndex rowIndex, ReadOnlySpan<char> key, out int position)
		{
			var keys = _properties[rowIndex];
			Span<int> bounds = stackalloc int[2] { 0, keys.Length };

			while (bounds[0] < bounds[1])
			{
				var middle = (bounds[0] + bounds[1]) >> 1;
				var middleKey = _tokenTable.GetTokenData(keys[middle].Key);
				var less = middleKey.SequenceCompareTo(key) < 0 ? 1 : 0;
				bounds[1 - less] = middle + less;
			}

			position = bounds[0];
			return position < keys.Length && _tokenTable.GetTokenData(keys[position].Key).SequenceEqual(key);
		}

		private struct NodeDescriptor
		{
			public TokenReference NameToken;
			public TokenReference AnnotationToken;
			public SafeIndex ArgumentsRow;
			public SafeIndex PropertiesRow;
			public SafeIndex ChildrenRow;
		}

		private readonly struct ValueDescriptor
		{
			internal readonly TokenReference ValueToken;
			internal readonly TokenReference AnnotationToken;

			internal ValueDescriptor(TokenReference value, TokenReference annotation = default) =>
				(ValueToken, AnnotationToken) = (value, annotation);
		}
	}
}

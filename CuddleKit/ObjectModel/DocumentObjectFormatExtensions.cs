using System;

namespace CuddleKit.ObjectModel
{
	using Internal;
	using Format;
	using Serialization;

	public static class DocumentObjectFormatExtensions
	{
		public static Document Export(this DocumentObjectModel sourceModel, in FormatterRegistry registry)
		{
			using var formatter = new DocumentFormatter(registry);
			var document = new Document();

			try
			{
				formatter.ExportModel(sourceModel, ref document);
			}
			catch
			{
				document.Dispose();
				throw;
			}

			return document;
		}

		public static Document Export(this DocumentObjectModel sourceModel) =>
			Export(sourceModel, FormatterRegistry.Default);

		public static void Export(this DocumentObjectModel sourceModel,
			ref Document targetDocument,
			FormatterRegistry registry)
		{
			using var formatter = new DocumentFormatter(registry);
			formatter.ExportModel(sourceModel, ref targetDocument);
		}

		public static void Export(this DocumentObjectModel sourceModel, ref Document targetDocument) =>
			Export(sourceModel, FormatterRegistry.Default);

		public static DocumentObjectModel ImportModel(this in Document sourceDocument, FormatterRegistry registry)
		{
			using var formatter = new DocumentFormatter(registry);
			var model = new DocumentObjectModel();

			try
			{
				formatter.ImportModel(sourceDocument, model);
			}
			catch
			{
				model.Dispose();
				throw;
			}

			return model;
		}

		public static DocumentObjectModel ImportModel(this in Document sourceDocument) =>
			ImportModel(sourceDocument, FormatterRegistry.Default);

		public static void ImportModel(this in Document sourceDocument,
			DocumentObjectModel targetModel,
			FormatterRegistry registry)
		{
			using var formatter = new DocumentFormatter(registry);
			formatter.ImportModel(sourceDocument, targetModel);
		}

		public static void ImportModel(this in Document sourceDocument, DocumentObjectModel targetModel) =>
			ImportModel(sourceDocument, targetModel, FormatterRegistry.Default);

		private ref struct DocumentFormatter
		{
			private readonly FormatterRegistry _registry;
			private DocumentNodeArgumentProxy _argumentProxy;
			private DocumentNodePropertyProxy _propertyProxy;
			private DocumentValueExportProxy _valueExportProxy;

			public DocumentFormatter(in FormatterRegistry registry) : this() =>
				_registry = registry;

			public void Dispose() =>
				_propertyProxy.Dispose();

			public void ExportModel(DocumentObjectModel model, ref Document document)
			{
				for (int i = 0, nodesCount = model.NodesCount; i < nodesCount; ++i)
					ExportNode(model[i], default, ref document);
			}

			public void ImportModel(in Document document, DocumentObjectModel model)
			{
				var nodes = document.Nodes;

				for (int i = 0, nodesCount = nodes.Length; i < nodesCount; ++i)
				{
					var nodeReference = nodes[i];
					var documentNode = model.AddNode(
						document.GetName(nodeReference),
						document.GetChildren(nodeReference).Length);

					ImportNode(document, nodes[i], documentNode);
				}
			}

			private void ImportNode(in Document document, NodeReference nodeReference, DocumentNode documentNode)
			{
				if (document.TryGetAnnotation(nodeReference, out var nodeAnnotation))
					documentNode.Annotation = nodeAnnotation;

				_argumentProxy.DocumentNode = documentNode;

				var arguments = document.GetArguments(nodeReference);
				for (int i = 0, argumentsCount = arguments.Length; i < argumentsCount; ++i)
				{
					var valueReference = arguments[i];
					var valueType = document.GetType(valueReference);
					document.TryGetAnnotation(valueReference, out var valueAnnotation);

					_registry
						.Lookup(valueType, valueAnnotation)?
						.Import(document, valueReference, ref _argumentProxy);
				}

				_propertyProxy.DocumentNode = documentNode;

				var properties = document.GetProperties(nodeReference);
				for (int i = 0, propertiesCount = properties.Length; i < propertiesCount; ++i)
				{
					var propertyReference = properties[i];
					var valueType = document.GetType(propertyReference.Value);
					document.TryGetAnnotation(propertyReference.Value, out var valueAnnotation);

					_propertyProxy.Key.Reset(document.GetKey(propertyReference));
					_registry
						.Lookup(valueType, valueAnnotation)?
						.Import(document, propertyReference.Value, ref _propertyProxy);
				}

				var nodes = document.GetChildren(nodeReference);
				for (int i = 0, nodesCount = nodes.Length; i < nodesCount; ++i)
				{
					var childNodeReference = nodes[i];
					var childDocumentNode = documentNode.AddNode(
						document.GetName(childNodeReference),
						document.GetChildren(childNodeReference).Length);

					ImportNode(document, childNodeReference, childDocumentNode);
				}
			}

			private void ExportNode(in DocumentNode node, NodeReference parentReference, ref Document document)
			{
				var resolutionVisitor = new FormatterResolutionVisitor(_registry);
				var nodeReference = parentReference.IsValid
					? document.AddNode(parentReference, node.Name)
					: document.AddNode(node.Name);

				document.Annotate(nodeReference, node.Annotation);

				for (int i = 0, argumentsCount = node.ArgumentsCount; i < argumentsCount; ++i)
				{
					var argument = node.GetArgument(i);
					argument.Visit(resolutionVisitor, out IFormatter formatter);

					_valueExportProxy.DocumentValue = argument;
					var valueReference = formatter.Export(ref _valueExportProxy, ref document);
					document.AddArgument(nodeReference, valueReference);
				}

				for (int i = 0, propertiesCount = node.PropertiesCount; i < propertiesCount; ++i)
				{
					var property = node.GetProperty(i);
					property.Value.Visit(resolutionVisitor, out IFormatter formatter);

					_valueExportProxy.DocumentValue = property.Value;
					var valueReference = formatter.Export(ref _valueExportProxy, ref document);
					document.SetProperty(nodeReference, property.Key, valueReference);
				}

				for (int i = 0, nodesCount = node.NodesCount; i < nodesCount; ++i)
					ExportNode(node[i], nodeReference, ref document);
			}
		}

		private struct DocumentValueExportProxy : IFormatterExportProxy
		{
			public DocumentValue DocumentValue;

			public readonly ReadOnlySpan<char> Annotation =>
				DocumentValue.Annotation;

			TValue IFormatterExportProxy.Export<TValue>() =>
				DocumentValue.GetValue<TValue>();
		}

		private struct DocumentNodeArgumentProxy : IFormatterImportProxy
		{
			public DocumentNode DocumentNode;

			void IFormatterImportProxy.Import<T>(T value, ReadOnlySpan<char> annotation) =>
				DocumentNode.AddArgument(value);
		}

		private struct DocumentNodePropertyProxy : IFormatterImportProxy
		{
			public DocumentNode DocumentNode;
			public Vector<char> Key;

			public void Dispose() =>
				Key.Dispose();

			void IFormatterImportProxy.Import<TValue>(TValue value, ReadOnlySpan<char> annotation)
			{
				var property = DocumentNode.AddProperty(Key.ReadOnlyBuffer, value);
				property.Annotation = annotation;
			}
		}

		private readonly struct FormatterResolutionVisitor : IDocumentValueVisitor<IFormatter>
		{
			private readonly FormatterRegistry _registry;

			public FormatterResolutionVisitor(FormatterRegistry registry) =>
				_registry = registry;

			IFormatter IDocumentValueVisitor<IFormatter>.Visit<TValue>(in TValue value, ReadOnlySpan<char> annotation) =>
				_registry.Lookup(typeof(TValue), annotation);
		}
	}
}

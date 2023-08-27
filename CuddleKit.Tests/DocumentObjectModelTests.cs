using System;
using CuddleKit.ObjectModel;
using NUnit.Framework;

namespace CuddleKit.Tests
{
	[TestFixture]
	public class DocumentObjectModelTests
	{
		[Test]
		public void Document_DefaultValues_Invalid()
		{
			// Arrange
			DocumentNode node = default;
			DocumentValue value = default;
			DocumentProperty property = default;

			// Assert
			Assert.IsFalse(node.IsValid);
			Assert.IsFalse(value.IsValid);
			Assert.IsFalse(property.IsValid);
		}

		[Test]
		public void CreateDocument_InitialState_NodesCountZero()
		{
			// Arrange
			using var document = DocumentObjectModel.Create(factory: TreadLocalDocumentObjectModelFactory.Shared);

			// Assert
			Assert.Zero(document.NodesCount);
		}

		[Test]
		public void AddNode_ValidNode_NodeAddedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			const string nodeName = "Node";

			// Act
			var node = document.AddNode(nodeName);

			// Assert
			Assert.AreEqual(1, document.NodesCount);
			Assert.AreEqual(nodeName, node.Name.ToString());
		}

		[Test]
		public void AddArgument_ValidArgument_ArgumentAddedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const int argumentValue = 10;

			// Act
			var argument = node.AddArgument(argumentValue);

			// Assert
			Assert.AreEqual(1, node.ArgumentsCount);
			Assert.IsTrue(argument.ValueEquals(argumentValue));
		}

		[Test]
		public void AddProperty_ValidProperty_PropertyAddedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const string propertyName = "Key";
			const string propertyValue = "Value";

			// Act
			var property = node.AddProperty(propertyName, propertyValue);

			// Assert
			Assert.AreEqual(1, node.PropertiesCount);
			Assert.AreEqual(propertyName, property.Key.ToString());
			Assert.IsTrue(property.Value.Equals(propertyValue, StringComparer.Ordinal));
		}

		[Test]
		public void AddNode_WithAnnotation_NodeWithAnnotationAddedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			const string nodeName = "Node";
			const string nodeAnnotation = "NodeAnnotation";

			// Act
			var node = document.AddNode(nodeName).WithAnnotation(nodeAnnotation);

			// Assert
			Assert.AreEqual(nodeAnnotation, node.Annotation.ToString());
		}

		[Test]
		public void AddArgument_WithAnnotation_ArgumentWithAnnotationAddedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const int argumentValue = 10;
			const string argumentAnnotation = "ArgumentAnnotation";

			// Act
			var argument = node.AddArgument(argumentValue).WithAnnotation(argumentAnnotation);

			// Assert
			Assert.AreEqual(argumentAnnotation, argument.Annotation.ToString());
		}

		[Test]
		public void AddProperty_WithAnnotation_PropertyWithValueAndAnnotationAddedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const string propertyName = "Key";
			const string propertyValue = "Value";
			const string propertyAnnotation = "PropertyAnnotation";

			// Act
			var property = node.AddProperty(propertyName, propertyValue).WithAnnotation(propertyAnnotation);

			// Assert
			Assert.AreEqual(propertyAnnotation, property.Value.Annotation.ToString());
		}

		[Test]
		public void UpdateNodeAnnotation_ValidAnnotation_AnnotationUpdatedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node").WithAnnotation("OriginalAnnotation");
			const string updatedAnnotation = "UpdatedAnnotation";

			// Act
			node.Annotation = updatedAnnotation;

			// Assert
			Assert.AreEqual(updatedAnnotation, node.Annotation.ToString());
		}

		[Test]
		public void UpdateArgumentAnnotation_ValidAnnotation_AnnotationUpdatedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const int argumentValue = 10;
			const string originalAnnotation = "OriginalAnnotation";
			const string updatedAnnotation = "UpdatedAnnotation";

			// Add the argument with the original annotation
			var argument = node.AddArgument(argumentValue).WithAnnotation(originalAnnotation);

			// Act
			argument.Annotation = updatedAnnotation;

			// Assert
			Assert.AreEqual(updatedAnnotation, argument.Annotation.ToString());
		}

		[Test]
		public void UpdatePropertyValueAnnotation_ValidAnnotation_AnnotationUpdatedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const string propertyName = "Key";
			const string propertyValue = "Value";
			const string originalAnnotation = "OriginalAnnotation";
			const string updatedAnnotation = "UpdatedAnnotation";

			// Add the property with the original annotation
			var property = node.AddProperty(propertyName, propertyValue).WithAnnotation(originalAnnotation);

			// Act
			property.Annotation = updatedAnnotation;

			// Assert
			Assert.AreEqual(updatedAnnotation, property.Annotation.ToString());
			Assert.AreEqual(updatedAnnotation, property.Value.Annotation.ToString());
		}

		[Test]
		public void UpdateArgumentValue_ValidValue_ValueUpdatedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const int argumentValue = 10;
			const string updatedValue = "UpdatedValue";

			// Add the argument with the initial value
			var argument = node.AddArgument(argumentValue);

			// Act
			argument.SetValue(updatedValue);

			// Assert
			Assert.AreEqual(updatedValue, argument.GetValue<string>());
		}

		[Test]
		public void UpdatePropertyValue_ValidValue_ValueUpdatedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			const string propertyName = "Key";
			const string originalValue = "Value";
			const int updatedValue = 10;

			// Add the property with the initial value
			var property = node.AddProperty(propertyName, originalValue);

			// Act
			property.Value.SetValue(updatedValue);

			// Assert
			Assert.AreEqual(updatedValue, property.Value.GetValue<int>());
		}

		[Test]
		public void RemoveNode_ValidIndex_NodeRemovedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node1 = document.AddNode("Node1");
			var node2 = document.AddNode("Node2");
			var node3 = document.AddNode("Node3").WithAnnotation("Annotation");

			// Act
			document.RemoveNodes(1, 1);

			// Assert
			Assert.AreEqual(2, document.NodesCount);
			Assert.AreEqual("Node3", document[1].Name.ToString());
			Assert.AreEqual("Annotation", document[1].Annotation.ToString());
			Assert.IsTrue(node1.IsValid);
			Assert.IsFalse(node2.IsValid);
			Assert.IsFalse(node3.IsValid);
		}

		[Test]
		public void RemoveArgument_ValidIndex_ArgumentRemovedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			var argument1 = node.AddArgument("Argument1");
			var argument2 = node.AddArgument("Argument2");
			var argument3 = node.AddArgument("Argument3");

			// Act
			node.RemoveArguments(1, 1);

			// Assert
			Assert.AreEqual(2, node.ArgumentsCount);
			Assert.AreEqual("Argument3", node.GetArgument(1).GetValue<string>());
			Assert.IsTrue(argument1.IsValid);
			Assert.IsFalse(argument2.IsValid);
			Assert.IsFalse(argument3.IsValid);
		}

		[Test]
		public void RemoveProperty_ValidIndex_PropertyRemovedSuccessfully()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			var property1 = node.AddProperty("Key1", "Value1");
			var property2 = node.AddProperty("Key2", 2.0f);
			var property3 = node.AddProperty("Key3", 3);

			// Act
			node.RemoveProperties(1, 1);

			// Assert
			Assert.AreEqual(2, node.PropertiesCount);
			Assert.AreEqual("Key3", node.GetProperty(1).Key.ToString());
			Assert.AreEqual(3, node.GetProperty(1).Value.GetValue<int>());
			Assert.IsTrue(property1.IsValid);
			Assert.IsFalse(property2.IsValid);
			Assert.IsFalse(property3.IsValid);
		}

		[Test]
		public void NodesCount_ShouldReturnCorrectCount()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			document.AddNode("Node1");
			document.AddNode("Node2");
			document.AddNode("Node3");

			// Act
			var count = document.NodesCount;

			// Assert
			Assert.AreEqual(3, count);
		}

		[Test]
		public void ChildrenCount_ShouldReturnCorrectCount()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("ParentNode");
			node.AddNode("Child1");
			node.AddNode("Child2");
			node.AddNode("Child3");

			// Act
			var count = node.NodesCount;

			// Assert
			Assert.AreEqual(3, count);
		}

		[Test]
		public void GetChild_ShouldRetrieveCorrectChild()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Parent Node");
			const string expectedName = "Child";
			const string expectedAnnotation = "Annotation";
			node.AddNode(expectedName).WithAnnotation(expectedAnnotation);

			// Act
			var retrievedChild = node[0];

			// Assert
			Assert.AreEqual(expectedName, retrievedChild.GetNameString());
			Assert.AreEqual(expectedAnnotation, retrievedChild.GetAnnotationString());
		}

		[Test]
		public void RemoveChild_ShouldDecreaseChildrenCount()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Parent Node");
			node.AddNode("Child");

			// Act
			node.RemoveNodes(0, 1);

			// Assert
			Assert.AreEqual(0, node.NodesCount);
		}

		[Test]
		public void Clone_ShouldCreateDeepCopyOfDocumentObject()
		{
			// Arrange
			using var originalDocument = DocumentObjectModel.Create();
			originalDocument.AddNode("Node 1");

			// Act
			using var clonedDocument = originalDocument.Clone();

			// Assert
			// Verify that the clonedDocument is not the same instance as the originalDocument
			Assert.AreNotSame(originalDocument, clonedDocument);
	
			// Verify that the clonedDocument has the same number of nodes as the originalDocument
			Assert.AreEqual(originalDocument.NodesCount, clonedDocument.NodesCount);

			// Verify that modifications in the clonedDocument do not affect the originalDocument
			originalDocument.AddNode("Node 2");

			// Assert that the originalDocument remains unchanged
			Assert.AreNotEqual(originalDocument.NodesCount, clonedDocument.NodesCount);
		}

		[Test]
		public void Clone_ShouldCloneHierarchy()
		{
			// Arrange
			using var originalDocument = DocumentObjectModel.Create();
			var node1 = originalDocument.AddNode("Node 1");
			var node2 = node1.AddNode("Node 2").WithAnnotation("Annotation");

			// Act
			using var clonedDocument = originalDocument.Clone();

			// Assert
			// Verify that the clonedDocument has the same hierarchy as the originalDocument
			var clonedNode1 = clonedDocument[0];
			var clonedNode2 = clonedNode1[0];

			Assert.AreEqual(node1.Name.ToString(), clonedNode1.Name.ToString());
			Assert.AreEqual(node2.Name.ToString(), clonedNode2.Name.ToString());
			Assert.IsTrue(clonedNode1.Annotation.IsEmpty);
			Assert.AreEqual("Annotation", clonedNode2.Annotation.ToString());
		}

		[Test]
		public void Clone_ShouldCloneArguments()
		{
			// Arrange
			using var originalDocument = DocumentObjectModel.Create();

			// Add a node with arguments to the original document...
			var node = originalDocument.AddNode("Node");
			node.AddArgument(10).WithAnnotation("i32");
			node.AddArgument("Argument");

			// Act
			using var clonedDocument = originalDocument.Clone();

			// Assert
			// Verify that the clonedDocument has the same arguments as the originalDocument
			var clonedNode = clonedDocument[0];

			Assert.AreEqual(2, clonedNode.ArgumentsCount);

			var clonedValue1 = clonedNode.GetArgument(0);
			var clonedValue2 = clonedNode.GetArgument(1);

			Assert.AreEqual(10, clonedValue1.GetValue<int>());
			Assert.AreEqual("i32", clonedValue1.Annotation.ToString());
			Assert.AreEqual("Argument", clonedValue2.GetValue<string>());
		}

		[Test]
		public void Clone_ShouldCloneProperties()
		{
			// Arrange
			using var originalDocument = DocumentObjectModel.Create();

			// Add a node with properties to the original document...
			var node = originalDocument.AddNode("Node");
			node.AddProperty("Key1", 20);
			node.AddProperty("Key2", "Property").WithAnnotation("string");

			// Act
			using var clonedDocument = originalDocument.Clone();

			// Assert
			// Verify that the clonedDocument has the same properties as the originalDocument
			var clonedNode = clonedDocument[0];

			Assert.AreEqual(2, clonedNode.PropertiesCount);

			var clonedProperty1 = clonedNode.GetProperty(0);
			var clonedProperty2 = clonedNode.GetProperty(1);

			Assert.AreEqual("Key1", clonedProperty1.Key.ToString());
			Assert.AreEqual(20, clonedProperty1.Value.GetValue<int>());
			Assert.AreEqual("Key2", clonedProperty2.Key.ToString());
			Assert.AreEqual("Property", clonedProperty2.Value.GetValue<string>());
			Assert.AreEqual("string", clonedProperty2.Value.Annotation.ToString());
		}

		[Test]
		public void Clone_ShouldCloneHierarchyArgumentsAndProperties()
		{
			// Arrange
			using var originalDocument = DocumentObjectModel.Create();

			// Add nodes with arguments and properties to the original document, forming a hierarchy...
			var node1 = originalDocument.AddNode("Node 1");
			var node2 = node1
				.AddNode("Node 2")
				.WithAnnotation("Annotation");

			node1.AddArgument(10).WithAnnotation("i32");
			node2.AddArgument("Argument");

			node1.AddProperty("Key1", true);
			node2.AddProperty("Key2", 20.0f).WithAnnotation("f32");

			// Act
			using var clonedDocument = originalDocument.Clone();

			// Assert
			// Verify that the clonedDocument has the same hierarchy, arguments, and properties as the originalDocument
			var clonedNode1 = clonedDocument[0];
			var clonedNode2 = clonedNode1[0];

			Assert.AreEqual("Node 1", clonedNode1.GetNameString());
			Assert.AreEqual("Node 2", clonedNode2.GetNameString());
			Assert.AreEqual("Annotation", clonedNode2.GetAnnotationString());

			Assert.AreEqual(1, clonedNode1.ArgumentsCount);
			Assert.AreEqual(1, clonedNode2.ArgumentsCount);

			var clonedArgument1 = clonedNode1.GetArgument(0);
			var clonedArgument2 = clonedNode2.GetArgument(0);

			Assert.AreEqual(10, clonedArgument1.GetValue<int>());
			Assert.AreEqual("i32", clonedArgument1.Annotation.ToString());
			Assert.AreEqual("Argument", clonedArgument2.GetValue<string>());

			Assert.AreEqual(1, clonedNode1.PropertiesCount);
			Assert.AreEqual(1, clonedNode2.PropertiesCount);

			var clonedProperty1 = clonedNode1.GetProperty(0);
			var clonedProperty2 = clonedNode2.GetProperty(0);

			Assert.AreEqual("Key1", clonedProperty1.Key.ToString());
			Assert.AreEqual(true, clonedProperty1.Value.GetValue<bool>());
			Assert.AreEqual("Key2", clonedProperty2.Key.ToString());
			Assert.AreEqual(20.0f, clonedProperty2.Value.GetValue<float>());
			Assert.AreEqual("f32", clonedProperty2.Value.Annotation.ToString());
		}

		[Test]
		public void Value_Equals_ReturnsTrue_WhenValuesAreEqual()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			var value1 = node.AddArgument("Hello");
			var value2 = node.AddArgument("Hello");

			// Act
			var result = value1.Equals(value2);

			// Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void Value_Equals_ReturnsFalse_WhenValuesAreNotEqual()
		{
			// Arrange
			using var document = DocumentObjectModel.Create();
			var node = document.AddNode("Node");
			var value1 = node.AddArgument("Hello");
			var value2 = node.AddArgument("World");

			// Act
			var result = value1.Equals(value2);

			// Assert
			Assert.IsFalse(result);
		}
	}
}

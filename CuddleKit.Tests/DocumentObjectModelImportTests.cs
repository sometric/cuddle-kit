using NUnit.Framework;

namespace CuddleKit.Tests
{
	using ObjectModel;

	[TestFixture]
	public class DocumentObjectModelImportTests
	{
		[Test]
		public void Document_DefaultValues_Invalid()
		{
			// Arrange
			using var model = DocumentObjectModel.Create();
			var node = model.AddNode("Node1");
			node.AddArgument(12).WithAnnotation("i32");
			node.AddArgument(1.0f);//.WithAnnotation("f32");

			// Act
			using var document = model.Export();
			using var model2 = document.ImportModel();

			// Assert
			Assert.AreEqual(2, document.GetArguments(document.Nodes[0]).Length);
			Assert.AreEqual("Node1", model2[0].Name.ToString());
			Assert.AreEqual(typeof(int), model2[0].GetArgument(0).Type);
			Assert.AreEqual(12, model2[0].GetArgument(0).GetValue<int>());
			Assert.AreEqual(typeof(float), model2[0].GetArgument(1).Type);
			Assert.AreEqual(1.0f, model2[0].GetArgument(1).GetValue<float>());
		}
	}
}

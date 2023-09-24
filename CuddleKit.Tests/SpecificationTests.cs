using System.IO;
using System.Text;
using CuddleKit.Serialization;
using NUnit.Framework;

namespace CuddleKit.Tests
{
	public class SpecificationTests
	{
		[Test]
		public void TestExpectations()
		{
			var inputs = ManifestResources.GetResourceMap("SpecificationInput.");
			var outputs = ManifestResources.GetResourceMap("SpecificationOutput.");

			foreach (var (name, inputPath) in inputs)
			{
				using var stream = ManifestResources.GetResourceStream(inputPath);
				Assert.NotNull(stream);

				using var reader = new StreamReader(stream);
				var data = reader.ReadToEnd();

				if (!outputs.TryGetValue(name, out var outputPath))
				{
					Assert.Throws<DeserializationException>(() =>
					{
						using var _ = Document.Deserialize(data);
					});

					continue;
				}

				using var document = Document.Deserialize(data);
				var stringBuilder = new StringBuilder();
				document.Write(stringBuilder);

				using var outputStream = ManifestResources.GetResourceStream(outputPath);
				Assert.NotNull(outputStream);

				using var outputReader = new StreamReader(outputStream);
				var expectedKdl = outputReader.ReadToEnd();
				Assert.AreEqual(expectedKdl, stringBuilder.ToString(), $"Wrong output: {name}");
			}
		}
	}
}

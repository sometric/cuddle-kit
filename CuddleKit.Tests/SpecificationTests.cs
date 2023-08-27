using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			var assembly = Assembly.GetExecutingAssembly();
			var names = assembly.GetManifestResourceNames();

			var inputs = GetCasesMap($"{assembly.GetName().Name}.Specification.Input.");
			var outputs = GetCasesMap($"{assembly.GetName().Name}.Specification.Output.");

			foreach (var (name, resourcePath) in inputs)
			{
				using var stream = assembly.GetManifestResourceStream(resourcePath);
				Assert.NotNull(stream);

				using var reader = new StreamReader(stream);
				var data = reader.ReadToEnd();

				if (!outputs.TryGetValue(name, out var output))
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

				using var outputStream = assembly.GetManifestResourceStream(output);
				Assert.NotNull(outputStream);

				using var outputReader = new StreamReader(outputStream);
				var expectedKdl = outputReader.ReadToEnd();
				Assert.AreEqual(expectedKdl, stringBuilder.ToString(), $"Wrong output: {name}");
			}

			return;

			Dictionary<string, string> GetCasesMap(string prefix) => names
				.Where(name => name.StartsWith(prefix))
				.ToDictionary(name => name.Substring(prefix.Length));
		}
	}
}

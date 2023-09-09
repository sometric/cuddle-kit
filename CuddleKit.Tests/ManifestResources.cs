using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CuddleKit.Tests
{
	internal static class ManifestResources
	{
		private static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
		private static readonly string[] AllResourceNames = Assembly.GetManifestResourceNames();

		public static Dictionary<string, string> GetResourceMap(string resourcePrefix)
		{
			var fullPrefix = $"{Assembly.GetName().Name}.{resourcePrefix}";
			return AllResourceNames
				.Where(name => name.StartsWith(fullPrefix))
				.ToDictionary(name => name.Substring(fullPrefix.Length));
		}

		public static Stream GetResourceStream(string resourcePath) =>
			Assembly.GetManifestResourceStream(resourcePath);
	}
}
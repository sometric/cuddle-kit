using System;
using CuddleKit.Reflection.Export;

namespace CuddleKit.Reflection.Dynamic
{
	internal sealed class DynamicObjectResolver : IMemberResolver
	{
		public static readonly IMemberResolver Shared = new DynamicObjectResolver();

		IMemberExporter IMemberResolver.ResolveExporter(Type type) =>
			(IMemberExporter) Activator.CreateInstance(typeof(GenericObjectExporter<>).MakeGenericType(type));
	}
}

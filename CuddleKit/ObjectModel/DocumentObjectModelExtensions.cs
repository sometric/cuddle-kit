using System.Runtime.CompilerServices;

namespace CuddleKit.ObjectModel
{
	public static class DocumentObjectModelExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveNode(this DocumentObjectModel model, int index) =>
			model.RemoveNodes(index, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Clear(this DocumentObjectModel model) =>
			model.RemoveNodes(0, model.NodesCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveNode(this in DocumentNode node, int index) =>
			node.RemoveNodes(index, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ClearNodes(this in DocumentNode node) =>
			node.RemoveNodes(0, node.NodesCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveArgument(this in DocumentNode node, int index) =>
			node.RemoveArguments(index, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ClearArguments(this in DocumentNode node) =>
			node.RemoveArguments(0, node.ArgumentsCount);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveProperty(this in DocumentNode node, int index) =>
			node.RemoveProperties(index, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ClearProperties(this in DocumentNode node) =>
			node.RemoveProperties(0, node.PropertiesCount);

		public static DocumentNode GetNode(this DocumentObjectModel model, int index) =>
			model[index];

		public static DocumentNode GetNode(this DocumentObjectModel model, int index1, int index2) =>
			model[index1][index2];
	}
}

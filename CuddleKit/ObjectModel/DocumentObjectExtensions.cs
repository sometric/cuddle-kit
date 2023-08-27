using System;

namespace CuddleKit.ObjectModel
{
	public static class DocumentObjectExtensions
	{
		public static ref readonly DocumentNode WithAnnotation(this in DocumentNode node,
			ReadOnlySpan<char> annotation)
		{
			node.Annotation = annotation;
			return ref node;
		}

		public static ref readonly DocumentValue WithAnnotation(this in DocumentValue value,
			ReadOnlySpan<char> annotation)
		{
			value.Annotation = annotation;
			return ref value;
		}

		public static ref readonly DocumentProperty WithAnnotation(this in DocumentProperty property,
			ReadOnlySpan<char> annotation)
		{
			property.Annotation = annotation;
			return ref property;
		}

		public static ref readonly DocumentNode WithArgument<T>(this in DocumentNode node, T value)
		{
			node.AddArgument(value);
			return ref node;
		}

		public static ref readonly DocumentNode WithArgument<T>(this in DocumentNode node,
			T value,
			ReadOnlySpan<char> annotation)
		{
			node.AddArgument(value).WithAnnotation(annotation);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2>(this in DocumentNode node,
			T1 value1,
			T2 value2)
		{
			node.AddArgument(value1);
			node.AddArgument(value2);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			ReadOnlySpan<char> annotation)
		{
			node.AddArgument(value1).WithAnnotation(annotation);
			node.AddArgument(value2).WithAnnotation(annotation);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2, T3>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			T3 value3)
		{
			node.AddArgument(value1);
			node.AddArgument(value2);
			node.AddArgument(value3);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2, T3>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			T3 value3,
			ReadOnlySpan<char> annotation)
		{
			node.AddArgument(value1).WithAnnotation(annotation);
			node.AddArgument(value2).WithAnnotation(annotation);
			node.AddArgument(value3).WithAnnotation(annotation);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2, T3, T4>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			T3 value3,
			T4 value4)
		{
			node.AddArgument(value1);
			node.AddArgument(value2);
			node.AddArgument(value3);
			node.AddArgument(value4);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2, T3, T4>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			T3 value3,
			T4 value4,
			ReadOnlySpan<char> annotation)
		{
			node.AddArgument(value1).WithAnnotation(annotation);
			node.AddArgument(value2).WithAnnotation(annotation);
			node.AddArgument(value3).WithAnnotation(annotation);
			node.AddArgument(value4).WithAnnotation(annotation);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2, T3, T4, T5>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			T3 value3,
			T4 value4,
			T5 value5)
		{
			node.AddArgument(value1);
			node.AddArgument(value2);
			node.AddArgument(value3);
			node.AddArgument(value4);
			node.AddArgument(value5);
			return ref node;
		}

		public static ref readonly DocumentNode WithArguments<T1, T2, T3, T4, T5>(this in DocumentNode node,
			T1 value1,
			T2 value2,
			T3 value3,
			T4 value4,
			T5 value5,
			ReadOnlySpan<char> annotation)
		{
			node.AddArgument(value1).WithAnnotation(annotation);
			node.AddArgument(value2).WithAnnotation(annotation);
			node.AddArgument(value3).WithAnnotation(annotation);
			node.AddArgument(value4).WithAnnotation(annotation);
			node.AddArgument(value5).WithAnnotation(annotation);
			return ref node;
		}

		public static ref readonly DocumentNode WithProperty<T>(this in DocumentNode node,
			string key,
			T value)
		{
			node.AddProperty(key, value);
			return ref node;
		}

		public static ref readonly DocumentNode WithProperty<T>(this in DocumentNode node,
			string key,
			T value,
			ReadOnlySpan<char> annotation)
		{
			node.AddProperty(key, value).Value.WithAnnotation(annotation);
			return ref node;
		}

		public static string GetNameString(this in DocumentNode node) =>
			node.Name.ToString();

		public static string GetAnnotationString(this in DocumentNode node)
		{
			var annotation = node.Annotation;
			return !annotation.IsEmpty ? annotation.ToString() : null;
		}

		public static bool TryGetValue<T>(this in DocumentValue value, out T result)
		{
			var success = value.Type == typeof(T);
			result = success ? value.GetValue<T>() : default;
			return success;
		}
	}
}

using CuddleKit.Reflection.Description;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Export
{
	public abstract class GenericDictionaryExporter<TDictionary, TKey, TValue> : IDictionaryExporter
	{
		System.Type IDictionaryExporter.KeyType =>
			typeof(TKey);

		System.Type IDictionaryExporter.ValueType =>
			typeof(TValue);

		void IDictionaryExporter.Export<TInstance, TVisitor>(
			MemberDescriptor descriptor,
			in TInstance instance,
			ref TVisitor visitor,
			ref Document document)
		{
			var array = descriptor.GetValue<TInstance, TDictionary>(instance);
			Iterate(array, ref visitor, ref document);
		}

		protected abstract void Iterate<TVisitor>(TDictionary dictionary, ref TVisitor visitor, ref Document document)
			where TVisitor : struct, IKeyValueVisitor;
	}
}

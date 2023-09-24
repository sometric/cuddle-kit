using System;
using System.Collections.Generic;
using CuddleKit.Format;
using CuddleKit.Reflection.Utility;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Serialization
{
	public interface ITypeResolver
	{
		Type Resolve(Type staticType, object actualValue);
	}

	public sealed class DynamicTypeResolver : ITypeResolver
	{
		public static readonly DynamicTypeResolver Shared = new();

		Type ITypeResolver.Resolve(Type staticType, object actualValue) =>
			actualValue?.GetType() ?? staticType;
	}

	public sealed class StaticTypeResolver : ITypeResolver
	{
		public static readonly StaticTypeResolver Shared = new();

		Type ITypeResolver.Resolve(Type staticType, object actualValue) =>
			staticType;
	}

	public class Serializer : IDisposable
	{
		private const MemberAccess MemberAccessMask =
			MemberAccess.Public | MemberAccess.NonPublic | MemberAccess.ReadOnly;

		private readonly TypeCache _typeCache;
		private FormatterRegistry _registry;
		private readonly SerializationSettings _settings;

		public Serializer(in SerializationSettings settings)
		{
			_typeCache = new TypeCache(settings.ReflectionPolicy, MemberAccessMask);
			_registry = new FormatterRegistry(settings.Formatters);
			_settings = settings;
		}

		public void Dispose() =>
			_registry.Dispose();

		public NodeReference Serialize<T>(in T instance, ReadOnlySpan<char> name, ref Document document)
		{
			var nameToken = document.AllocateString(name);
			return WriteInstance(instance, nameToken, default, ref document);
		}

		private NodeReference WriteInstance<TInstance>(
			in TInstance instance,
			TokenReference instanceNameToken,
			NodeReference parentReference,
			ref Document document)
		{
			var typeDescriptor = _typeCache.GetTypeDescriptor(
				instance.GetType(),
				_settings.MemberAccessMask,
				_settings.MemberKindMask);

			// settings
			// annotate types
			// member style

			var instanceNode = instanceNameToken.IsValid
				? document.AddNode(parentReference, instanceNameToken)
				: default;

			//if (objectNode.IsValid)
			//	document.Annotate(objectNode, descriptor.Type.FullName);
			//var memberAccessFilter = _settings.MemberAccessFilter;
			//var ignorePrivate = (memberAccessFilter & MemberAccessFilter.IgnorePrivate) == MemberAccessFilter.IgnorePrivate;
			//var ignorePublic = (memberAccessFilter & MemberAccessFilter.IgnorePublic) == MemberAccessFilter.IgnorePublic;
			//var ignoreReadonly = (memberAccessFilter & MemberAccessFilter.IgnoreReadonly) == MemberAccessFilter.IgnoreReadonly;

			foreach (var memberDescriptor in typeDescriptor.Members)
			{
				//if ((memberAccessFilter & memberDescriptor.ReadAccess) == 0)
				//	continue;

				//_settings.
				//var access = memberDescriptor.MatchFilter(_settings.MemberAccessFilter, _settings.PropertyFilter);
				//_settings.FieldFilter

				var memberName = memberDescriptor.Name
					.AsSpan()
					.SkipPrefixes(_settings.MemberPrefixes);

				var nameToken = _settings.NamingConvention.Write(memberName, ref document);
				var boundMember = new BoundMember<TInstance>(instance, memberDescriptor, "");

				var formatter = _registry.Lookup(memberDescriptor.Type, default);
				if (formatter != null)
				{
					var valueReference = formatter.Export(ref boundMember, ref document);

					switch (memberDescriptor.Style)
					{
						case MemberStyle.Property when instanceNode.IsValid:
							document.SetProperty(instanceNode, nameToken, valueReference);
							break;

						case MemberStyle.Argument when instanceNode.IsValid:
							document.AddArgument(instanceNode, valueReference);
							break;

						default:
							var memberNode = document.AddNode(instanceNode, nameToken);
							document.AddArgument(memberNode, valueReference);
							break;
					}

					continue;
				}

				switch (boundMember.ResolveCategory(out var typeArguments))
				{
					case MemberCategory.Object:
						var objectVisitor = new ObjectEntryVisitor(this, instanceNode, nameToken);

						_settings.ReflectionPolicy
							.VisitObject(boundMember, ref objectVisitor, ref document);

						break;

					case MemberCategory.Array:
						var arrayNode = document.AddNode(instanceNode, nameToken);
						var arrayVisitor = new ArrayElementVisitor(this, arrayNode, typeArguments[0]);

						_settings.ReflectionPolicy
							.VisitArray(boundMember, ref arrayVisitor, ref document, typeArguments);

						break;

					case MemberCategory.Dictionary:
						var dictionaryNode = document.AddNode(instanceNode, nameToken);
						var dictionaryVisitor = new DictionaryEntryVisitor(this, dictionaryNode, typeArguments);

						_settings.ReflectionPolicy
							.VisitDictionary(boundMember, ref dictionaryVisitor, ref document, typeArguments);

						break;
				}
			}

			return instanceNode;
		}

		private readonly struct ObjectEntryVisitor : IValueVisitor
		{
			private readonly Serializer _serializer;
			private readonly NodeReference _instanceNode;
			private readonly TokenReference _nameToken;

			public ObjectEntryVisitor(Serializer serializer, NodeReference instanceNode, TokenReference nameToken)
			{
				_serializer = serializer;
				_instanceNode = instanceNode;
				_nameToken = nameToken;
			}

			void IValueVisitor.Visit<TValue>(in TValue entry, ref Document document) =>
				_serializer.WriteInstance(entry, _nameToken, _instanceNode, ref document);
		}

		private readonly struct ArrayElementVisitor : IValueVisitor
		{
			private readonly Serializer _serializer;
			private readonly NodeReference _arrayNode;
			private readonly IFormatter _declaredElementFormatter;

			public ArrayElementVisitor(Serializer serializer, NodeReference arrayNode, Type declaredElementType)
			{
				_serializer = serializer;
				_arrayNode = arrayNode;

				_declaredElementFormatter = serializer._registry
					.Lookup(declaredElementType, default);
			}

			void IValueVisitor.Visit<TValue>(in TValue entry, ref Document document)
			{
				var elementFormatter = _declaredElementFormatter ??
					_serializer._registry
						.Lookup(entry.GetType(), default);

				var entryNameToken = document.AllocateString("-");

				if (elementFormatter != null)
				{
					var proxy = new ValueExportProxy<TValue>(entry);
					var elementReference = elementFormatter.Export(ref proxy, ref document);

					document.AddArgument(_arrayNode, elementReference);

					//var elementNode = document.AddNode(collectionNode, "-");
					//document.AddArgument(elementNode, elementReference);
				}
				else
				{
					_serializer.WriteInstance(entry, entryNameToken, _arrayNode, ref document);
				}
			}
		}

		private readonly struct DictionaryEntryVisitor : IKeyValueVisitor
		{
			private readonly Serializer _serializer;
			private readonly NodeReference _dictionaryNode;
			private readonly IFormatter _keyFormatter;
			private readonly IFormatter _declaredValueFormatter;

			public DictionaryEntryVisitor(Serializer serializer, NodeReference dictionaryNode, IReadOnlyList<Type> typeArguments)
			{
				_serializer = serializer;
				_dictionaryNode = dictionaryNode;

				_keyFormatter =
					serializer._registry.Lookup(typeArguments[0], default)
					?? throw new InvalidOperationException("Key should be a value");

				_declaredValueFormatter =
					serializer._registry.Lookup(typeArguments[1], default);
			}

			void IKeyValueVisitor.Visit<TKey, TValue>(in TKey key, in TValue value, ref Document document)
			{
				NodeReference entryNode;

				var keyProxy = new ValueExportProxy<TKey>(key);
				var keyReference = _keyFormatter.Export(ref keyProxy, ref document);
				var entryNameToken = document.AllocateString("-");

				if (_declaredValueFormatter != null)
				{
					var valueProxy = new ValueExportProxy<TValue>(value);
					var valueReference = _declaredValueFormatter.Export(ref valueProxy, ref document);

					entryNode = document.AddNode(_dictionaryNode, entryNameToken);
					document.AddArgument(entryNode, keyReference);
					document.AddArgument(entryNode, valueReference);
				}
				else
				{
					entryNode = _serializer.WriteInstance(value, entryNameToken, _dictionaryNode, ref document);
					document.AddArgument(entryNode, keyReference);
				}
			}
		}

		private readonly struct ValueExportProxy<TOriginalValue> : IFormatterExportProxy
		{
			private readonly TOriginalValue _instance;

			public ValueExportProxy(in TOriginalValue instance) =>
				_instance = instance;

			ReadOnlySpan<char> IFormatterExportProxy.Annotation =>
				default;

			TValue IFormatterExportProxy.Export<TValue>() =>
				_instance is TValue value
					? value
					: throw new InvalidCastException("tbd");
		}
	}
}

using System;
using CuddleKit.Format;
using CuddleKit.Reflection.Description;
using CuddleKit.Reflection.Export;
using CuddleKit.Reflection.Naming;
using CuddleKit.Reflection.Utility;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Serialization
{
	public enum ValueTypeResolution
	{
		Actual,
		Declared
	}

	public class Serializer : IDisposable
	{
		private const MemberAccess MemberAccessMask =
			MemberAccess.Public | MemberAccess.NonPublic | MemberAccess.ReadOnly;

		private readonly INamingConvention _namingConvention;
		private readonly string[] _memberPrefixes;
		private readonly MemberKind _memberKindMask;
		private readonly MemberAccess _memberAccessMask;
		private readonly ValueTypeResolution _valueTypeResolution;
		private readonly TypeCache _typeCache;
		private FormatterRegistry _registry;

		public Serializer(in SerializationSettings settings)
		{
			_namingConvention = settings.NamingConvention;
			_memberPrefixes = settings.MemberPrefixes ?? Array.Empty<string>();
			_memberKindMask = settings.MemberKindMask;
			_memberAccessMask = settings.MemberAccessMask;
			_valueTypeResolution = settings.ValueTypeResolution;
			_typeCache = new TypeCache(settings.ReflectionPolicy, settings.CustomResolvers, MemberAccessMask);
			_registry = new FormatterRegistry(settings.Formatters);
		}

		public void Dispose() =>
			_registry.Dispose();

		public NodeReference Serialize<T>(in T instance, ReadOnlySpan<char> name, ref Document document)
		{
			var nameToken = document.AllocateString(name);
			return WriteInstance(instance, typeof(T), nameToken, default, ref document);
		}

		private NodeReference WriteInstance<TInstance>(
			in TInstance instance,
			Type instanceType,
			TokenReference instanceNameToken,
			NodeReference parentReference,
			ref Document document)
		{
			var typeDescriptor = _typeCache.GetTypeDescriptor(
				instanceType,
				_memberAccessMask,
				_memberKindMask);

			// settings
			// annotate types
			// member style

			var instanceNode = instanceNameToken.IsValid
				? document.AddNode(parentReference, instanceNameToken)
				: default;

			foreach (var memberDescriptor in typeDescriptor.Members)
			{
				var memberName = memberDescriptor.Name
					.AsSpan()
					.SkipPrefixes(_memberPrefixes);

				using var nameAllocation = _namingConvention.Apply(memberName, out memberName);
				var nameToken = document.AllocateString(memberName);
				var memberType = memberDescriptor.Type;

				var formatter = _registry.Lookup(memberType, default);
				if (formatter != null)
				{
					var boundMember = new MemberExportProxy<TInstance>(instance, memberDescriptor, "");
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

				switch (_typeCache.GetTypeExporter(memberDescriptor.Type))
				{
					case IObjectExporter objectExporter:
						var objectVisitor = new ObjectEntryVisitor(
							this,
							instanceNode,
							nameToken,
							memberDescriptor.Type);

						objectExporter.Export(memberDescriptor, instance, ref objectVisitor, ref document);
						break;

					case IArrayExporter arrayExporter:
						var arrayNode = document.AddNode(instanceNode, nameToken);
						var arrayVisitor = new ArrayElementVisitor(
							this,
							arrayNode,
							arrayExporter.ElementType);

						arrayExporter.Export(memberDescriptor, instance, ref arrayVisitor, ref document);
						break;

					case IDictionaryExporter dictionaryExporter
						when _registry.Lookup(dictionaryExporter.KeyType, default) is {} keyFormatter:
						var dictionaryNode = document.AddNode(instanceNode, nameToken);
						var dictionaryVisitor = new DictionaryEntryVisitor(
							this,
							dictionaryNode,
							keyFormatter,
							dictionaryExporter.ValueType);

						dictionaryExporter.Export(memberDescriptor, instance, ref dictionaryVisitor, ref document);
						break;
				}
			}

			return instanceNode;
		}

		private Type ResolveInstanceType<TInstance>(in TInstance instance, Type declaredType) =>
			_valueTypeResolution switch
			{
				ValueTypeResolution.Actual when instance is not null => instance.GetType(),
				_ => declaredType
			};

		private readonly struct MemberExportProxy<TInstance> : IFormatterExportProxy
		{
			private readonly TInstance _instance;
			private readonly MemberDescriptor _descriptor;
			private readonly string _annotation;

			internal MemberExportProxy(in TInstance instance, MemberDescriptor descriptor, string annotation)
			{
				_instance = instance;
				_descriptor = descriptor;
				_annotation = annotation;
			}

			ReadOnlySpan<char> IFormatterExportProxy.Annotation =>
				_annotation;

			TValue IFormatterExportProxy.Export<TValue>() =>
				_descriptor.GetValue<TInstance, TValue>(_instance);
		}

		private readonly struct ObjectEntryVisitor : IValueVisitor
		{
			private readonly Serializer _serializer;
			private readonly NodeReference _instanceNode;
			private readonly TokenReference _nameToken;
			private readonly Type _declaredType;

			public ObjectEntryVisitor(
				Serializer serializer,
				NodeReference instanceNode,
				TokenReference nameToken,
				Type declaredType)
			{
				_serializer = serializer;
				_instanceNode = instanceNode;
				_nameToken = nameToken;
				_declaredType = declaredType;
			}

			void IValueVisitor.Visit<TValue>(in TValue instance, ref Document document)
			{
				var instanceType = _serializer.ResolveInstanceType(instance, _declaredType);
				_serializer.WriteInstance(instance, instanceType, _nameToken, _instanceNode, ref document);
			}
		}

		private readonly struct ArrayElementVisitor : IValueVisitor
		{
			private readonly Serializer _serializer;
			private readonly NodeReference _arrayNode;
			private readonly Type _declaredElementType;
			private readonly IFormatter _declaredElementFormatter;

			public ArrayElementVisitor(Serializer serializer, NodeReference arrayNode, Type declaredElementType)
			{
				_serializer = serializer;
				_arrayNode = arrayNode;
				_declaredElementType = declaredElementType;

				_declaredElementFormatter = serializer._registry
					.Lookup(declaredElementType, default);
			}

			void IValueVisitor.Visit<TValue>(in TValue element, ref Document document)
			{
				var elementFormatter = _declaredElementFormatter ??
					_serializer._registry
						.Lookup(element.GetType(), default);

				var entryNameToken = document.AllocateString("-");

				if (elementFormatter != null)
				{
					var proxy = new ValueExportProxy<TValue>(element);
					var elementReference = elementFormatter.Export(ref proxy, ref document);

					document.AddArgument(_arrayNode, elementReference);

					//var elementNode = document.AddNode(collectionNode, "-");
					//document.AddArgument(elementNode, elementReference);
				}
				else
				{
					var elementType = _serializer.ResolveInstanceType(element, _declaredElementType);
					_serializer.WriteInstance(element, elementType, entryNameToken, _arrayNode, ref document);
				}
			}
		}

		private readonly struct DictionaryEntryVisitor : IKeyValueVisitor
		{
			private readonly Serializer _serializer;
			private readonly NodeReference _dictionaryNode;
			private readonly Type _declaredValueType;
			private readonly IFormatter _keyFormatter;
			private readonly IFormatter _declaredValueFormatter;

			public DictionaryEntryVisitor(
				Serializer serializer,
				NodeReference dictionaryNode,
				IFormatter keyFormatter,
				Type declaredValueType)
			{
				_serializer = serializer;
				_dictionaryNode = dictionaryNode;
				_declaredValueType = declaredValueType;
				_keyFormatter = keyFormatter;

				_declaredValueFormatter =
					serializer._registry.Lookup(declaredValueType, default);
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
					var valueType = _serializer.ResolveInstanceType(value, _declaredValueType);
					entryNode = _serializer.WriteInstance(value, valueType, entryNameToken, _dictionaryNode, ref document);
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

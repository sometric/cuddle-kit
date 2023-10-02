using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using CuddleKit.Reflection;
using CuddleKit.Reflection.Dynamic;
using CuddleKit.Reflection.Naming;
using CuddleKit.Reflection.Serialization;
using CuddleKit.Reflection.Portable;
using CuddleKit.Serialization;
using NUnit.Framework;

namespace CuddleKit.Tests
{
	[TestFixture]
	public class ReflectionTests
	{
		//[CuddleObject(LispCaseNamingConvention.Shared)]
		public struct AnotherStruct
		{
			public float FloatField;
			public string SomeSting;
			public bool SomeBool { get; set; }
		}

		public class SomeClass
		{
			public float ClassMember = -123.22f;
		}

		public struct SerializedStruct
		{
			public SomeClass Instance;
			public int IntegerField;
			public bool BoolField;
			private int _privateIntegerField;

			public AnotherStruct ComplexField;
			public IDictionary<string, object> Map;

			public bool BooleanProperty { get; set; }

			public IReadOnlyList<string> Collection;
			public IReadOnlyList<AnotherStruct> Collection2;
//			[System.NonSerialized]


			//public bool BooleanProperty1 { get; private set; } // yes, no
			//public bool BooleanProperty2 { get; } // yes, no
			//private bool BooleanProperty3 { get; set; } // no, no
			//public bool BooleanProperty4 { set {} } // yes, no
		}

		private struct MemberAccessTest
		{
			public int PublicField;
			public readonly int PublicReadonlyField;
			private int PrivateField;
			private readonly int PrivateReadonlyField;
			public int PublicPropertyPublicGetPublicSet { get; set; }
			public int PublicPropertyPublicGetPrivateSet { get; private set; }
			public int PublicPropertyPrivateGetPublicSet { private get; set; }
			public int PublicPropertyPublicGetNoSet { get; }
			public int PublicPropertyNoGetPublicSet { set {} }
			private int PrivatePropertyPublicGetPublicSet { get; set; }
			private int PrivatePropertyPublicGetNoSet { get; }
			private int PrivatePropertyNoGetPublicSet { set {} }
		}

		private interface IContact
		{
			string Name { get; }
			string PhoneNumber { get; }
		}

		private class CompanyContact : IContact
		{
			public string Name { get; set; }
			public string PhoneNumber { get; set; }
			public string RepresentativeName { get; set; }
		}

		private class Phonebook
		{
			public IContact[] Contacts =
			{
				new CompanyContact
				{
					Name = "Oz-Ware",
					PhoneNumber = "123456789",
					RepresentativeName = "John Smith"
				}
			};
		}

		[Test]
		public void Document_DefaultValues_Invalid()
		{
			// Arrange
			using var serializer = SerializerBuilder.Create()
				.WithReflectionPolicy(DynamicReflectionPolicy.Shared)
				.WithNamingConvention(LispCaseNamingConvention.Shared)
				.WithMemberAccessMask(MemberAccess.Public | MemberAccess.NonPublic)
				.Build();

			using var test1 = serializer.Serialize(new MemberAccessTest(), "-");

			var value = new SerializedStruct
			{
				Instance = new SomeClass(),
				IntegerField = 12,
				BoolField = false,
				BooleanProperty = true,
				Collection = new []{ "a", "b", "c" },
				Collection2 = new AnotherStruct[]{ default, default },
				Map = new Dictionary<string, object>
				{
					["first"] = new AnotherStruct { FloatField = -127.3f },
					["second"] = new AnotherStruct { FloatField = 1.0e3f }
				}
			};

			// Act
			using var document = serializer.Serialize(value, "root");
			using var document2 = serializer.Serialize(value, "root");
			var stringBuilder = new StringBuilder();
			document.Write(stringBuilder);
			document2.Write(stringBuilder);

			var serialized = stringBuilder.ToString();

			// Assert
			// Assert.AreEqual(2, document.GetArguments(document.Nodes[0]).Length);
			// Assert.AreEqual("Node1", model2[0].Name.ToString());
			// Assert.AreEqual(typeof(int), model2[0].GetArgument(0).Type);
			// Assert.AreEqual(12, model2[0].GetArgument(0).GetValue<int>());
			// Assert.AreEqual(typeof(float), model2[0].GetArgument(1).Type);
			// Assert.AreEqual(1.0f, model2[0].GetArgument(1).GetValue<float>());
		}

		[Test]
		public void Reflection_Serialize_MemberAccess()
		{
			const string casePrefix = "Reflection.serialize_member_access_";
			var cases = ManifestResources.GetResourceMap(casePrefix);

			var builder = SerializerBuilder.Create()
				.WithReflectionPolicy(DynamicReflectionPolicy.Shared)
				.WithMemberAccessMask(MemberAccess.Public | MemberAccess.NonPublic);

			var instance = new MemberAccessTest();
			var filters = new[]
			{
				MemberAccess.Public,
				MemberAccess.Public | MemberAccess.NonPublic,
				MemberAccess.Public | MemberAccess.ReadOnly,
				MemberAccess.Public | MemberAccess.NonPublic | MemberAccess.ReadOnly,
				MemberAccess.NonPublic,
				MemberAccess.NonPublic | MemberAccess.ReadOnly,
				MemberAccess.ReadOnly,
				// should be the same as MemberAccess.Public, ignore MemberAccess.WriteOnly for serialization
				MemberAccess.Public | MemberAccess.WriteOnly
			};

			foreach (var filter in filters)
			{
				using var serializer = builder
					.WithMemberAccessMask(filter)
					.Build();

				using var document = serializer.Serialize(instance, "-");
				var stringBuilder = new StringBuilder();
				document.Write(stringBuilder);

				var caseName = filter.ToString().ToLower().Replace(", ", "_") + ".kdl";

				using var outputStream = ManifestResources.GetResourceStream(cases[caseName]);
				Assert.NotNull(outputStream);

				using var outputReader = new StreamReader(outputStream);
				var expectedKdl = outputReader.ReadToEnd();

				Assert.AreEqual(expectedKdl, stringBuilder.ToString(), $"Wrong output: {casePrefix}{caseName}");
			}
		}

		[Test]
		public void Reflection_Serialize_ValueTypeResolution()
		{
			using var serializer = SerializerBuilder.Create()
				.WithReflectionPolicy(DynamicReflectionPolicy.Shared)
				.WithMemberAccessMask(MemberAccess.Public | MemberAccess.NonPublic | MemberAccess.ReadOnly)
				.WithValueTypeResolution(ValueTypeResolution.Actual)
				.Build();

			using var document = serializer.Serialize(new Phonebook(), "phonebook");

			var stringBuilder = new StringBuilder();
			document.Write(stringBuilder);

			var serialized = stringBuilder.ToString();
			int x = 12;
		}
	}
}

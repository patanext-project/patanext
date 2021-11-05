using System;
using System.Linq;
using System.Reflection;
using GameHost.Simulation.TabEcs.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PataNext.Generation.UnityGameHost
{
	class Program
	{
		static void Main(string[] args)
		{
			var header = @"
using Unity.Entities;
using System;
";

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (TryCreateType(type, out var gen))
					{
						Console.WriteLine("Created type: " + type);
						Console.WriteLine(gen);
					}
				}
			}

			var root = (CompilationUnitSyntax) CSharpSyntaxTree.ParseText(@"
using System;
using System.Reflection;
using GameHost.Simulation.TabEcs.Interfaces;

namespace PataNext.Generation.UnityGameHost
{
	public struct ComponentTest : IComponentData
	{
		public int Value;

		public GameEntity GameEntity;

		public int Property { get; set; }

		public readonly int ReadOnlyVar;
	}
}
").GetRoot();

			foreach (var member in root.Members)
			{
				if (member is NamespaceDeclarationSyntax namespaceSyntax)
				{
					foreach (var nsMember in namespaceSyntax.Members)
					{
						if (nsMember is StructDeclarationSyntax structSyntax)
						{
							foreach (var stMember in structSyntax.Members)
							{
								if (stMember is FieldDeclarationSyntax fieldSyntax)
								{
									var declaration = fieldSyntax.Declaration;
									var fieldType   = declaration.Type.GetText();
									if (fieldType.ToString().Replace(" ", string.Empty).SequenceEqual("GameEntity"))
									{
										Console.WriteLine("modify");
										root = root.ReplaceNode(fieldSyntax, fieldSyntax.WithDeclaration(
											declaration.WithType(
												SyntaxFactory.ParseName("Unity.Entities.Entity")
												             .WithTriviaFrom(declaration.Type))));
									}
								}
							}
						}
					}
				}
			}

			Console.WriteLine(root);
		}

		public static bool TryCreateType(Type type, out string generated)
		{
			generated = null;
			if (!type.IsValueType || !typeof(IEntityComponent).IsAssignableFrom(type))
				return false;

			string componentType = null;
			if (typeof(IComponentData).IsAssignableFrom(type))
				componentType = "IComponentData";
			if (typeof(IComponentBuffer).IsAssignableFrom(type))
				componentType = "IBufferElementData";

			if (componentType == null)
				return false;

			var fieldGen = string.Empty;
			foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				fieldGen += $"\t\tpublic {field.FieldType.Namespace}.{field.FieldType} {field.Name};\n";
			}

			generated = @$"namespace {type.Namespace} {{
	public struct {type.Name} : {componentType} {{
{fieldGen}
	}}
}}";

			return true;
		}
	}
}
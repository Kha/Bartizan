using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Patcher
{
	class MainClass
	{
		static IEnumerable<TypeDefinition> AllNestedTypes(TypeDefinition type)
		{
			yield return type;
			foreach (TypeDefinition nested in type.NestedTypes)
				foreach (TypeDefinition moarNested in AllNestedTypes(nested))
					yield return moarNested;
		}

		public static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine("Usage: Patcher.exe makeBaseImage | patchBaseImage");
				return;
			}
			if (args[0] == "makeBaseImage") {
				// unseal, publicize, virtualize
				var module = ModuleDefinition.ReadModule("OrigTowerFall.exe");
				foreach (var type in module.GetTypes()) {
					type.IsSealed = false;
					foreach (var field in type.Fields)
						field.IsPublic = true;
					foreach (var method in type.Methods) {
						method.IsPublic = true;
						if (!method.IsConstructor && !method.IsStatic)
							method.IsVirtual = true;
					}
				}
				module.Write("BaseTowerFall.exe");
			} else {
				var baseModule = ModuleDefinition.ReadModule("BaseTowerFall.exe");
				var modModule = ModuleDefinition.ReadModule("Mod.dll");

				// replace all object creations in BaseTowerFall with the respective derived class in Mod, if any.
				var typeMapping = modModule.Types.Where(t => t.BaseType != null).ToDictionary(t => t.BaseType.FullName);
				foreach (TypeDefinition type in baseModule.Types.SelectMany(AllNestedTypes))
					foreach (var method in type.Methods)
						if (method.HasBody)
							foreach (var instr in method.Body.Instructions)
								if (instr.OpCode == OpCodes.Newobj) {
									var ctor = (MethodReference)instr.Operand;
									if (typeMapping.ContainsKey(ctor.DeclaringType.FullName))
										instr.Operand = baseModule.Import(typeMapping[ctor.DeclaringType.FullName].Methods.Single((m => m.IsConstructor)));
								}
				baseModule.Write("TowerFall.exe");
			}
		}
	}
}
using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Xml.Linq;

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

		static void MakeBaseImage()
		{
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
		}

		static void PatchBaseImage()
		{
			var baseModule = ModuleDefinition.ReadModule("BaseTowerFall.exe");
			var modModule = ModuleDefinition.ReadModule("Mod.dll");
			var modTypes = modModule.Types.Where(t => t.BaseType != null).ToList();
			var typeMapping = modTypes.ToDictionary(t => t.BaseType.FullName);

			foreach (TypeDefinition type in baseModule.Types.SelectMany(AllNestedTypes))
				foreach (var method in type.Methods)
					if (method.HasBody)
						foreach (var instr in method.Body.Instructions) {
							var callee = instr.Operand as MethodReference;
							TypeDefinition modType;
							if (callee != null && typeMapping.TryGetValue(callee.DeclaringType.FullName, out modType)) {
								if (instr.OpCode == OpCodes.Newobj) {
									// replace all object creations in BaseTowerFall with the respective derived class in Mod, if any.
									instr.Operand = baseModule.Import(modType.Methods.Single((m => m.IsConstructor)));
								} else if (instr.OpCode == OpCodes.Call) {
									// make instance method calls to overriden methods virtual
									var overrider = modType.Methods.SingleOrDefault(m => m.Name == callee.Name);
									if (overrider != null && overrider.DeclaringType == modType)
										instr.OpCode = OpCodes.Callvirt;
								} else if (instr.OpCode == OpCodes.Ldftn) {
									// Some methods are called as callbacks, which are established by getting the method via ldftn.
									// In such cases we have to reroute the ldftn to our own class
									var overrider = modType.Methods.SingleOrDefault(m => m.Name == callee.Name);
									if (overrider != null && overrider.DeclaringType == modType)
										instr.Operand = baseModule.Import(overrider);
								}
							}
						}
			baseModule.Write("TowerFall.exe");
		}

		static void PatchResources()
		{
			foreach (var atlasPath in Directory.EnumerateDirectories(Path.Combine("Content", "Atlas"))) {
				File.Copy(Path.Combine("Original", atlasPath + ".png"), atlasPath + ".png", overwrite: true);
				File.Copy(Path.Combine("Original", atlasPath + ".xml"), atlasPath + ".xml", overwrite: true);

				var xml = XElement.Load(atlasPath + ".xml");

				string[] files = Directory.GetFiles(atlasPath, "*.png", SearchOption.AllDirectories);
				int x = 700;

				using (var baseImage = Bitmap.FromFile(atlasPath + ".png")) {
					using (var g = Graphics.FromImage(baseImage))
						foreach (string file in files)
							using (var image = Bitmap.FromFile(file)) {
								string name = file.Substring(atlasPath.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
								name = name.Substring(0, name.Length - ".png".Length);
								g.DrawImage(image, x, 1700);
								xml.Add(new XElement("SubTexture",
									new XAttribute("name", name),
									new XAttribute("x", x),
									new XAttribute("y", 1700),
									new XAttribute("width", image.Width),
									new XAttribute("height", image.Height)
								));
								x += image.Width;
							}
					baseImage.Save(atlasPath + ".png");
				}
				xml.Save(atlasPath + ".xml");
			}
		}

		public static void Main (string[] args)
		{
			if (args.Length != 1) {
				Console.WriteLine("Usage: Patcher.exe makeBaseImage | patchBaseImage");
				return;
			}
			if (args[0] == "makeBaseImage") {
				MakeBaseImage();
			} else {
				PatchBaseImage();
				PatchResources();
			}
		}
	}
}
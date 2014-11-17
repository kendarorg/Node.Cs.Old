// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GenericHelpers
{
	public class AssembliesManager
	{
		public static string GetAssemblyDirectory(Assembly asm = null)
		{
			if (asm == null)
			{
				asm = Assembly.GetCallingAssembly();
			}
			var codeBase = asm.CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}

		public static Type LoadType(string fullQualifiedName)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int index = 0; index < assemblies.Length; index++)
			{
				var asm = assemblies[index];
				var toret = LoadType(asm, fullQualifiedName);
				if (toret != null) return toret;

			}
			return null;
		}

		public static IEnumerable<Type> LoadTypesWithAttribute(params Type[] types)
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!asm.IsDynamic && !string.IsNullOrEmpty(asm.CodeBase))
				{
					foreach (var type in LoadTypesWithAttribute(asm, types))
					{
						yield return type;
					}
				}
			}
		}

		public static IEnumerable<Type> LoadTypesWithAttribute(Assembly sourceAssembly, params Type[] types)
		{
			var toret = new List<Type>();

			try
			{

				var classTypes = sourceAssembly.GetTypes();
				for (int classTypeIndex = 0; classTypeIndex < classTypes.Length; classTypeIndex++)
				{
					var classType = classTypes[classTypeIndex];
					var customAttributes = classType.GetCustomAttributes(true);
					bool founded = false;
					for (int attributeIndex = 0; attributeIndex < customAttributes.Length && founded == false; attributeIndex++)
					{
						object attribute = customAttributes[attributeIndex];
						for (int attributesTypeIndex = 0; attributesTypeIndex < types.Length; attributesTypeIndex++)
						{
							var type = types[attributesTypeIndex];
							if (attribute.GetType().FullName == type.FullName)
							{
								toret.Add(classType);
								founded = true;
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

			return toret;
		}

		public static IEnumerable<Type> LoadTypesInheritingFrom(params Type[] types)
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!asm.IsDynamic && !string.IsNullOrEmpty(asm.CodeBase))
				{
					foreach (var type in LoadTypesInheritingFrom(asm, types))
					{
						yield return type;
					}
				}
			}
		}

		public static IEnumerable<Type> LoadTypesInheritingFrom<T>()
		{
			return LoadTypesInheritingFrom(typeof(T));
		}

		public static IEnumerable<Type> LoadTypesInheritingFrom(Assembly sourceAssembly, params Type[] types)
		{
			var toret = new List<Type>();
			if (sourceAssembly.GetName().Name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
				sourceAssembly.GetName().Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
				sourceAssembly.GetName().Name == "mscorlib" ||
				sourceAssembly.GetName().Name == "System") return toret;
			try
			{
				var classTypes = sourceAssembly.GetTypes();
				for (int classTypeIndex = 0; classTypeIndex < classTypes.Length; classTypeIndex++)
				{
					var classType = classTypes[classTypeIndex];
					for (int attributesTypeIndex = 0; attributesTypeIndex < types.Length; attributesTypeIndex++)
					{
						var type = types[attributesTypeIndex];
						if (classType != type &&
							classType != typeof(object) &&
							!classType.IsAbstract &&
							!classType.IsInterface &&
							(type.IsAssignableFrom(classType) || type.IsSubclassOf(classType)))
						{
							toret.Add(classType);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return toret;
		}

		private static void DumpAssembly(string path, Assembly assm, Dictionary<string, Assembly> alreadyLoaded)
		{
			AssemblyName fullQualifiedName = assm.GetName();
			if (alreadyLoaded.ContainsKey(fullQualifiedName.FullName))
			{
				return;
			}
			alreadyLoaded[fullQualifiedName.FullName] = assm;

			foreach (AssemblyName name in assm.GetReferencedAssemblies())
			{
				if (!alreadyLoaded.ContainsKey(name.FullName))
				{
					var dllFile = GetAssemblyName(name.FullName);
					var matchingFiles = new List<string>(Directory.GetFiles(path, dllFile, SearchOption.AllDirectories));
					if (matchingFiles.Count != 1)
					{
						try
						{
							Assembly.Load(name);
						}
						catch
						{
							throw new FileNotFoundException(string.Format("Dll not found in {0} or subdirectories", path), dllFile);
						}

					}
					else
					{
						Assembly referenced = Assembly.LoadFrom(matchingFiles[0]);
						DumpAssembly(path, referenced, alreadyLoaded);
					}
				}
			}
		}

		private static string GetAssemblyName(string fullName)
		{
			var comma = fullName.IndexOf(",", StringComparison.InvariantCultureIgnoreCase);
			return fullName.Substring(0, comma) + ".dll";
		}

		public static bool LoadAssemblyFrom(string dllFile, List<string> missingDll, params string[] paths)
		{
			foreach (var path in paths)
			{
				try
				{
					LoadAssemblyFrom(path, dllFile);
					return true;
				}
				catch (FileNotFoundException ex)
				{
					if (missingDll != null) missingDll.Add(ex.FileName);
				}
			}
			return false;
		}

		public static bool LoadAssemblyFrom(string path, string dllFile, bool throwOnError = true)
		{
			var alreadyPresentAssemblies = new Dictionary<string, Assembly>();

			foreach (var alreadyPresent in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!alreadyPresent.IsDynamic && !string.IsNullOrEmpty(alreadyPresent.CodeBase))
				{
					alreadyPresentAssemblies.Add(alreadyPresent.FullName, alreadyPresent);

				}
			}
			var matchingFiles = new List<string>(Directory.GetFiles(path, dllFile, SearchOption.AllDirectories));
			if (matchingFiles.Count != 1)
			{
				if (!throwOnError) return false;
				throw new FileNotFoundException(string.Format("Dll not found in {0} or subdirectories", path), Path.Combine(path, dllFile));
			}
			try
			{
				var sm = Assembly.LoadFrom(matchingFiles[0]);
				DumpAssembly(path, sm, alreadyPresentAssemblies);
				return true;
			}
			catch (Exception)
			{
				if (!throwOnError) return false;
				throw new FileNotFoundException("Dll not found ", matchingFiles[0]);
			}
		}

		public static Assembly LoadReflectionAssemblyFrom(string path, string dllFile, bool throwOnError = true)
		{
			var matchingFiles = new List<string>(Directory.GetFiles(path, dllFile, SearchOption.AllDirectories));
			if (matchingFiles.Count != 1)
			{
				if (!throwOnError) return null;
				throw new FileNotFoundException(string.Format("Dll not found in {0} or subdirectories", path), Path.Combine(path, dllFile));
			}
			try
			{
				var bytes = File.ReadAllBytes(matchingFiles[0]);
				return Assembly.ReflectionOnlyLoad(bytes);
			}
			catch (Exception)
			{
				if (!throwOnError) return null;
				throw new FileNotFoundException("Dll not found ", matchingFiles[0]);
			}
		}

		public static IEnumerable<Assembly> LoadAssembliesFrom(string path, bool deep = true)
		{
			var result = new List<Assembly>();
			var asmFileList = new List<string>(Directory.GetFiles(path, "*.dll", deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
			var alreadyPresentAssemblies = new Dictionary<string, Assembly>();

			foreach (var alreadyPresent in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!alreadyPresent.IsDynamic && !string.IsNullOrEmpty(alreadyPresent.CodeBase))
				{
					alreadyPresentAssemblies.Add(alreadyPresent.FullName, alreadyPresent);
				}
			}

			int assemblyLoaded;
			do
			{
				assemblyLoaded = 0;

				for (int i = asmFileList.Count - 1; i > -1; i--)
				{
					try
					{
						var asmPath = asmFileList[i];
						var reflectionOnlyAssembly = Assembly.ReflectionOnlyLoadFrom(asmPath);
						if (!alreadyPresentAssemblies.ContainsKey(reflectionOnlyAssembly.FullName))
						{
							result.Add(Assembly.LoadFrom(asmPath));
						}

						asmFileList.RemoveAt(i);
						assemblyLoaded++;
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
					}
				}
			} while (assemblyLoaded > 0 && asmFileList.Count > 0);

			if (asmFileList.Count > 0)
				throw new Exception(string.Concat("Error loading assemblies: ", Environment.NewLine, string.Join(Environment.NewLine, asmFileList)));

			return result;
		}

		public static Type LoadType(Assembly sourceAssembly, string fullQualifiedName)
		{
			if (!fullQualifiedName.Contains("<"))
			{
				return sourceAssembly.GetType(fullQualifiedName, false);
			}

			var tb = ParseType(fullQualifiedName);
			return LoadType(sourceAssembly, tb);
		}

		public static TypeBlock ParseType(string name)
		{
			int i = 0;
			var res = ParseType(ref i, name);
			if (res.Count != 1) return null;
			return res[0];
		}

		public static List<TypeBlock> ParseType(ref  int i, string name)
		{
			var tbList = new List<TypeBlock>();
			var tempName = string.Empty;
			for (; i < name.Length; i++)
			{
				var c = name[i];

				switch (c)
				{
					case ('<'):
						{
							//Create the previous type block
							var tb = new TypeBlock();
							tb.Name = tempName.Trim();
							i++;
							tb.GenericArgs = ParseType(ref i, name);
							tbList.Add(tb);
							tempName = string.Empty;
							break;
						}

					case ('>'):
						{
							break;
						}
					case (','):
						{
							if (tempName.Length > 0)
							{
								var tb = new TypeBlock();
								tb.Name = tempName.Trim();
								tbList.Add(tb);
							}
							tempName = string.Empty;
							break;
						}
					default:
						{
							tempName += c;
							break;
						}
				}
			}
			if (tempName.Length > 0)
			{
				var tb = new TypeBlock();
				tb.Name = tempName.Trim();
				tbList.Add(tb);
			}
			return tbList;
		}

		private static Type LoadType(Assembly fullQualifiedName, TypeBlock tb)
		{
			return Type.GetType(tb.ToGenericString());
		}

		public class TypeBlock
		{
			public string Name;
			public List<TypeBlock> GenericArgs = new List<TypeBlock>();
			public bool IsGeneric { get { return GenericArgs.Count > 0; } }

			public override string ToString()
			{
				if (GenericArgs.Count == 0) return Name;
				return string.Format("{0}<{1}>", Name, string.Join(",", GenericArgs));
			}

			public string ToGenericString()
			{
				if (GenericArgs.Count == 0) return Name;
				return string.Format("{0}`{1}[{2}]", Name, GenericArgs.Count, string.Join(",", GenericArgs.Select(a => a.ToGenericString())));
			}
		}
	}


}

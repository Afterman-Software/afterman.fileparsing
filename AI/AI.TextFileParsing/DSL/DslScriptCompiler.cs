using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using AI.TextFileParsing.DSL.Extensions;
using Rhino.DSL;

namespace AI.TextFileParsing.DSL
{
    using AI.Common.Extensions.Sys;

    public class DslScriptCompiler : IDslScriptCompiler
	{
	   
	    public DslScriptCompiler()
	    {
	        
	    }
		public DslAssembly Execute(string script)
		{
			var engine = new IoDslEngine();
			try
			{
                var scriptFile = SharedTempFile.NewShortNameFile();
				using (var fileStream = new StreamWriter(scriptFile))
				{
					fileStream.Write(script);
					fileStream.Close();
				}
				var context = engine.Compile(scriptFile);
				if (context.Errors.Any())
				{
					var errors = String.Empty;
					foreach (var err in context.Errors)
					{
						errors += err.Message + "\r\n";
					}
					throw new DslCompileException(errors);
				}
				var className = GetFirstClass(context.GeneratedAssemblyFileName);
				var assemblyFile = SaveAssembly(context.GeneratedAssemblyFileName);
				return new DslAssembly()
				{
					Assembly = assemblyFile,
					ClassName = className,
				};
			}
			catch (Exception e)
			{
				throw new DslCompileException(e.Message);
			}
		}

		private string SaveAssembly(string assembly)
		{
			var oldFile = new FileInfo(assembly);
			var targetPath = ConfigurationManager.AppSettings["DslAssemblyPath"];
			targetPath.EnsureDirectoryExists();
			var targetFile = System.IO.Path.Combine(targetPath, oldFile.Name);
			var newFile = oldFile.CopyTo(targetFile, true);
			return newFile.FullName;
		}

		private string GetFirstClass(string assembly)
		{
			Assembly asm = null;
			try
			{
				asm = Assembly.LoadFrom(assembly);
				foreach (var t in asm.GetTypes())
				{
					return t.FullName;
				}
			}

			finally
			{
				if (null != asm)
				{

				}
			}
			return String.Empty;
		}




		public IEnumerable<DslMacro> GetAvailableMacros()
		{
			var types = new Dictionary<string, Type>();
			types["Math"] = typeof(DslMathExtensions);
			types["Common"] = typeof(DslCommonExtensions);
			types["Text"] = typeof(DslStringExtensions);
			types["Tables"] = typeof(DslTableExtensions);
			return GetMacrosForClass(types);
		}

		public IEnumerable<DslMacro> GetMacrosForClass(Dictionary<string, Type> types)
		{
			string[] ignoreMethods = new[] { "ToString", "GetType", "GetHashCode", "Equals" };
			var tableExt = new DslTableExtensions();
			foreach (var typeLookup in types)
			{
				var type = typeLookup.Value;
				var asStringOnly = type.GetCustomAttribute<MethodValuesAsStringsAttribute>() != null;
				var key = typeLookup.Key;
				var methods = type.GetMethods();
				foreach (var method in methods)
				{
					if (ignoreMethods.Contains(method.Name))
						continue;

					if (asStringOnly && method.ReturnType == typeof(string))
					{
						var formattedKey = method.Invoke(tableExt, null).ToString();
						yield return new DslMacro(
							String.Format("{0}", formattedKey),
							key);
					}
					else
					{
						yield return new DslMacro(
							String.Format("{0}.{1}", typeLookup.Key, method.Name),
							key);
					}
				}
			}
		}
	}



	public class IoDslEngine : DslEngine
	{
		protected override void CustomizeCompiler(Boo.Lang.Compiler.BooCompiler compiler, Boo.Lang.Compiler.CompilerPipeline pipeline, string[] urls)
		{
			compiler.Parameters.Ducky = true;
			compiler.Parameters.GenerateInMemory = false;

			pipeline.Insert(1, new Rhino.DSL.ImplicitBaseClassCompilerStep(typeof(DslParser),
				"CalculateValue",
				"System",
				"AI.TextFileParsing.DSL.Extensions",
				"AI.TextFileParsing.DSL"));

		}
	}
}

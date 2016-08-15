using System;
using System.Configuration;
using System.Reflection;
using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing.DSL
{
	public class DslScriptRunner : IDslScriptRunner
	{
		//TODO: are boo scripts using a secondary domain?
		private static AppDomain _secondaryDomain;

		static DslScriptRunner()
		{
			AppDomainSetup clads = new AppDomainSetup
			{
				ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
				PrivateBinPath = ConfigurationManager.AppSettings["DslAssemblyPath"],
				ShadowCopyFiles = "true",
				DisallowCodeDownload = true
			};
			_secondaryDomain = AppDomain.CreateDomain("Secondary Dsl Scripting Domain", null, clads);
		}

		public void RunScript(string assembly, string className, IColumn column, IRow row, ITable table)
		{
			var asm = Assembly.LoadFrom(assembly);
			var t = asm.GetType(className);

			var script = Activator.CreateInstance(t) as DslParser;
			if (script == null)
				throw new InvalidCastException("Script is not of type DslParser");
			script.Column = column;
			script.CurrentRow = row;
			script.Table = table;
			script.Execute();
		}
	}
}
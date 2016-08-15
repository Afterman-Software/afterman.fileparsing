using System;
using AI.Scripting;
using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing.Scripting
{
    public class RowDefinitionScriptConfiguration : ScriptConfiguration
    {
        public RowDefinitionScriptConfiguration()
        {
            this.Language = ScriptLanguages.CSharp;
            this.SourceCode = this.DefaultCSharpScript();
            this.CompilerOptions.References.FromDelimitedString("System.dll;System.Configuration.dll;System.Core.dll;Performant.IO.Domain.dll;AI.Scripting.dll;AI.TextFileParsing.dll;AI.TextFileParsing.Interfaces.dll");
            this.CompilerOptions.Imports.FromDelimitedString("System;System.Linq;Performant.IO.Domain.TextParsing");
            this.CompilerOptions.OutputType = OutputTypes.File;
        }

        public override string DefaultCSharpScript()
        {
            return "p0 = p0;";
        }

        public override string DefaultVisualBasicScript()
        {
            return "p0 = p0";
        }

        public override string RequiredMethodName()
        {
            return "ProcessParsedField";
        }

        public override Type[] RequiredMethodParameters()
        {
            return new Type[] { typeof(IRow), typeof(IColumn) };
        }

        public override Type RequiredMethodReturnValue()
        {
            return null;
        }


        public bool RowDefinitionScriptMethodExists(ref Exception except)
        {
            return this.ScriptEngine.MethodExists(this.RequiredMethodName(), this.RequiredMethodParameters(), this.RequiredMethodReturnValue(), ref except);
        }

        public bool RowDefinitionScriptMethodExists()
        {
            bool exists = false;
            Exception except = null;
            exists = RowDefinitionScriptMethodExists(ref except);
            if (except != null)
                throw except;
            else
                return exists;
        }

        public void RowDefinitionScriptCallMethod(IRow row, IColumn col, ref Exception except)
        {
            this.ScriptEngine.CallMethod(this.RequiredMethodName(), new object[] { row, col }, ref except);
        }

        public void RowDefinitionScriptCallMethod(IRow row, IColumn col)
        {
            Exception except = null;
            RowDefinitionScriptCallMethod(row, col, ref except);
            if (except != null)
                throw except;
            else
                return;
        }
    }
}

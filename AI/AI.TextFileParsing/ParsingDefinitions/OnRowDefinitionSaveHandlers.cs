using AI.TextFileParsing.DSL;

namespace AI.TextFileParsing.ParsingDefinitions
{
    public class OnRowDefinitionSaveHandlers : AI.nRepo.IBeforeAddListener
    {
        private readonly IDslScriptCompiler _compiler;

        public OnRowDefinitionSaveHandlers(IDslScriptCompiler compiler)
        {
            this._compiler = compiler;
        }

        public void Handle(object entity)
        {
            var rowDef = entity as RowDefinition;
            if (null == rowDef || (string.IsNullOrEmpty(rowDef.CustomScript) && string.IsNullOrEmpty(rowDef.AIScriptSource)))
                return;

            if (!string.IsNullOrEmpty(rowDef.AIScriptSource))
            {
                rowDef.CustomScript = null;
                rowDef.CustomClassName = null;
                rowDef.CustomAssemblyPath = null;
            }
            else if (!string.IsNullOrEmpty(rowDef.CustomScript))
            {
                var dslAssembly = this._compiler.Execute(rowDef.CustomScript);
                rowDef.CustomClassName = dslAssembly.ClassName;
                rowDef.CustomAssemblyPath = dslAssembly.Assembly;
                rowDef.AIScriptSource = null;
            }
        }
    }
}
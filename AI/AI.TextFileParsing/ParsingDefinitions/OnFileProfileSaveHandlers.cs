using AI.TextFileParsing.DSL;

namespace AI.TextFileParsing.ParsingDefinitions
{
    public class OnFileProfileSaveHandlers : AI.nRepo.IBeforeAddListener
    {
        private readonly IDslScriptCompiler _compiler;

        public OnFileProfileSaveHandlers(IDslScriptCompiler compiler)
        {
            this._compiler = compiler;
        }

        public void Handle(object entity)
        {
            var fileProf = entity as FileProfile;

            if (null == fileProf)
                return;

            if (!string.IsNullOrWhiteSpace(fileProf.CustomScript))
            {
                var dslAssembly = _compiler.Execute(fileProf.CustomScript);
                fileProf.CustomClassName = dslAssembly.ClassName;
                fileProf.CustomAssemblyPath = dslAssembly.Assembly;
            }
        }
    }
}
namespace WuGanhao.Resource.Compiler;

public interface IResourceCompiler
{
    void Compile(string sourceFileName, string targetFileName, string defaultCulture = "en");
}

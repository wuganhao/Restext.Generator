namespace WuGanhao.Resource.Compiler;

public interface ILogger
{
    void Info(string message);
    void Info(string subCategory, string errorCode, string fileName, int line, int column, string message);
    void Error(string subCategory, string errorCode, string fileName, int line, int column, string message);
    void Error(string subCategory, string errorCode, string fileName, int line, int column, Exception ex);
}

using Microsoft.Build.Utilities;
using System;
using ILogger = WuGanhao.Resource.Compiler.ILogger;

namespace WuGanhao.Restext.Compiler
{
    public class MSBuildLogger: ILogger
    {
        private TaskLoggingHelper _log;
        public MSBuildLogger(TaskLoggingHelper logHelper)
        {
            this._log = logHelper;
        }

        public void Error(string subCategory, string errorCode, string fileName, int line, int column, string message) =>
            this._log.LogError(subCategory, errorCode, null, fileName, line, column, line, column, message);

        public void Error(string subCategory, string errorCode, string fileName, int line, int column, Exception ex)
        {
            this._log.LogError(subCategory, errorCode, null, fileName, line, column, line, column, ex?.Message);
            this._log.LogErrorFromException(ex);
        }

        public void Info(string message)
        {
            this._log.LogMessage(message);
        }

        public void Info(string subCategory, string errorCode, string fileName, int line, int column, string message) =>
            this._log.LogMessage(subCategory, errorCode, null, fileName, line, column, line, column, message);
    }
}

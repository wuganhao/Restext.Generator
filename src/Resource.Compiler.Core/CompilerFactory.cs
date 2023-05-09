using System;
using System.IO;

namespace WuGanhao.Resource.Compiler
{
    public class CompilerFactory {
        public static void Compile(ILogger logger, string src, string dest) {
            string ext = Path.GetExtension(src);

            IResourceCompiler compiler = null;
            if (string.Equals(ext, ".restext", StringComparison.InvariantCultureIgnoreCase)) {
                compiler = new RestextCompiler(logger);
            }

            compiler.Compile(src, dest);
        }
    }
}

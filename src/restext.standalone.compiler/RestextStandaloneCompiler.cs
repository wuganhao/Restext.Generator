using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using WuGanhao.Resource.Compiler;

namespace WuGanhao.Restext.Compiler;

public class RestextStandaloneCompiler : Task
{
    public ITaskItem[] Sources { get; set; }

    public string DefaultCulture { get; set; }

    public override bool Execute()
    {
        if (Sources == null) return true;

        MSBuildLogger logger = new MSBuildLogger(Log);

        string solutionDir = null;
        BuildEngine7.GetGlobalProperties()?.TryGetValue("SolutionDir", out solutionDir);
        Uri solutionUri = solutionDir == null ? null : new Uri(Path.GetFullPath(solutionDir));

        foreach (ITaskItem item in Sources)
        {
            string src = item.ItemSpec;

            if (solutionUri != null)
            {
                Uri currentRestextUri = new Uri(Path.GetFullPath(src));
                if (!solutionUri.IsBaseOf(currentRestextUri))
                {
                    Log.LogMessage(MessageImportance.Normal, $"Skipping {src}, out of current solution folder.");
                    continue;
                }
            }

            string dest = Path.ChangeExtension(src, ".resources");
            CompilerFactory.Compile(logger, src, dest);
        }

        return true;
    }
}

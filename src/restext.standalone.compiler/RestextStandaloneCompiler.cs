using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Resources;
using System.Linq;

namespace WuGanhao.Restext.Compiler {
    public class RestextStandaloneCompiler : Task {
        public ITaskItem[] Sources { get; set; }
        public string DefaultCulture { get; set; }
        public override bool Execute() {
            if (this.Sources == null) return true;

            string solutionDir = null;
            this.BuildEngine7.GetGlobalProperties()?.TryGetValue("SolutionDir", out solutionDir);
            Uri solutionUri = (solutionDir == null) ? null : new Uri(Path.GetFullPath(solutionDir));

            foreach (ITaskItem item in this.Sources) {
                string restext = item.ItemSpec;
                try {
                    if (solutionUri != null) {
                        Uri currentRestextUri = new Uri(Path.GetFullPath(restext));
                        if (!solutionUri.IsBaseOf(currentRestextUri)) {
                            this.Log.LogMessage(MessageImportance.Normal, $"Skipping {restext}, out of current solution folder.");
                            continue;
                        }
                    }

                    if (!File.Exists(restext)) {
                        this.Log.LogError($"Input file {restext} not found!");
                    }

                    string resources = Path.ChangeExtension(restext, ".resources");

                    this.Log.LogMessage($"Generating {resources} ...");
                    if (File.Exists(resources)) File.Delete(resources);
                    FileStream ous = new FileStream(resources, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                    using ResourceWriter writer = new ResourceWriter(ous);
                    StreamReader reader = new StreamReader(restext);

                    string line = null;
                    long lIdx = 0;
                    while(null != (line = reader.ReadLine())) {
                        lIdx ++;
                        int idx = line.IndexOf('=');
                        if (idx < 0) {
                            this.Log.LogWarning($"Incorrect format at line: {lIdx}");
                            continue;
                        }
                        string key = line.Substring(0, idx);
                        string value = line.Substring(Math.Min(idx + 1, line.Length));
                        try {
                            writer.AddResource(key, value);
                        } catch (Exception ex) {
                            throw new InvalidOperationException($"Failed adding resource {key} = {value}", ex);
                        }
                    }
                    writer.Generate();
                    ous.Flush();

                    if (!string.IsNullOrEmpty(this.DefaultCulture)) {
                        string noExtResources = Path.GetFileNameWithoutExtension(resources);
                        string culture = Path.GetExtension(noExtResources);
                        string fileName = Path.GetFileNameWithoutExtension(noExtResources);

                        if (string.Equals(culture, $".{this.DefaultCulture}", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(fileName)) {
                            string folder = Path.GetDirectoryName(resources);
                            string targetPath = Path.Combine(folder, $"{fileName}.resources");
                            this.Log.LogMessage($"Generating {targetPath} ...");
                            if (File.Exists(targetPath)) File.Delete(targetPath);
                            File.Copy(resources, targetPath);
                        }
                    }
                } catch (Exception ex) {
                    this.Log.LogError($"Failed to compile {restext}");
                    this.Log.LogErrorFromException(ex);
                }
            }

            return true;
        }
    }
}

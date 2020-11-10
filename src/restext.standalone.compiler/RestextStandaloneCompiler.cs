using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Resources;

namespace WuGanhao.Restext.Compiler {
    public class RestextStandaloneCompiler : Task {
        public ITaskItem[] Sources { get; set; }
        public string DefaultCulture { get; set; }
        public override bool Execute() {
            if (this.Sources == null) return true;

            foreach(ITaskItem item in this.Sources) {
                string restext = item.ItemSpec;
                if (!File.Exists(restext)) {
                    this.Log.LogError($"Input file {restext} not found!");
                }

                string resources = Path.ChangeExtension(restext, ".resources");

                FileStream ous = new FileStream(resources, FileMode.Create, FileAccess.Write, FileShare.Read);
                using ResourceWriter writer = new ResourceWriter(ous);
                StreamReader reader = new StreamReader(restext);

                string line = null;
                long lIdx = 0;
                this.Log.LogMessage($"Generating {resources} ...");
                while(null != (line = reader.ReadLine())) {
                    lIdx ++;
                    int idx = line.IndexOf('=');
                    if (idx < 0) {
                        this.Log.LogWarning($"Incorrect format at line: {lIdx}");
                        continue;
                    }
                    string key = line.Substring(0, idx);
                    string value = line.Substring(Math.Min(idx + 1, line.Length));
                    writer.AddResource(key, value);
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
                        File.Copy(resources, targetPath);
                    }
                }
            }

            return true;
        }
    }
}

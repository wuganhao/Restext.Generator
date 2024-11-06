using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace WuGanhao.Restext.Compiler;

public class RestextStandaloneCompiler : Task {
    public ITaskItem[] Sources { get; set; }
    public string DefaultCulture { get; set; }
    public override bool Execute() {
        if (this.Sources == null) return true;

        string solutionDir = null;
        this.BuildEngine7.GetGlobalProperties()?.TryGetValue("SolutionDir", out solutionDir);
        Uri solutionUri = (solutionDir == null) ? null : new Uri(Path.GetFullPath(solutionDir));

        bool error = false;
        foreach (ITaskItem item in this.Sources) {
            string restext = item.ItemSpec;
            int lIdx = 0;

            try
            {
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
                using FileStream ous = new FileStream(resources, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                using ResourceWriter writer = new ResourceWriter(ous);
                Dictionary<string, string> dict = new();

                string line = null;
                using (StreamReader reader = new StreamReader(restext)) {
                    while(null != (line = reader.ReadLine())) {
                        lIdx ++;
                        int idx = line.IndexOf('=');

                        if (string.IsNullOrWhiteSpace(line)) {
                            continue;
                        }

                        if (idx < 0) {
                            this.Log.LogError("RES-GEN", "RG-001", "RG-001", item.ItemSpec, lIdx, 0, 0, 0, "Incorrect resource format");
                            error = true;
                            continue;
                        }

                        string key = line.Substring(0, idx)?.Trim();
                        string value = line.Substring(Math.Min(idx + 1, line.Length))?.Trim();

                        if (dict.TryGetValue(key, out string prevVal) && prevVal != value) {
                            this.Log.LogError("RES-GEN", "RG-003", "RG-003", item.ItemSpec, lIdx, 0, 0, 0, "Duplicated entry for {0}", key);
                            error = true;
                        } else {
                            dict[key] = value;
                        }
                    }
                }

                if (error) {
                    continue;
                }

                foreach (var kvp in dict) {
                    try {
                        writer.AddResource(kvp.Key, kvp.Value);
                    } catch (Exception ex) {
                        throw new InvalidOperationException($"Failed adding resource {kvp.Key} = {kvp.Value}", ex);
                    }
                }
                writer.Generate();
                ous.Flush();

                using (StreamWriter restextWriter = new StreamWriter(restext)) {
                    foreach ( var key in dict.Keys.OrderBy(x => x, StringComparer.InvariantCultureIgnoreCase)) {
                        string value = dict[key];
                        restextWriter.WriteLine($"{key}={value}");
                    }
                }

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
                error = true;
                this.Log.LogError(subcategory:"RES-GEN", errorCode: "RG-002", helpKeyword: "RG-002", file: item.ItemSpec, lineNumber: lIdx, 0, 0, 0, ex.Message, null);
                this.Log.LogErrorFromException(ex);
            }
        }

        return !error;
    }
}

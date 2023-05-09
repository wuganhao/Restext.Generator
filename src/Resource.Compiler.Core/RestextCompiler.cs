using System.Resources;

namespace WuGanhao.Resource.Compiler;

public static class ErrorCode
{
    public const string CATEGORY = "RES-GEN";
    public const string FILE_NOT_FOUND = "RG-001";
    public const string INCORRECT_FORMAT = "RG-002";
    public const string FATAL = "RG-003";
}

public class RestextCompiler: IResourceCompiler
{
    private ILogger Log { get; }

    public RestextCompiler(ILogger logger) {
        this.Log = logger;
    }

    public void Compile(string restext, string dest, string defaultCulture = "en") {
        int lIdx = 0;
        try {
            if (!File.Exists(restext)) {
                //this.Log.LogError($"Input file {restext} not found!");
                this.Log.Error(ErrorCode.CATEGORY, ErrorCode.FILE_NOT_FOUND, restext, 0, 0, "Input file not found!");
            }

            this.Log.Info($"Generating {dest} ...");
            if (File.Exists(dest)) File.Delete(dest);
            FileStream ous = new FileStream(dest, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            using ResourceWriter writer = new ResourceWriter(ous);
            StreamReader reader = new StreamReader(restext);

            string line = null;
            while (null != (line = reader.ReadLine())) {
                lIdx++;
                int idx = line.IndexOf('=');
                if (idx < 0) {
                    //this.Log.LogWarning("RES-GEN", "RG-001", "RG-001", restext, lIdx, 0, 0, 0, "Incorrect resource format");
                    this.Log.Info(ErrorCode.CATEGORY, ErrorCode.INCORRECT_FORMAT, restext, lIdx, 0, "Incorrect resource format");
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

            if (!string.IsNullOrEmpty(defaultCulture)) {
                string noExtResources = Path.GetFileNameWithoutExtension(dest);
                string culture = Path.GetExtension(noExtResources);
                string fileName = Path.GetFileNameWithoutExtension(noExtResources);

                if (string.Equals(culture, $".{defaultCulture}", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(fileName)) {
                    string folder = Path.GetDirectoryName(dest);
                    string targetPath = Path.Combine(folder, $"{fileName}.resources");
                    this.Log.Info($"Generating {targetPath} ...");
                    if (File.Exists(targetPath)) File.Delete(targetPath);
                    File.Copy(dest, targetPath);
                }
            }
        } catch (Exception ex) {
            //this.Log.LogError(subcategory: "RES-GEN", errorCode: "RG-002", helpKeyword: "RG-002", file: restext, lineNumber: lIdx, 0, 0, 0, ex.Message, null);
            //this.Log.LogErrorFromException(ex);
            this.Log.Error(ErrorCode.CATEGORY, ErrorCode.FATAL, restext, lIdx, 0, ex);
        }
    }
}

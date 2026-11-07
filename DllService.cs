using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
namespace ModForge;
public class DllService {
  public string DllPath;
  public List<string> Refs = new();
  public List<string> TypeNames() {
    using var m = ModuleDefinition.ReadModule(DllPath);
    return m.GetTypes().Where(t => t.Name != "<Module>").Select(t => t.FullName).OrderBy(x => x).ToList();
  }
  public string Decompile(string fullName) {
    var d = new CSharpDecompiler(DllPath, new DecompilerSettings());
    return d.DecompileTypeAsString(new FullTypeName(fullName));
  }
  public (bool ok, string log, string outPath) Compile(string source, string asmName) {
    var refs = Refs.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p));
    var comp = CSharpCompilation.Create(asmName, new[] { CSharpSyntaxTree.ParseText(source) }, refs,
      new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    var outPath = Path.Combine(FileSystem.AppDataDirectory, asmName + ".dll");
    var r = comp.Emit(outPath);
    var log = string.Join("\n", r.Diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Select(x => x.ToString()));
    return (r.Success, log, outPath);
  }
  public static async Task<string> CopyPicked(FileResult f, string sub) {
    var dir = Path.Combine(FileSystem.AppDataDirectory, sub); Directory.CreateDirectory(dir);
    var dest = Path.Combine(dir, f.FileName);
    using var s = await f.OpenReadAsync(); using var o = File.Create(dest); await s.CopyToAsync(o);
    return dest;
  }
}

#:property UseWPF=true
#:property TargetFramework=net10.0-windows
#:property PublishTrimmed=false
#:property SelfContained=false
using System.Windows.Media;

string dir = @"C:\Users\Res\Downloads\Pandoras-Box-Optimizer\PandorasBoxOptimizer\assets\";
Console.WriteLine("Families WPF can read from: " + dir);
var fams = Fonts.GetFontFamilies(dir);
if (!fams.Any()) { Console.WriteLine("  (NONE — WPF cannot parse these TTFs)"); return; }
foreach (var f in fams)
{
    var gtf = f.GetTypefaces().FirstOrDefault();
    Console.WriteLine($"  source='{f.Source}'  names=[{string.Join(" | ", f.FamilyNames.Values)}]");
}

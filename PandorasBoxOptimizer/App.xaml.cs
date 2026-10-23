using System;
using System.Linq;
using System.Windows;

namespace PandorasBoxOptimizer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Dev self-test: --verify <code> <outfile> hits the real endpoint via the app's own
        // HttpClient + machine id, writes the result, and exits (no UI).
        int vi = Array.IndexOf(e.Args, "--verify");
        if (vi >= 0 && vi + 2 < e.Args.Length)
        {
            // Run off the UI thread to avoid an async-over-sync deadlock on the dispatcher.
            var r = System.Threading.Tasks.Task.Run(() => License.VerifyAsync(e.Args[vi + 1])).GetAwaiter().GetResult();
            System.IO.File.WriteAllText(e.Args[vi + 2], $"ok={r.Ok};reason={r.Reason};machine={License.MachineId()}");
            Shutdown();
            return;
        }

        // Dev/verification: --shot jumps straight to the main window (skips the gate).
        if (e.Args.Contains("--shot")) { new MainWindow().Show(); return; }

        // Normal launch: the activation window checks a saved code (or asks for one),
        // then opens the main window itself.
        new ActivationWindow().Show();
    }
}

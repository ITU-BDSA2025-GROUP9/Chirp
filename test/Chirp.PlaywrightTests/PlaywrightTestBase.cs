using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Chirp.PlaywrightTests;

[SetUpFixture]
public class PlaywrightTestBase
{
    private static Process? _webProcess;
    public static string AppUrl { get; private set; } = "http://localhost:5198";

    [OneTimeSetUp]
    public void GlobalSetup()
    {
        if (_webProcess != null && !_webProcess.HasExited)
            return;

        var webProjectPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../../src/Chirp.Web"));
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --urls={AppUrl}",
            WorkingDirectory = webProjectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        _webProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start Chirp.Web process.");

        Console.WriteLine("Starting Chirp.Web...");

        // Wait until app stats up
        var started = false;
        var start = DateTime.UtcNow;
        while (!started && (DateTime.UtcNow - start).TotalSeconds < 20)
        {
            var line = _webProcess.StandardOutput.ReadLine();
            if (line?.Contains("Now listening on:", StringComparison.OrdinalIgnoreCase) == true)
            {
                Console.WriteLine(line);
                started = true;
            }
        }

        if (!started)
        {
            _webProcess.Kill(true);
            throw new TimeoutException("Timed out waiting for Chirp.Web to start.");
        }

        Console.WriteLine($"Chirp.Web running at {AppUrl}");
    }

    [OneTimeTearDown]
    public void GlobalTeardown()
    {
        if (_webProcess is { HasExited: false })
        {
            Console.WriteLine("Stopping...");
            _webProcess.Kill(true);
            _webProcess.Dispose();
        }
    }
}

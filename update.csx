#!/usr/bin/env dotnet-script

using System.ComponentModel;
using System.Text.Json;

class Dependency
{
    public string url { get; set; }
    public string Path;
}

var DEPENDENCIES = JsonSerializer.Deserialize<Dictionary<string, Dependency>>(File.ReadAllText("dependencies/deps.json"));

Main();

void Main()
{
    var shouldContinue = true;
    foreach (var (program, exist) in CheckRequirements())
    {
        if (!exist)
        {
            shouldContinue = false;
            Error($"Program '{program}' not found.");
        }
    }

    if (!shouldContinue) {
        Error("Execution stopped");
        return;
    }

    if (Environment.GetCommandLineArgs().Contains("--no-fetch"))
    {
        Debug("Fetching dependencies cancelled");
    }
    else
    {
        CheckDependencies();
    }

    if (Environment.GetCommandLineArgs().Contains("--no-pack"))
    {
        Debug("Packaging cancelled");
    }
    else
    {
        StartPackaging();
    }
}

IEnumerable<(string name, bool exist)> CheckRequirements()
{
    (string, bool) CheckProgram(string program)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = program,
                RedirectStandardOutput = true
            });
            process.WaitForExit();
        }
        catch (Win32Exception)
        {
            return (program, false);
        }
        
        return (program, true);
    }

    yield return CheckProgram("git");
}

void CheckDependencies()
{
    var directory = Directory.CreateDirectory("dependencies");
    foreach (var (shortName, dep) in DEPENDENCIES)
    {
        var sub = directory.CreateSubdirectory(shortName);

        var overrides = sub.GetFiles("PATH").FirstOrDefault();
        if (overrides != null)
        {
            var path = File.ReadAllText(overrides.FullName);
            if (!Directory.Exists(path))
            {
                Error($"Dependency {shortName} has an override, but doesn't point to a correct directory. (target={path})");
                return;
            }
            
            dep.Path = path;

            Debug($"Dependency {shortName} has an override (target={path})', skipping.");
            continue;
        }
        
        dep.Path = sub.FullName;

        // Update
        if (sub.GetDirectories(".git").Any())
        {
            Debug($"Updating {shortName} (url={dep.url})");
            var git = Process.Start(new ProcessStartInfo("git", "pull --no-rebase")
            {
                WorkingDirectory = sub.FullName
            });
            git.WaitForExit();
        }
        // Clone
        else
        {
            Debug($"Cloning {shortName} (url={dep.url})");
            var git = Process.Start(new ProcessStartInfo("git", $"clone {dep.url} .")
            {
                WorkingDirectory = sub.FullName
            });
            git.WaitForExit();
        }
    }
}

void StartPackaging()
{
    var packFolder = Directory.CreateDirectory("dependencies/.nuget");
    // Delete previous versions
    foreach (var file in packFolder.GetFiles("*.nupkg"))
    {
        file.Delete();
    }

    Pack();
    ClearTemporary();
    Restore();

    // Clear nuget packages that were packeted here.
    // They will get regenerated with Restore()
    //
    // The reason why we're doing that is to make sure that a dependency update has been correctly installed on the project.
    // Because it can be possible that a dependency will not change its package version.
    void ClearTemporary()
    {
        var getGlobalFolder = Process.Start(new ProcessStartInfo("dotnet", "nuget locals -l global-packages")
        {
            RedirectStandardOutput = true
        });
        var folder = getGlobalFolder.StandardOutput.ReadLine()
                                                   .Replace("global-packages: ", string.Empty);

        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException(":: " + folder);
        }

        Debug($"Global-Packages folder: {folder}");

        foreach (var file in Directory.CreateDirectory("dependencies/.nuget").GetFiles("*.nupkg"))
        {
            var name = file.Name.ToLower();
            var start = 0;
            for (; start < name.Length; start++)
            {
                if (char.IsNumber(name[start]))
                {
                    break;
                }
            }

            if (start < name.Length)
            {
                name = name.Substring(0, start - 1);
            }

            var target = folder + name;
            if (!Directory.Exists(target))
            {
                Debug($"Temporary Package Folder for {name} does not exist. Skip ({target})");
                continue;
            }

            Console.WriteLine($"Removing temporary package for {name}");
            Directory.Delete(target, true);
        }
    }

    void Pack()
    {
        foreach (var (shortName, dep) in DEPENDENCIES)
        {
            // Create a fake nuget folder for dependency sub-dependencies
            Directory.CreateDirectory($"{dep.Path}/dependencies/.nuget");
        
            Debug($"Building {shortName}");
            var process = Process.Start(new ProcessStartInfo(
                "dotnet", 
                $"pack {dep.Path}/ -o ./dependencies/.nuget/ /p:Version=0.0.0-local"
            ));
            process.WaitForExit();
        }
    }

    void Restore()
    {
        var process = Process.Start("dotnet", "restore");
        process.WaitForExit();
    }
}

void Debug(string msg)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("DEBUG: ");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write(msg);
    Console.WriteLine();
    Console.ResetColor();
}

void Error(string msg)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("ERROR: ");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write(msg);
    Console.WriteLine();
    Console.ResetColor();
}

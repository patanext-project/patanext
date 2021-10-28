var target = Argument("target", "Prebuild");
var configuration = Argument("configuration", "Release");

void DoCommand(string cmd)
{
    var split = cmd.Split(' ');
    var args = ProcessArgumentBuilder.FromString(string.Join(" ", split.Skip(1)));

    using (var process = StartAndReturnProcess(split[0], new ProcessSettings { Arguments = args }))
    {
        process.WaitForExit();
        Information("(cmd '{1}') Exit code: {0}", process.GetExitCode(), cmd);
        Information(split[0]);
        Information(string.Join("", split.Skip(1)));
    }
}

void DoCommand(string cmd, DirectoryPath workDir)
{
    var split = cmd.Split();
    var args = ProcessArgumentBuilder.FromString(string.Join(" ", split.Skip(1)));

    using (var process = StartAndReturnProcess(split[0], new ProcessSettings 
        { 
            Arguments = args,
            WorkingDirectory = workDir
        }))
    {
        process.WaitForExit();
        Information("(cmd '{1}') Exit code: {0}", process.GetExitCode(), cmd);
    }
}

Task("Clean Cache Folder")
    .Does(() => 
{
    Console.WriteLine("yes");
});

Task("Prebuild")
    .IsDependentOn("Clean Cache Folder")
    .Does(() =>
{
    Console.WriteLine("prebuild");
    
    var workDir = MakeAbsolute(Directory("src/"));
    
    void PackGithubProject(string gitLink)
    {
        DoCommand($"dotnet pack paket-files/github.com/{gitLink}/ -o ./packages --version-suffix 42", workDir);
    }
    
    DoCommand("dotnet paket install");
    DoCommand("dotnet paket update");
    PackGithubProject("guerro323/GameHost");
    PackGithubProject("guerro323/revtask");
    PackGithubProject("guerro323/revecs");
    PackGithubProject("guerro323/GodotProxy");
    DoCommand("dotnet restore");
});

RunTarget(target);
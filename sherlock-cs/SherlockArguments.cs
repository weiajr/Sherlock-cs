using CommandDotNet;

namespace sherlock_cs;

internal class SherlockArguments : IArgumentModel
{
    [Operand("username")] public required string[] Usernames { get; set; }
    [Option('o', "output", Description = "If using single username, the output of the result will be saved to this file.")] public string? OutputFile { get; set; }
    [Option('f', "folderoutput", Description = "If using multiple usernames, the output of the results will be saved to this folder.")] public string? OutputFolder { get; set; }
    [Option('c', "concurrency", Description = "The degree of concurrency used more is faster")] public int Concurrency { get; set; } = 256;
    [Option("timeout", Description = "Time (in seconds) to wait for response to requests ")] public double Timeout { get; set; } = 60;
    [Option("print-all", Description = "Output sites where the username was not found.")] public bool PrintAll { get; set; }
    [Option("print-found", Description = "Output sites where the username was found.")] public bool PrintFound { get; set; } = true;
    [Option('l', "local", Description = "Force the use of the local data.json file.")] public bool Local { get; set; }
    [Option("nsfw", Description = "Include checking of NSFW sites from default list.")] public bool Nsfw { get; set; }
    [Option("user-agent", Description = "Force the use of a custom user agent.")] public string UserAgent { get; set; } = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.12; rv:55.0) Gecko/20100101 Firefox/55.0";
}
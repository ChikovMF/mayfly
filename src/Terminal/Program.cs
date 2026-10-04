using System.CommandLine;

var rootCommand = new RootCommand("Share files by link.");

return rootCommand.Parse(args).Invoke();

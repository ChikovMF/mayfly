using System.CommandLine;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Completion;

internal sealed class CompletionCommand : Command
{
    public CompletionCommand() : base("completion", Resources.CompletionDescription)
    {
        var shellArgument = new Argument<string>("shell") { Description = Resources.CompletionShellArgumentDescription };
        shellArgument.AcceptOnlyFromAmong(CompletionAction.Shells);

        Arguments.Add(shellArgument);

        SetAction(parseResult => CompletionAction.Execute(parseResult.GetRequiredValue(shellArgument)));
    }
}

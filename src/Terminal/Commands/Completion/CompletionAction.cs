using Mayfly.Terminal.Output;

namespace Mayfly.Terminal.Commands.Completion;

internal static class CompletionAction
{
    public static readonly string[] Shells = ["bash", "zsh", "powershell"];

    private const string Bash = """
        _mayfly() {
            local cur="$2" line
            COMPREPLY=()
            while IFS= read -r line; do
                [[ "$line" == "$cur"* ]] || continue
                [[ -z "$cur" && "$line" == [-/]* ]] && continue
                COMPREPLY+=("$line")
            done < <("$1" "[suggest:$COMP_POINT]" "$COMP_LINE" 2>/dev/null)
        }
        complete -o default -F _mayfly mayfly
        """;

    private const string Zsh = """
        _mayfly() {
            local line="${(j: :)words[1,CURRENT]}"
            local -a suggestions
            suggestions=("${(@f)$("${words[1]}" "[suggest:${#line}]" "$line" 2>/dev/null)}")
            suggestions=(${suggestions:#})
            [[ -z "$PREFIX" ]] && suggestions=(${suggestions:#[-/]*})
            if (( ${#suggestions} )); then
                compadd -a suggestions
            else
                _files
            fi
        }
        compdef _mayfly mayfly
        """;

    private const string PowerShell = """
        Register-ArgumentCompleter -Native -CommandName mayfly, mayfly.exe -ScriptBlock {
            param($wordToComplete, $commandAst, $cursorPosition)
            $position = $cursorPosition - $commandAst.Extent.StartOffset
            $line = $commandAst.ToString().PadRight($position)
            & $commandAst.GetCommandName() "[suggest:$position]" $line 2>$null |
                Where-Object { $_.StartsWith($wordToComplete) -and ($wordToComplete -or $_ -notmatch '^[-/]') } |
                ForEach-Object { [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_) }
        }
        """;

    public static int Execute(string shell)
    {
        var script = shell switch
        {
            "bash" => Bash,
            "zsh" => Zsh,
            "powershell" => PowerShell,
            _ => throw new ArgumentOutOfRangeException(nameof(shell)),
        };

        // A checkout on Windows can turn the line breaks of these literals into CRLF, which bash and zsh reject.
        ConsoleOutput.Value(script.ReplaceLineEndings("\n"));
        return 0;
    }
}

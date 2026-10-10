using System.Net;
using Mayfly.Client;
using Mayfly.Terminal.Input;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands;

internal static class PasswordRetry
{
    public static async Task<T> RunAsync<T>(Func<string?, Task<T>> request)
    {
        try
        {
            return await request(null);
        }
        catch (MayflyApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            if (!PasswordPrompt.IsAvailable)
            {
                throw new AccessDeniedException(Resources.PasswordPromptUnavailable);
            }
        }

        var password = PasswordPrompt.Read(Resources.PasswordPromptLabel);

        try
        {
            return await request(password);
        }
        catch (MayflyApiException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new AccessDeniedException(Resources.WrongPassword);
        }
    }
}

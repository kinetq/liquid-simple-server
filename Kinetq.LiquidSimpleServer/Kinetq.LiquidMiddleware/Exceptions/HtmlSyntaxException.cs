using HtmlAgilityPack;

namespace Kinetq.LiquidMiddleware.Exceptions;

public class HtmlSyntaxException : Exception
{
    public HtmlSyntaxException(string message) : base(message)
    {
    }

    public HtmlSyntaxException(IEnumerable<HtmlParseError> htmlParseErrors) : this(FormatErrors(htmlParseErrors))
    {

    }

    private static string FormatErrors(IEnumerable<HtmlParseError> htmlParseErrors)
    {
        var errorMessages = htmlParseErrors.Select(error => error.Reason);
        return string.Join(", ", errorMessages);
    }
}
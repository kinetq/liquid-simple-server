namespace Kinetq.LiquidMiddleware.Exceptions;

public class LiquidSyntaxException : Exception
{
    public LiquidSyntaxException(string message) : base(message)
    {
    }
}
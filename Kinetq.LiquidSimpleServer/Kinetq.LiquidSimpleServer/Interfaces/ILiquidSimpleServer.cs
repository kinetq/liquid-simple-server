namespace Kinetq.LiquidSimpleServer.Interfaces;

public interface ILiquidSimpleServer : IDisposable
{
    void Stop();
    Task StartAsync();
    Uri Prefix { get; }
}
using Fluid;
using Kinetq.LiquidSimpleServer.Interfaces;

namespace Kinetq.LiquidSimpleServer.Managers;

public class FluidParserManager : IFluidParserManager
{
    private readonly Lazy<FluidParser> _fluidParsers = new(() => new FluidParser());
    public FluidParser FluidParser => _fluidParsers.Value;
}
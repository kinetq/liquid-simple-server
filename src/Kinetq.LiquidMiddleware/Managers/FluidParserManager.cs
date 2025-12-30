using Fluid;
using Kinetq.LiquidMiddleware.Interfaces;

namespace Kinetq.LiquidMiddleware.Managers;

public class FluidParserManager : IFluidParserManager
{
    private readonly Lazy<FluidParser> _fluidParsers = new(() => new FluidParser());
    public FluidParser FluidParser => _fluidParsers.Value;
}
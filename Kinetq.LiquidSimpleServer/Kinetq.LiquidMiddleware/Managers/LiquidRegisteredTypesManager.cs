using Kinetq.LiquidMiddleware.Interfaces;

namespace Kinetq.LiquidMiddleware.Managers;

public class LiquidRegisteredTypesManager : ILiquidRegisteredTypesManager
{
    private readonly Lazy<IList<Type>> _liquidRegisteredTypes =
        new(() => new List<Type>());

    public IList<Type> RegisteredTypes => _liquidRegisteredTypes.Value;

    public void RegisterType<T>()
    {
        RegisterType(typeof(T));
    }

    public void RegisterType(Type type)
    {
        if (!RegisteredTypes.Contains(type))
        {
            _liquidRegisteredTypes.Value.Add(type);
        }
    }
}
namespace Kinetq.LiquidMiddleware.Interfaces;

public interface ILiquidRegisteredTypesManager
{
    IList<Type> RegisteredTypes { get; }
    void RegisterType(Type type);
    void RegisterType<T>();
}
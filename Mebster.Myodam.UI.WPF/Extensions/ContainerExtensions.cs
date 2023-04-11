using Autofac;
using Autofac.Builder;

namespace Mebster.Myodam.UI.WPF.Extensions;

public static class ContainerExtensions
{
    public static IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> RegisterAsInterfaceSingleton<T>(this ContainerBuilder containerBuilder) 
    {
        return containerBuilder.RegisterAsInterface<T>().SingleInstance();
    }

    public static IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> RegisterAsInterface<T>(this ContainerBuilder containerBuilder)
    {
        return containerBuilder.RegisterType<T>().AsImplementedInterfaces();
    }
}
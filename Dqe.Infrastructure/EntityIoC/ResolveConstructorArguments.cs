namespace Dqe.Infrastructure.EntityIoC
{
    /// <summary>
    /// COMPONENT
    /// </summary>
    /// <param name="sender">IEntityInjector</param>
    /// <param name="args">ResolveConstructorArgumentsArgs</param>
    /// <returns>Array of constructor arguments</returns>
    public delegate object[] ResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args);
}
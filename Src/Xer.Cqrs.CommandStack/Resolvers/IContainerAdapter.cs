namespace Xer.Cqrs.CommandStack.Resolvers
{
    public interface IContainerAdapter
    {
        /// <summary>
        /// Resolve instance from a container.
        /// </summary>
        /// <typeparam name="T">Type of object to resolve from container.</typeparam>
        /// <returns>Instance of requested object.</returns>
        T Resolve<T>() where T : class;
    }
}
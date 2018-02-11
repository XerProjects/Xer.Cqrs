using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xer.Delegator;
using Xer.Delegator.Exceptions;

namespace Xer.Cqrs.CommandStack.Resolvers
{
    public class ContainerCommandAsyncHandlerResolver : IMessageHandlerResolver
    {
        #region Static Declarations

        private static readonly MethodInfo ResolveMessageHandlerOpenGenericMethodInfo = typeof(ContainerCommandAsyncHandlerResolver).GetRuntimeMethod(nameof(ResolveMessageHandler), new Type[] { });
        private static readonly Dictionary<Type, Func<ContainerCommandAsyncHandlerResolver, MessageHandlerDelegate>> _genericResolveDelegatesByMessageType = new Dictionary<Type, Func<ContainerCommandAsyncHandlerResolver, MessageHandlerDelegate>>();       
        private static readonly object _padlock = new object();
        
        #endregion Static Declarations

        #region Declarations

        private readonly IContainerAdapter _containerAdapter;
        private readonly Func<Exception, bool> _exceptionHandler;

        #endregion Declarations

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="containerAdapter">Container adapter.</param>
        /// <param name="resolveExceptionHandler">
        /// Delegate that will execute when an exception occurs while resolving handlers from the container.
        /// If true is returned by exception handler, exception will not be propagated. Otherwise, exception will be propagated.
        /// </param>
        public ContainerCommandAsyncHandlerResolver(IContainerAdapter containerAdapter, Func<Exception, bool> resolveExceptionHandler = null)
        {
            _containerAdapter = containerAdapter;
            _exceptionHandler = resolveExceptionHandler;
        }

        #endregion Constructors

        #region IMessageHandlerResolver Implementation

        /// <summary>
        /// Resolves an instance of <see cref="Xer.Cqrs.CommandStack.ICommandAsyncHandler{TCommand}"/> from the container
        /// and converts it to a message handler delegate which processes the command when invoked.
        /// </summary>
        /// <param name="commandType">Type of command which is handled by the command handler.</param>
        /// <returns>Instance of <see cref="Xer.Delegator.MessageHandlerDelegate"/> which executes the command handler processing when invoked.</returns>
        public MessageHandlerDelegate ResolveMessageHandler(Type commandType) 
        {
            if(!_genericResolveDelegatesByMessageType.TryGetValue(commandType, out Func<ContainerCommandAsyncHandlerResolver, MessageHandlerDelegate> genericResolveDelegate))
            {
                if(!commandType.GetTypeInfo().IsClass)
                {
                    throw new ArgumentException("Command is not a reference type.", nameof(commandType));
                }

                lock(_padlock)
                {
                    // Check after locking thread if another thread has already added a delegate.
                    if(!_genericResolveDelegatesByMessageType.TryGetValue(commandType, out genericResolveDelegate))
                    {
                        // Build and cache delegate.
                        genericResolveDelegate = buildGenericResolveMessageHandlerDelegate(commandType);

                        // Cache.
                        _genericResolveDelegatesByMessageType.Add(commandType, genericResolveDelegate);
                    }
                }
            }

            return genericResolveDelegate.Invoke(this);
        }

        #endregion IMessageHandlerResolver Implementation

        #region Methods
        
        // Note: When renaming or changing signature of this ResolveMessageHandler method, 
        // the cached ResolveMessageHandlerOpenGenericMethodInfo should be updated. 

        /// <summary>
        /// Resolves an instance of <see cref="Xer.Cqrs.CommandStack.ICommandAsyncHandler{TCommand}"/> from the container
        /// and converts it to a message handler delegate which processes the command when invoked.
        /// </summary>
        /// <typeparamref name="commandType">Type of command which is handled by the command handler.</typeparamref>
        /// <returns>Instance of <see cref="Xer.Delegator.MessageHandlerDelegate"/> which executes the command handler processing when invoked.</returns>
        public MessageHandlerDelegate ResolveMessageHandler<TCommand>() where TCommand : class
        {
            try
            {
                // Try to resolve async handler first.
                ICommandAsyncHandler<TCommand> commandAsyncHandler = _containerAdapter.Resolve<ICommandAsyncHandler<TCommand>>();
                if (commandAsyncHandler != null)
                {
                    return CommandHandlerDelegateBuilder.FromCommandHandler(commandAsyncHandler);
                }
            }
            catch(Exception ex)
            {
                bool exceptionHandled = _exceptionHandler?.Invoke(ex) ?? false;
                if(!exceptionHandled)
                {
                    // Exception while resolving handler. Throw exception.
                    throw new NoMessageHandlerResolvedException($"Error encoutered while trying to retrieve an instance of {typeof(ICommandAsyncHandler<TCommand>)} from the container.", typeof(TCommand), ex);
                }
            }
        
            return NullMessageHandlerDelegate.Instance;
        }

        #endregion Methods

        #region Functions

        /// <summary>
        /// Create a delegate that calls the generic resolveMessageHandler function. 
        /// </summary>
        /// <param name="commandType">Type of command.</param>
        /// <returns>Delegate that calls the generic resolveMessageHandler function.</returns>
        private static Func<ContainerCommandAsyncHandlerResolver, MessageHandlerDelegate> buildGenericResolveMessageHandlerDelegate(Type commandType)
        {
            // Create a delegate which calls the ResolveMessageHandler<TCommand> method where TCommand is set to the commandType.
            // Signature: (resolver) => resolver.ResolveMessageHandler<TCommand>();
            ParameterExpression resolverParameter = Expression.Parameter(typeof(ContainerCommandAsyncHandlerResolver), "resolver");
            Expression callGenericResolveMessageHandler = Expression.Call(resolverParameter, ResolveMessageHandlerOpenGenericMethodInfo.MakeGenericMethod(commandType));
            return Expression.Lambda<Func<ContainerCommandAsyncHandlerResolver, MessageHandlerDelegate>>(callGenericResolveMessageHandler, resolverParameter).Compile();
        }

        #endregion Functions
    }
}
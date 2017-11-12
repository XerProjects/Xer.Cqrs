using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack
{
    internal class CommandHandlerDelegateBuilder
    {
        #region From CommandHandler

        internal static CommandHandlerDelegate FromCommandHandler<TCommand>(ICommandAsyncHandler<TCommand> commandAsyncHandler)
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate(async (c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    throw ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType());
                }

                await commandAsyncHandler.HandleAsync(command, ct).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromCommandHandler<TCommand>(ICommandHandler<TCommand> commandHandler)
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate((c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    return TaskUtility.FromException(ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType()));
                }

                try
                {
                    commandHandler.Handle(command);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            });
        }

        #endregion From CommandHandler

        #region From Factory

        internal static CommandHandlerDelegate FromFactory<TCommand>(Func<ICommandAsyncHandler<TCommand>> commandHandlerFactory)
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate(async (c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    throw ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType());
                }

                ICommandAsyncHandler<TCommand> instance = EnsureInstanceFromFactory(commandHandlerFactory);
                
                await instance.HandleAsync(command, ct).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromFactory<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory)
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate((c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    return TaskUtility.FromException(ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType()));
                }
                
                ICommandHandler<TCommand> instance;

                try
                {
                    instance = EnsureInstanceFromFactory(commandHandlerFactory);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }

                try
                {
                    instance.Handle(command);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            });
        }

        #endregion From Factory

        #region From Delegate
        
        internal static CommandHandlerDelegate FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TCommand, Task> asyncAction)
            where TAttributed : class
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate(async (c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    throw ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType());
                }

                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                await asyncAction.Invoke(instance, command).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TCommand, CancellationToken, Task> cancellableAsyncAction)
            where TAttributed : class
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate(async (c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    throw ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType());
                }

                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);

                await cancellableAsyncAction.Invoke(instance, command, ct).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Action<TAttributed, TCommand> action)
            where TAttributed : class
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate((c, ct) =>
            {
                TCommand command = c as TCommand;
                if (command == null)
                {
                    return TaskUtility.FromException(ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), c.GetType()));
                }

                TAttributed instance;

                try
                {
                    instance = EnsureInstanceFromFactory(attributedObjectFactory);
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }

                try
                {
                    action.Invoke(instance, command);
                    return TaskUtility.CompletedTask;
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
                }
            });
        }

        #endregion From Delegate

        #region Functions

        private static TInstance EnsureInstanceFromFactory<TInstance>(Func<TInstance> factory)
        {
            try
            {
                TInstance instance = factory.Invoke();

                if (instance == null)
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>();
                }

                return instance;
            }
            catch (Exception ex)
            {
                throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>(ex);
            }
        }

        #endregion Functions
    }
}

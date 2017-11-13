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
            return new CommandHandlerDelegate(async (inputCommand, ct) =>
            {
                TCommand command = EnsureValidCommand<TCommand>(inputCommand);
                await commandAsyncHandler.HandleAsync(command, ct).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromCommandHandler<TCommand>(ICommandHandler<TCommand> commandHandler)
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate((inputCommand, ct) =>
            {
                TCommand command;
                try
                {
                    command = EnsureValidCommand<TCommand>(inputCommand);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException(ex);
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
            return new CommandHandlerDelegate(async (inputCommand, ct) =>
            {
                TCommand command = EnsureValidCommand<TCommand>(inputCommand);
                ICommandAsyncHandler<TCommand> instance = EnsureInstanceFromFactory(commandHandlerFactory);
                
                await instance.HandleAsync(command, ct).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromFactory<TCommand>(Func<ICommandHandler<TCommand>> commandHandlerFactory)
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate((inputCommand, ct) =>
            {
                TCommand command;
                try
                {
                    command = EnsureValidCommand<TCommand>(inputCommand);
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException(ex);
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
            return new CommandHandlerDelegate(async (inputCommand, ct) =>
            {
                TCommand command = EnsureValidCommand<TCommand>(inputCommand);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                await asyncAction.Invoke(instance, command).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TCommand, CancellationToken, Task> cancellableAsyncAction)
            where TAttributed : class
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate(async (inputCommand, ct) =>
            {
                TCommand command = EnsureValidCommand<TCommand>(inputCommand);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);

                await cancellableAsyncAction.Invoke(instance, command, ct).ConfigureAwait(false);
            });
        }

        internal static CommandHandlerDelegate FromDelegate<TAttributed, TCommand>(Func<TAttributed> attributedObjectFactory, Action<TAttributed, TCommand> action)
            where TAttributed : class
            where TCommand : class, ICommand
        {
            return new CommandHandlerDelegate((inputCommand, ct) =>
            {
                TCommand command;
                try
                {
                    command = EnsureValidCommand<TCommand>(inputCommand);
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException(ex);
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

        private static TCommand EnsureValidCommand<TCommand>(ICommand inputCommand) where TCommand : class
        {
            if (inputCommand == null)
            {
                throw new ArgumentNullException(nameof(inputCommand));
            }

            TCommand command = inputCommand as TCommand;
            if (command == null)
            {
                throw ExceptionBuilder.InvalidCommandTypeArgumentException(typeof(TCommand), inputCommand.GetType());
            }

            return command;
        }

        #endregion Functions
    }
}

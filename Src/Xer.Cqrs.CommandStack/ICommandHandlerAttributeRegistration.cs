using System;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerAttributeRegistration
    {
        /// <summary>
        /// Register methods marked with the [CommandHandler] attribute as command handlers.
        /// <para>Supported signatures for methods marked with [CommandHandler] are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the object which contains the methods marked with the [CommandHandler] attribute.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will provide an instance of the specified <typeparamref name="TAttributed"/> type.</param>
        void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class;
    }
}
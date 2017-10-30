using System;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandHandlerAttributeRegistration
    {
        /// <summary>
        /// Register methods marked with [CommandHandler] attribute. 
        /// It will make the dispatcher treat the method as command/query handlers.
        /// <para>Supported signatures for [CommandHandler] methods are: (Methods can be named differently)</para>
        /// <para>void HandleCommand(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command);</para>
        /// <para>Task HandleCommandAsync(TCommand command, CancellationToken cancellationToken);</para>
        /// </summary>
        /// <typeparam name="TAttributed">Type of the objects which contains the attributed methods.</typeparam>
        /// <param name="attributedHandlerFactory">Factory which will create the instance of the TAttributed object.</param>
        void Register<TAttributed>(Func<TAttributed> attributedHandlerFactory) where TAttributed : class;
    }
}

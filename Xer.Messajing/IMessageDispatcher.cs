namespace Xer.Messajing
{
    public interface IMessageDispatcher
    {
        /// <summary>
        /// Dispatch the message to the registered message handlers.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to dispatch.</typeparam>
        /// <param name="message">Message to dispatch.</param>
        void Dispatch<TMessage>(TMessage message) where TMessage : IMessage;

        /// <summary>
        /// Dispatch the message to the registered message handlers 
        /// and return message processing result.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of result of message processing.</typeparam>
        /// <param name="message">Message to dispatch.</param>
        /// <returns>Result of message processing.</returns>
        TResult Dispatch<TMessage, TResult>(TMessage message) where TMessage : IMessage<TResult>;
    }
}

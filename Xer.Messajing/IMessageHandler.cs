namespace Xer.Messajing
{
    public interface IMessageHandler<in TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Handle message processing.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        void Handle(TMessage message);
    }

    public interface IMessageHandler<in TMessage, out TResult> where TMessage : IMessage<TResult>
    {
        /// <summary>
        /// Handle message processing and return message processing result.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <returns>Message processing result.</returns>
        TResult Handle(TMessage message);
    }
}

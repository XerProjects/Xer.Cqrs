using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Delegator;
using Xunit.Abstractions;

namespace Xer.Cqrs.CommandStack.Tests.Entities
{
    public class LoggingCommandDelegator : IMessageDelegator
    {
        private readonly IMessageDelegator _inner;
        private readonly ITestOutputHelper _outputHelper;
        
        public LoggingCommandDelegator(IMessageDelegator inner, ITestOutputHelper outputHelper)
        {
            _inner = inner;
            _outputHelper = outputHelper;
        }
        public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : class
        {
            try
            {
                await _inner.SendAsync(message, cancellationToken);
            }
            catch(Exception ex)
            {
                _outputHelper.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}
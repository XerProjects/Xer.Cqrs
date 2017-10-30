using System;
using System.Collections.Generic;
using System.Text;
using Xer.Cqrs.CommandStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Commands.Registration
{
    public class AttributeRegistrationTests
    {
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Should_Not_Allow_Async_Void_CommandHandler_Methods()
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedCommandHandlerWithAsyncVoid(_outputHelper));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public void Should_Not_Sync_Methods_With_Cancellation_Token()
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        var registration = new CommandHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedSyncCommandHandlerWithCancellationToken(_outputHelper));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Register Method
    }
}

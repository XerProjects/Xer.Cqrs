using System;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.Tests.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries.Registration
{
    public class AttributeRegistrationTests
    {
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            [Fact]
            public void Should_Not_Allow_Query_Handlers_With_Void_Return_Type()
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedQueryHandlerNoReturnType(_outputHelper));
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

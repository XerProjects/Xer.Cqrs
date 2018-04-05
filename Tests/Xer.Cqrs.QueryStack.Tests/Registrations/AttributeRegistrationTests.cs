using System;
using FluentAssertions;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Tests.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.QueryStack.Tests.Queries.Registration
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
            public void ShouldNotAllowQueryHandlersWithVoidReturnType()
            {
                Action action = () =>
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
                };

                action.Should().Throw<InvalidOperationException>();
            }
        }

        #endregion Register Method
    }
}

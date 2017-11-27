using Xer.Cqrs.EventSourcing.Tests.Mocks;
using Xunit;

namespace Xer.Cqrs.EventSourcing.Tests
{
    public class ValueObjectTests
    {
        public class EqualsMethod
        {
            [Fact]
            public void Must_Be_True_If_Value_Objects_Are_Equal_By_Value()
            {
                TestValueObject valueObject1 = new TestValueObject("Test", 123);
                TestValueObject valueObject2 = new TestValueObject("Test", 123);

                Assert.True(valueObject1 == valueObject2);
            }

            [Fact]
            public void Must_Be_False_If_Value_Objects_Are_Not_Equal_By_Value()
            {
                TestValueObject valueObject1 = new TestValueObject("Test", 123);
                TestValueObject valueObject2 = new TestValueObject("Test2", 1234);

                Assert.True(valueObject1 != valueObject2);
            }

            [Fact]
            public void Must_Be_False_If_Other_Checked_Against_Null()
            {
                TestValueObject valueObject1 = new TestValueObject("Test", 123);
                TestValueObject valueObject2 = null;

                Assert.True(valueObject1 != valueObject2);
            }
        }

        public class GetHashCodeMethod
        {
            [Fact]
            public void Must_Be_Equal_For_The_Same_Instance()
            {
                TestValueObject valueObject1 = new TestValueObject("Test", 123);

                Assert.True(valueObject1.GetHashCode() == valueObject1.GetHashCode());
            }

            [Fact]
            public void Must_Be_Equal_For_The_Different_Instances_With_Same_Values()
            {
                TestValueObject valueObject1 = new TestValueObject("Test", 123);
                TestValueObject valueObject2 = new TestValueObject("Test", 123);

                Assert.True(valueObject1.GetHashCode() == valueObject2.GetHashCode());
            }

            [Fact]
            public void Must_Not_Be_Equal_For_The_Different_Instances_With_Different_Values()
            {
                TestValueObject valueObject1 = new TestValueObject("Test", 123);
                TestValueObject valueObject2 = new TestValueObject("Test2", 1234);

                Assert.True(valueObject1.GetHashCode() != valueObject2.GetHashCode());
            }
        }
    }
}
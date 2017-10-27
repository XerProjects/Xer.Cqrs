using Xer.Cqrs.QueryStack;

namespace Xer.Cqrs.Tests.Mocks
{
    public class QuerySomething<T> : IQuery<T>
    {
        public T Data { get; set; }

        public QuerySomething(T input)
        {
            Data = input;
        }
    }

    public class QuerySomething : QuerySomething<string>
    {
        public QuerySomething(string input) : base(input)
        {
        }
    }



    public class QuerySomethingWithException : QuerySomething<string>
    {
        public QuerySomethingWithException(string input) : base(input)
        {
        }
    }

    public class QuerySomethingAsync : QuerySomething
    {
        public QuerySomethingAsync(string input) : base(input)
        {
        }
    }

    public class QuerySomethingAsyncWithDelay : QuerySomething
    {
        public int DelayInMilliseconds { get; }

        public QuerySomethingAsyncWithDelay(string input, int delayInMilliseconds)
            : base(input)
        {
            DelayInMilliseconds = delayInMilliseconds;
        }
    }

    public class QuerySomethingNonReferenceType : QuerySomething<int>
    {
        public QuerySomethingNonReferenceType(int input) : base(input)
        {
        }
    }
}

using Xer.Cqrs.QueryStack;

namespace Xer.Cqrs.Tests.Entities
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

    public class QuerySomethingWithDelay : QuerySomething
    {
        public int DelayInMilliseconds { get; }

        public QuerySomethingWithDelay(string input, int delayInMilliseconds)
            : base(input)
        {
            DelayInMilliseconds = delayInMilliseconds;
        }
    }

    public class QuerySomethingWithNonReferenceTypeResult : QuerySomething<int>
    {
        public QuerySomethingWithNonReferenceTypeResult(int input) : base(input)
        {
        }
    }
}

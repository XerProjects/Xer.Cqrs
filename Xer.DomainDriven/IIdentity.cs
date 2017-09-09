using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.DomainDriven
{
    public interface IIdentity<T> : IEquatable<T>
    {
        T Id { get; }
    }
}

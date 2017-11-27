using System;

namespace Xer.DomainDriven
{
    public interface IEntity<TId> where TId : IEquatable<TId>
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        TId Id { get; }

        /// <summary>
        /// Date when object was created.
        /// </summary>
        DateTime Created { get; }

        /// <summary>
        /// Date when object was last updated.
        /// </summary>
        DateTime Updated { get; }
    }
}
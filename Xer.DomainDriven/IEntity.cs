using System;

namespace Xer.DomainDriven
{
    public interface IEntity
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        Guid Id { get; }

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
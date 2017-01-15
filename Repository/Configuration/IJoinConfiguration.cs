using System;

namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Define the basis join configuration.
    /// </summary>
    public interface IJoinConfiguration
    {
        /// <summary>
        /// Gets the type of repository values.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Gets the type of repository left items source.
        /// </summary>
        Type LeftType { get; }

        /// <summary>
        /// Gets the type of repository right items source.
        /// </summary>
        Type RightType { get; }

        /// <summary>
        /// Gets the type of repository join link key.
        /// </summary>
        Type LinkKeyType { get; }

        /// <summary>
        /// Gets the join mode.
        /// </summary>
        JoinMode Mode { get; }

        /// <summary>
        /// Gets the right source name.
        /// </summary>
        string RightSourceName { get; }

        /// <summary>
        /// Gets the right source filter.
        /// </summary>
        Delegate RightFilter { get; }

        /// <summary>
        /// Gets the left link key configuration
        /// </summary>
        KeyConfiguration LeftLinkKey { get; }

        /// <summary>
        /// Gets the right link key configuration
        /// </summary>
        KeyConfiguration RightLinkKey { get; }
    }
}

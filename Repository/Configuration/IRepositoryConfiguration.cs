using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Interface for the repository configuration.
    /// </summary>
    public interface IRepositoryConfiguration
    {
        /// <summary>
        /// Gets the repository name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the keys type.
        /// </summary>
        Type KeyType { get; }

        /// <summary>
        /// Gets the values type.
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Gets the main source type.
        /// </summary>
        Type LeftType { get; }

        /// <summary>
        /// Gets the main source name.
        /// </summary>
        string LeftSourceName { get; }

        /// <summary>
        /// Gets the key getter configuration.
        /// </summary>
        KeyConfiguration Key { get; }

        /// <summary>
        /// Gets the filter on main source.
        /// </summary>
        Delegate LeftFilter { get; }

        /// <summary>
        /// Indicates if the values created by the repository have to be disposed when their are removed. 
        /// </summary>
        bool DisposeWhenValueIsRemoved { get; }

        /// <summary>
        /// Gets dispatcher used to notify all changes.
        /// </summary>
        Action<Action> Dispatcher { get; }

        /// <summary>
        /// Gets the constructor delegate.
        /// </summary>
        Delegate Ctor { get; }

        /// <summary>
        /// Gets the constructor arguments type.
        /// </summary>
        ReadOnlyCollection<Type> CtorArguments { get; }

        /// <summary>
        /// Gets all joins configurations.
        /// </summary>
        IReadOnlyList<IJoinConfiguration> Joins { get; }

        /// <summary>
        /// Gets the storage behavior.
        /// </summary>
        StorageBehavior Behavior { get; }

        /// <summary>
        /// Gets the rolling max number for the Rolling behavior.
        /// </summary>
        int RollingCount { get; }

        /// <summary>
        /// Gets the time interval for the TimeInterval behavior.
        /// </summary>
        TimeSpan TimeInterval { get; }

        /// <summary>
        /// Gets the timestamp delegate for the TimeInterval behavior.
        /// </summary>
        Delegate GetTimestamp { get; }
    }
}

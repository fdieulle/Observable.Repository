﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Observable.Repository.Join;

namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Repository configuration to initialize and configure the repository.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys.</typeparam>
    /// <typeparam name="TValue">Type of repository values.</typeparam>
    /// <typeparam name="TLeft">Type of repository main source.</typeparam>
    public class RepositoryConfiguration<TKey, TValue, TLeft> : IRepositoryConfiguration
    {
        private readonly KeyConfiguration _keyConfiguration;
        private readonly List<IJoin<TKey, TValue, TLeft>> _joins = new List<IJoin<TKey, TValue, TLeft>>(); 

        /// <summary>
        /// Gets the left key getter delegate.
        /// </summary>
        public Func<TLeft, TKey> GetKey { get; }

        /// <summary>
        /// Gets the constructor value delegate.
        /// </summary>
        public Func<TLeft, object[], TValue> Ctor { get; set; }

        /// <summary>
        /// Gets the update methods delegate.
        /// </summary>
        public Action<TValue, TLeft> OnUpdate { get; set; }

        /// <summary>
        /// Gets the filter on the main source.
        /// </summary>
        public Func<TLeft, bool> LeftFilter { get; }

        /// <summary>
        /// Gets the timestamp used by the TimeInterval behavior.
        /// </summary>
        public Func<TValue, DateTime> GetTimestamp { get; set; }

        /// <summary>
        /// Gets all join configurations.
        /// </summary>
        public IReadOnlyList<IJoin<TKey, TValue, TLeft>> Joins => _joins;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the repository.</param>
        /// <param name="getKey">Main source key getter delegate.</param>
        /// <param name="onUpdate">Define the update method to avoid create a new instance for each updates. Null by default (Null = disabled)</param>
        /// <param name="leftSourceName">Main source name.</param>
        /// <param name="leftFilter">Main source filter.</param>
        /// <param name="disposeWhenValueIsRemoved">Indicates if the values created by the repository have to be disposed when their are removed.</param>
        /// <param name="dispatcher">Dispatcher used to notify all changes.</param>
        public RepositoryConfiguration(
            string name, 
            Func<TLeft, TKey> getKey,
            Action<TValue, TLeft> onUpdate,
            string leftSourceName, 
            Func<TLeft, bool> leftFilter,
            bool disposeWhenValueIsRemoved,
            Action<Action> dispatcher)
        {
            KeyType = typeof (TKey);
            ValueType = typeof (TValue);
            LeftType = typeof (TLeft);

            Name = name;
            GetKey = getKey;
            _keyConfiguration = new KeyConfiguration<TLeft, TKey>(getKey);
            OnUpdate = onUpdate;
            LeftSourceName = leftSourceName;
            LeftFilter = leftFilter;
            DisposeWhenValueIsRemoved = disposeWhenValueIsRemoved;
            Dispatcher = dispatcher;
        }

        #region Implementation of IRepositoryConfiguration

        /// <summary>
        /// Gets the repository name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the keys type.
        /// </summary>
        public Type KeyType { get; }

        /// <summary>
        /// Gets the values type.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Gets the main source type.
        /// </summary>
        public Type LeftType { get; }

        /// <summary>
        /// Gets the main source name.
        /// </summary>
        public string LeftSourceName { get; }

        /// <summary>
        /// Gets the key getter configuration.
        /// </summary>
        public KeyConfiguration Key => _keyConfiguration;

        /// <summary>
        /// Gets the filter on main source.
        /// </summary>
        Delegate IRepositoryConfiguration.LeftFilter => LeftFilter;

        /// <summary>
        /// Indicates if the values created by the repository have to be disposed when their are removed. 
        /// </summary>
        public bool DisposeWhenValueIsRemoved { get; }

        /// <summary>
        /// Gets dispatcher used to notify all changes.
        /// </summary>
        public Action<Action> Dispatcher { get; }

        /// <summary>
        /// Gets the constructor delegate.
        /// </summary>
        Delegate IRepositoryConfiguration.Ctor => Ctor;

        /// <summary>
        /// Gets the constructor arguments type.
        /// </summary>
        public ReadOnlyCollection<Type> CtorArguments { get; set; }

        /// <summary>
        /// Gets all joins configurations.
        /// </summary>
        IReadOnlyList<IJoinConfiguration> IRepositoryConfiguration.Joins => _joins;

        /// <summary>
        /// Gets the storage behavior.
        /// </summary>
        public StorageBehavior Behavior { get; set; }

        /// <summary>
        /// Gets the rolling max number for the Rolling behavior.
        /// </summary>
        public int RollingCount { get; set; }

        /// <summary>
        /// Gets the time interval for the TimeInterval behavior.
        /// </summary>
        public TimeSpan TimeInterval { get; set; }

        /// <summary>
        /// Gets the timestamp delegate for the TimeInterval behavior.
        /// </summary>
        Delegate IRepositoryConfiguration.GetTimestamp => GetTimestamp;

        #endregion

        /// <summary>
        /// Add a join on the configuration.
        /// </summary>
        /// <param name="join">Join to add.</param>
        public void AddJoin(IJoin<TKey, TValue, TLeft> join) => _joins.Add(@join);
    }
}

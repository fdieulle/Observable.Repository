using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Observable.Repository.Configuration;

namespace Observable.Repository.Builders
{
    #region Interfaces

    /// <summary>
    /// Builder interface to get the next step which define the link key between 2 sources.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinBuilderNode<TKey, TValue, TLeft, TRight>
    {
        /// <summary>
        /// Get next building step.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <returns>Returns the next building step</returns>
        IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> GetNext<TLinkKey>();
    }

    /// <summary>
    /// Builder interface which define the link key between 2 sources.
    /// And allow to build the Join configuration.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key between 2 sources</typeparam>
    public interface IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey>
    {
        /// <summary>
        /// Gets the <see cref="IRepositoryContainer"/>
        /// </summary>
        IRepositoryContainer Container { get; }

        /// <summary>
        /// Gets the building repository configuration.
        /// </summary>
        RepositoryConfiguration<TKey, TValue, TLeft> Configuration { get; }

        /// <summary>
        /// Gets or sets the link key getter from right items source.
        /// </summary>
        Func<TRight, TLinkKey> GetRightLinkKey { get; set; }

        /// <summary>
        /// Gets or sets the link key getter from left items source.
        /// </summary>
        Func<TLeft, TLinkKey> GetLeftLinkKey { get; set; }

        /// <summary>
        /// Build the Join configuration.
        /// </summary>
        void Build();
    }

    /// <summary>
    /// Builder interface which define the update method on repository values.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight>
    {
        /// <summary>
        /// Gets the update methods on the repository values.
        /// </summary>
        Func<TValue, Action<TRight>> OnUpdate { get; set; }

        /// <summary>
        /// Get next building step.
        /// </summary>
        /// <returns>Returns the next building step</returns>
        IJoinBuilderNode<TKey, TValue, TLeft, TRight> GetNext();
    }

    /// <summary>
    /// Builder interface which define the property list getter on repository values.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinManyBuilderNode<TKey, TValue, TLeft, TRight>
    {
        /// <summary>
        /// Gets or sets the property list getter delegate.
        /// </summary>
        Func<TValue, IList<TRight>> GetList { get; set; }

        /// <summary>
        /// Gets the next building step.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right primary key used to populate the list.</typeparam>
        /// <returns>Returns the next builder step.</returns>
        IJoinManyBuilderNode<TKey, TValue, TLeft, TRight, TRightKey> GetNext<TRightKey>();
    }

    /// <summary>
    /// Builder interface which define the right key getter used to populate the list.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRightKey">Type of right key.</typeparam>
    public interface IJoinManyBuilderNode<TKey, TValue, TLeft, TRight, TRightKey>
    {
        /// <summary>
        /// Gets or sets the right key getter delegate
        /// </summary>
        Func<TRight, TRightKey> GetRightKey { get; set; }

        /// <summary>
        /// Gets the next building step.
        /// </summary>
        /// <returns>Returns the next builder step.</returns>
        IJoinBuilderNode<TKey, TValue, TLeft, TRight> GetNext();
    }

    #region Depth1

    /// <summary>
    /// Repository Join builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    public interface IRepositoryJoinBuilder<TKey, TValue, out TLeft> : IRepositoryCtorBuilder<TKey, TValue, TLeft>
    {
        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        IJoinUpdateBuilder<TKey, TValue, TLeft, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        IJoinManyBuilder<TKey, TValue, TLeft, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinRightKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    public interface IJoinLeftKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    #endregion 

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinUpdateBuilder<TKey, TValue, out TLeft, out TRight>
    {
        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate);
    }

    #endregion

    #region Join many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinManyBuilder<TKey, TValue, out TLeft, TRight>
    {
        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight> DefineList(Func<TValue, IList<TRight>> getList);
    }

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public interface IJoinManyRightKeyBuilder<TKey, TValue, out TLeft, out TRight>
    {
        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey);
    }

    #endregion

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    public interface IJoinRightKeyBuilder<TKey, TValue, out TLeft, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        IJoinLeftKeyBuilder<TKey, TValue, TLeft, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    public interface IJoinLeftKeyBuilder<TKey, TValue, out TLeft, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    /// <summary>
    /// Repository constructor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    public interface IRepositoryCtorBuilder<TKey, TValue, out TLeft> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TValue> ctor);
    }

    #endregion

    #region Depth2

    /// <summary>
    /// Repository Join builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public interface IRepositoryJoinBuilder<TKey, TValue, out TLeft, out TRight1> : IRepositoryCtorBuilder<TKey, TValue, TLeft, TRight1>
    {
        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public interface IJoinRightKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public interface IJoinLeftKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    #endregion 

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    public interface IJoinUpdateBuilder<TKey, TValue, out TLeft, out TRight1, out TRight>
    {
        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate);
    }

    #endregion

    #region Join many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    public interface IJoinManyBuilder<TKey, TValue, out TLeft, out TRight1, TRight>
    {
        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> DefineList(Func<TValue, IList<TRight>> getList);
    }

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    public interface IJoinManyRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight>
    {
        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey);
    }

    #endregion

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public interface IJoinRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public interface IJoinLeftKeyBuilder<TKey, TValue, out TLeft, out TRight1, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    /// <summary>
    /// Repository constructor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public interface IRepositoryCtorBuilder<TKey, TValue, out TLeft, out TRight1> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TValue> ctor);
    }

    #endregion

    #region Depth3

    /// <summary>
    /// Repository Join builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public interface IRepositoryJoinBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2> : IRepositoryCtorBuilder<TKey, TValue, TLeft, TRight1, TRight2>
    {
        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public interface IJoinRightKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public interface IJoinLeftKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    #endregion 

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    public interface IJoinUpdateBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight>
    {
        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate);
    }

    #endregion

    #region Join many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    public interface IJoinManyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, TRight>
    {
        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> DefineList(Func<TValue, IList<TRight>> getList);
    }

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    public interface IJoinManyRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight>
    {
        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey);
    }

    #endregion

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public interface IJoinRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public interface IJoinLeftKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    /// <summary>
    /// Repository constructor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public interface IRepositoryCtorBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TValue> ctor);
    }

    #endregion

    #region Depth4

    /// <summary>
    /// Repository Join builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public interface IRepositoryJoinBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3> : IRepositoryCtorBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3>
    {
        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public interface IJoinRightKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public interface IJoinLeftKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    #endregion 

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    public interface IJoinUpdateBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight>
    {
        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate);
    }

    #endregion

    #region Join many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    public interface IJoinManyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, TRight>
    {
        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> DefineList(Func<TValue, IList<TRight>> getList);
    }

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    public interface IJoinManyRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight>
    {
        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey);
    }

    #endregion

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public interface IJoinRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public interface IJoinLeftKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    /// <summary>
    /// Repository constructor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public interface IRepositoryCtorBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TRight3, TValue> ctor);
    }

    #endregion

    #region Depth5

    /// <summary>
    /// Repository Join builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public interface IRepositoryJoinBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4> : IRepositoryCtorBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4>
    {
        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public interface IJoinRightKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public interface IJoinLeftKeyToBuildBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    #endregion 

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    public interface IJoinUpdateBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight>
    {
        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate);
    }

    #endregion

    #region Join many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    public interface IJoinManyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, TRight>
    {
        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> DefineList(Func<TValue, IList<TRight>> getList);
    }

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    public interface IJoinManyRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight>
    {
        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey);
    }

    #endregion

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public interface IJoinRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public interface IJoinLeftKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    /// <summary>
    /// Repository constructor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public interface IRepositoryCtorBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TRight3, TRight4, TValue> ctor);
    }

    #endregion

    #region Last Depth

    /// <summary>
    /// Repository Join builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public interface IRepositoryJoinBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5> : IRepositoryCtorBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5>
    {
        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null);
    }

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    /// <typeparam name="TRight5">Type of repository join source 5</typeparam>
    public interface IJoinUpdateBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5, out TRight>
    {
        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate);
    }

    #endregion

    #region Join many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    /// <typeparam name="TRight5">Type of repository join source 5</typeparam>
    public interface IJoinManyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5, TRight>
    {
        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> DefineList(Func<TValue, IList<TRight>> getList);
    }

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    /// <typeparam name="TRight5">Type of repository join source 5s</typeparam>
    public interface IJoinManyRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5, out TRight>
    {
        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey);
    }

    #endregion

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public interface IJoinRightKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5, out TRight>
    {
        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey);
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public interface IJoinLeftKeyBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5, in TLinkKey>
    {
        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5> LeftLinkKey(Func<TLeft, TLinkKey> getKey);
    }

    /// <summary>
    /// Repository constructor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public interface IRepositoryCtorBuilder<TKey, TValue, out TLeft, out TRight1, out TRight2, out TRight3, out TRight4, out TRight5> : IRepositoryBehaviorBuilder<TKey, TValue>
    {
        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TValue> ctor);
    }

    #endregion // Last Depth

    #endregion // Interfaces

    #region Classes

    #region Depth1

    /// <summary>
    /// Repository Join and ctor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    public class RepositoryJoinBuilder<TKey, TValue, TLeft> : RepositoryBuilderFinalizer<TKey, TValue, TLeft>, IRepositoryJoinBuilder<TKey, TValue, TLeft>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        public RepositoryJoinBuilder(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
            : base(container, configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryCtorBuilder<TKey,TValue,out TLeft>

        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        public IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TValue> ctor)
        {
            _configuration.Ctor = (left, rights) => ctor(left);
            _configuration.CtorArguments = new ReadOnlyCollection<Type>(new[] { typeof(TLeft) });
            return this;
        }

        #endregion

        #region Implementation of IRepositoryJoinBuilder<TKey,TValue,out TLeft>

        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        public IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight>(new JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        public IJoinUpdateBuilder<TKey, TValue, TLeft, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight>(new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        public IJoinManyBuilder<TKey, TValue, TLeft, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight>(new JoinManyBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        #endregion
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public class RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight> : IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        public IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    public class RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey> : IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion 

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public class RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight> : IJoinUpdateBuilder<TKey, TValue, TLeft, TRight>
    {
        private readonly IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous step builder.</param>
        public RepositoryJoinUpdateBuilder(IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinUpdateBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate)
        {
            _builder.OnUpdate = onUpdate;
            var next = _builder.GetNext();
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight>(next);
        }

        #endregion
    }

    #endregion // Join to update

    #region Join Many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    public class RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight> : IJoinManyBuilder<TKey, TValue, TLeft, TRight>, IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight>
    {
        private readonly IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous builder step.</param>
        public RepositoryJoinManyBuilder(IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinManyBuilder<TKey,TValue,out TLeft,TRight>

        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        public IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight> DefineList(Func<TValue, IList<TRight>> getList)
        {
            _builder.GetList = getList;
            return this;
        }

        #endregion

        #region Implementation of IJoinManyRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey)
        {
            var next = _builder.GetNext<TRightKey>();
            next.GetRightKey = getKey;
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight>(next.GetNext());
        }

        #endregion
    }

    #endregion // JoinMany

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    public class RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight> : IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        public IJoinLeftKeyBuilder<TKey, TValue, TLeft, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    public class RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight, TLinkKey> : IJoinLeftKeyBuilder<TKey, TValue, TLeft, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion // Depth1

    #region Depth2

    /// <summary>
    /// Repository Join and ctor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public class RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1> : RepositoryBuilderFinalizer<TKey, TValue, TLeft>, IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        public RepositoryJoinBuilder(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
            : base(container, configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryCtorBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        public IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TValue> ctor)
        {
            _configuration.Ctor = (left, rights) => ctor(left, (TRight1)rights[0]);
            _configuration.CtorArguments = new ReadOnlyCollection<Type>(new[] { typeof(TLeft), typeof(TRight1) });
            return this;
        }

        #endregion

        #region Implementation of IRepositoryJoinBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        public IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight>(new JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        public IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight>(new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        public IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight>(new JoinManyBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        #endregion
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public class RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight> : IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        public IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public class RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey> :
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    public class RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight> : IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight>
    {
        private readonly IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous step builder.</param>
        public RepositoryJoinUpdateBuilder(IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinUpdateBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate)
        {
            _builder.OnUpdate = onUpdate;
            var next = _builder.GetNext();
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight>(next);
        }

        #endregion
    }

    #endregion // Join to update

    #region Join Many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    public class RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight> : IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight>, IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight>
    {
        private readonly IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous builder step.</param>
        public RepositoryJoinManyBuilder(IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinManyBuilder<TKey,TValue,out TLeft,TRight>

        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        public IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> DefineList(Func<TValue, IList<TRight>> getList)
        {
            _builder.GetList = getList;
            return this;
        }

        #endregion

        #region Implementation of IJoinManyRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey)
        {
            var next = _builder.GetNext<TRightKey>();
            next.GetRightKey = getKey;
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight>(next.GetNext());
        }

        #endregion
    }

    #endregion // JoinMany

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public class RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight> : IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyBuilder<TKey,TValue,out TLeft,out TRight1,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        public IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    public class RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight, TLinkKey> : IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #region Depth3

    /// <summary>
    /// Repository Join and ctor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public class RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2> : RepositoryBuilderFinalizer<TKey, TValue, TLeft>, IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        public RepositoryJoinBuilder(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
            : base(container, configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryCtorBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        public IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TValue> ctor)
        {
            _configuration.Ctor = (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1]);
            _configuration.CtorArguments = new ReadOnlyCollection<Type>(new[] { typeof(TLeft), typeof(TRight1), typeof(TRight2) });
            return this;
        }

        #endregion

        #region Implementation of IRepositoryJoinBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        public IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>(new JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        public IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>(new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        public IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>(new JoinManyBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        #endregion
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public class RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> : IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        public IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public class RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey> :
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion
    
    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    public class RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> : IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>
    {
        private readonly IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous step builder.</param>
        public RepositoryJoinUpdateBuilder(IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinUpdateBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate)
        {
            _builder.OnUpdate = onUpdate;
            var next = _builder.GetNext();
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>(next);
        }

        #endregion
    }

    #endregion // Join to update

    #region Join Many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    public class RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> : IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>, IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>
    {
        private readonly IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous builder step.</param>
        public RepositoryJoinManyBuilder(IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinManyBuilder<TKey,TValue,out TLeft,TRight>

        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        public IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> DefineList(Func<TValue, IList<TRight>> getList)
        {
            _builder.GetList = getList;
            return this;
        }

        #endregion

        #region Implementation of IJoinManyRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey)
        {
            var next = _builder.GetNext<TRightKey>();
            next.GetRightKey = getKey;
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>(next.GetNext());
        }

        #endregion
    }

    #endregion // JoinMany

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public class RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight> : IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyBuilder<TKey,TValue,out TLeft, TRight1,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        public IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    public class RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight, TLinkKey> : IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #region Depth4

    /// <summary>
    /// Repository Join and ctor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public class RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3> : RepositoryBuilderFinalizer<TKey, TValue, TLeft>, IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        public RepositoryJoinBuilder(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
            : base(container, configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryCtorBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        public IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TRight3, TValue> ctor)
        {
            _configuration.Ctor = (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1], (TRight3)rights[2]);
            _configuration.CtorArguments = new ReadOnlyCollection<Type>(new[] { typeof(TLeft), typeof(TRight1), typeof(TRight2), typeof(TRight3) });
            return this;
        }

        #endregion

        #region Implementation of IRepositoryJoinBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        public IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>(new JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        public IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>(new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        public IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>(new JoinManyBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        #endregion
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public class RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> : IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        public IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public class RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey> :
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion
    
    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    public class RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> : IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>
    {
        private readonly IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous step builder.</param>
        public RepositoryJoinUpdateBuilder(IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinUpdateBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate)
        {
            _builder.OnUpdate = onUpdate;
            var next = _builder.GetNext();
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>(next);
        }

        #endregion
    }

    #endregion // Join to update

    #region Join Many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    public class RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> : IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>, IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>
    {
        private readonly IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous builder step.</param>
        public RepositoryJoinManyBuilder(IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinManyBuilder<TKey,TValue,out TLeft,TRight>

        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        public IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> DefineList(Func<TValue, IList<TRight>> getList)
        {
            _builder.GetList = getList;
            return this;
        }

        #endregion

        #region Implementation of IJoinManyRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey)
        {
            var next = _builder.GetNext<TRightKey>();
            next.GetRightKey = getKey;
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>(next.GetNext());
        }

        #endregion
    }

    #endregion // JoinMany

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public class RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight> : IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyBuilder<TKey,TValue,out TLeft, TRight1,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        public IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    public class RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight, TLinkKey> : IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #region Depth5

    /// <summary>
    /// Repository Join and ctor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public class RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4> : RepositoryBuilderFinalizer<TKey, TValue, TLeft>, IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        public RepositoryJoinBuilder(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
            : base(container, configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryCtorBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        public IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TRight3, TRight4, TValue> ctor)
        {
            _configuration.Ctor = (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1], (TRight3)rights[2], (TRight4)rights[3]);
            _configuration.CtorArguments = new ReadOnlyCollection<Type>(new[] { typeof(TLeft), typeof(TRight1), typeof(TRight2), typeof(TRight3), typeof(TRight4) });
            return this;
        }

        #endregion

        #region Implementation of IRepositoryJoinBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Add <see cref="JoinMode.OneToBuild"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToBuild"/> builder.</returns>
        public IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> Join<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>(new JoinOneToBuildBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        public IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>(new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        public IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>(new JoinManyBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        #endregion
    }

    #region Join to build

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public class RepositoryJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> : IJoinRightKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate.
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the link key builder for the main source</returns>
        public IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Join link key builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public class RepositoryJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey> :
        IJoinLeftKeyToBuildBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyToBuildBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyToBuildBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate.
        /// </summary>
        /// <param name="getKey">Link key getter delegate.</param>
        /// <returns>Repository join builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    public class RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> : IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>
    {
        private readonly IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous step builder.</param>
        public RepositoryJoinUpdateBuilder(IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinUpdateBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate)
        {
            _builder.OnUpdate = onUpdate;
            var next = _builder.GetNext();
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>(next);
        }

        #endregion
    }

    #endregion // Join to update

    #region Join Many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    public class RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> : IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>, IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>
    {
        private readonly IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous builder step.</param>
        public RepositoryJoinManyBuilder(IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinManyBuilder<TKey,TValue,out TLeft,TRight>

        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        public IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> DefineList(Func<TValue, IList<TRight>> getList)
        {
            _builder.GetList = getList;
            return this;
        }

        #endregion

        #region Implementation of IJoinManyRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey)
        {
            var next = _builder.GetNext<TRightKey>();
            next.GetRightKey = getKey;
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>(next.GetNext());
        }

        #endregion
    }

    #endregion // JoinMany

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public class RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight> : IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyBuilder<TKey,TValue,out TLeft, TRight1,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        public IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    public class RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight, TLinkKey> : IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #region Last Depth

    /// <summary>
    /// Repository Join and ctor builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public class RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5> : RepositoryBuilderFinalizer<TKey, TValue, TLeft>, IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5>
    {
        private readonly IRepositoryContainer _container;
        private readonly RepositoryConfiguration<TKey, TValue, TLeft> _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="configuration">Repository configuration</param>
        public RepositoryJoinBuilder(IRepositoryContainer container, RepositoryConfiguration<TKey, TValue, TLeft> configuration)
            : base(container, configuration)
        {
            _container = container;
            _configuration = configuration;
        }

        #region Implementation of IRepositoryCtorBuilder<TKey,TValue,out TLeft,out TRight1,out TRight2,out TRight3,out TRight4>

        /// <summary>
        /// Define the constructor delegate.
        /// </summary>
        /// <param name="ctor">Constructor delegate</param>
        /// <returns>Repository builder</returns>
        public IRepositoryBehaviorBuilder<TKey, TValue> DefineCtor(Func<TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TValue> ctor)
        {
            _configuration.Ctor = (left, rights) => ctor(left, (TRight1)rights[0], (TRight2)rights[1], (TRight3)rights[2], (TRight4)rights[3], (TRight5)rights[4]);
            _configuration.CtorArguments = new ReadOnlyCollection<Type>(new[] { typeof(TLeft), typeof(TRight1), typeof(TRight2), typeof(TRight3), typeof(TRight4), typeof(TRight5) });
            return this;
        }

        #endregion

        #region Implementation of IRepositoryJoinBuilder<TKey,TValue,out TLeft,out TRight1,out TRight2,out TRight3,out TRight4,out TRight5>

        /// <summary>
        /// Add <see cref="JoinMode.OneToUpdate"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.OneToUpdate"/> builder.</returns>
        public IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> JoinUpdate<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>(new JoinOneToUpdateBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        /// <summary>
        /// Add <see cref="JoinMode.Many"/> join 
        /// </summary>
        /// <typeparam name="TRight">Type of joined source</typeparam>
        /// <param name="rightSourceName">Joined source name.</param>
        /// <param name="rightFilter">Joined source filter</param>
        /// <returns>Returns the <see cref="JoinMode.Many"/> builder.</returns>
        public IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> JoinMany<TRight>(string rightSourceName = null, Func<TRight, bool> rightFilter = null)
        {
            return new RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>(new JoinManyBuilder<TKey, TValue, TLeft, TRight>(_container, _configuration, rightSourceName, rightFilter));
        }

        #endregion
    }

    #region Join to update

    /// <summary>
    /// Join update builder.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    /// <typeparam name="TRight5">Type of repository join source 5</typeparam>
    public class RepositoryJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> : IJoinUpdateBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>
    {
        private readonly IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous step builder.</param>
        public RepositoryJoinUpdateBuilder(IJoinToUpdateBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinUpdateBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the update method on the repository values.
        /// </summary>
        /// <param name="onUpdate">Method delegate to call.</param>
        /// <returns>Next builder step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> DefineUpdate(Func<TValue, Action<TRight>> onUpdate)
        {
            _builder.OnUpdate = onUpdate;
            var next = _builder.GetNext();
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>(next);
        }

        #endregion
    }

    #endregion // Join to update

    #region Join Many

    /// <summary>
    /// Join many builder step.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of repository join source</typeparam>
    /// <typeparam name="TRight1">Type of repository join source 1</typeparam>
    /// <typeparam name="TRight2">Type of repository join source 2</typeparam>
    /// <typeparam name="TRight3">Type of repository join source 3</typeparam>
    /// <typeparam name="TRight4">Type of repository join source 4</typeparam>
    /// <typeparam name="TRight5">Type of repository join source 5</typeparam>
    public class RepositoryJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> : IJoinManyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>, IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>
    {
        private readonly IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="builder">Previous builder step.</param>
        public RepositoryJoinManyBuilder(IJoinManyBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinManyBuilder<TKey,TValue,out TLeft,TRight>

        /// <summary>
        /// Define the list getter to populate it by the repository.
        /// </summary>
        /// <param name="getList">List getter delegate.</param>
        /// <returns>Next builder step.</returns>
        public IJoinManyRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> DefineList(Func<TValue, IList<TRight>> getList)
        {
            _builder.GetList = getList;
            return this;
        }

        #endregion

        #region Implementation of IJoinManyRightKeyBuilder<TKey,TValue,out TLeft,out TRight>

        /// <summary>
        /// Define the right key to manage items in the lists.
        /// </summary>
        /// <typeparam name="TRightKey">Type of right key.</typeparam>
        /// <param name="getKey">Right key getter.</param>
        /// <returns>Returns the next build step.</returns>
        public IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> RightPrimaryKey<TRightKey>(Func<TRight, TRightKey> getKey)
        {
            var next = _builder.GetNext<TRightKey>();
            next.GetRightKey = getKey;
            return new RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>(next.GetNext());
        }

        #endregion
    }

    #endregion // JoinMany

    /// <summary>
    /// Joined source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public class RepositoryJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight> : IJoinRightKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinRightKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinRightKeyBuilder<TKey,TValue,out TLeft, TRight1,out TRight>

        /// <summary>
        /// Define the joined link key getter delegate
        /// </summary>
        /// <typeparam name="TLinkKey">Type of link key</typeparam>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the main source link key builder</returns>
        public IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TLinkKey> RightLinkKey<TLinkKey>(Func<TRight, TLinkKey> getKey)
        {
            var next = _builder.GetNext<TLinkKey>();
            next.GetRightLinkKey = getKey;
            return new RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight, TLinkKey>(next);
        }

        #endregion
    }

    /// <summary>
    /// Main source link key builder
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    /// <typeparam name="TLeft">Type of repository source</typeparam>
    /// <typeparam name="TLinkKey">Type of link key</typeparam>
    /// <typeparam name="TRight">Type of joined source</typeparam>
    /// <typeparam name="TRight1">Type of joined source 1</typeparam>
    /// <typeparam name="TRight2">Type of joined source 2</typeparam>
    /// <typeparam name="TRight3">Type of joined source 3</typeparam>
    /// <typeparam name="TRight4">Type of joined source 4</typeparam>
    /// <typeparam name="TRight5">Type of joined source 5</typeparam>
    public class RepositoryJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TRight, TLinkKey> : IJoinLeftKeyBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5, TLinkKey>
    {
        private readonly IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> _builder;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="builder">Previous step Builder</param>
        public RepositoryJoinLeftKeyBuilder(IJoinBuilderNode<TKey, TValue, TLeft, TRight, TLinkKey> builder)
        {
            _builder = builder;
        }

        #region Implementation of IJoinLeftKeyBuilder<TKey,TValue,out TLeft,out TRight,in TLinkKey>

        /// <summary>
        /// Define the main link key getter delegate
        /// </summary>
        /// <param name="getKey">Link key getter delegate</param>
        /// <returns>Returns the repository builder</returns>
        public IRepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5> LeftLinkKey(Func<TLeft, TLinkKey> getKey)
        {
            _builder.GetLeftLinkKey = getKey;
            _builder.Build();
            return new RepositoryJoinBuilder<TKey, TValue, TLeft, TRight1, TRight2, TRight3, TRight4, TRight5>(_builder.Container, _builder.Configuration);
        }

        #endregion
    }

    #endregion

    #endregion
}

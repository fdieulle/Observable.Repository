namespace Observable.Repository.Builders
{
    /// <summary>
    /// Builder finalizer to create and/or register the building repository.
    /// </summary>
    /// <typeparam name="TKey">Type of repository keys</typeparam>
    /// <typeparam name="TValue">Type of repository values</typeparam>
    public interface IRepositoryBuilderFinalizer<TKey, TValue>
    {
        /// <summary>
        /// Create the instance of the building repository.
        /// </summary>
        /// <returns>An instance of <see cref="IRepository{TKey, TValue}"/></returns>
        IRepository<TKey, TValue> Create();

        /// <summary>
        /// Create an instance and register the building repository in the <see cref="IRepositoryContainer"/>
        /// </summary>
        /// <returns>Returns the <see cref="IRepositoryContainer"/> where the instance is registered.</returns>
        IRepositoryContainer Register();
    }
}

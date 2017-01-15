namespace Observable.Repository.Configuration
{
    /// <summary>
    /// Define the join mode used.
    /// </summary>
    public enum JoinMode
    {
        /// <summary>
        /// Means join another source to build a new repository value.
        /// </summary>
        OneToBuild,
        /// <summary>
        /// Means join another source to update an existing repository value.
        /// </summary>
        OneToUpdate,
        /// <summary>
        /// Means join another source to fill a list from existing repository values.
        /// </summary>
        Many
    }
}

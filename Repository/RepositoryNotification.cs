using System.Collections.Generic;
using System.Linq;

namespace Observable.Repository
{
    /// <summary>
    /// Notification from a producer.
    /// </summary>
    /// <typeparam name="T">Type of data produced.</typeparam>
    public struct RepositoryNotification<T>
    {
        private static readonly IEnumerable<T> emptyCollection = new T[0];

        /// <summary>
        /// Gets notification action of the producer.
        /// </summary>
        public ActionType Action { get; }

        /// <summary>
        /// Gets items added or updated from the producer.
        /// </summary>
        public IEnumerable<T> NewItems { get; }

        /// <summary>
        /// Gets items removed or replaced from the producer.
        /// </summary>
        public IEnumerable<T> OldItems { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">Action of the producer.</param>
        /// <param name="oldItems">Items removed or replaced from the producer.</param>
        /// <param name="newItems">Items added or updated from the producer.</param>
        public RepositoryNotification(ActionType action, IEnumerable<T> oldItems, IEnumerable<T> newItems)
        {
            Action = action;
            OldItems = oldItems ?? emptyCollection;
            NewItems = newItems ?? emptyCollection;
        }

        #region Equality members

        public bool Equals(RepositoryNotification<T> other)
        {
            return Action == other.Action 
                && Equals(NewItems, other.NewItems) 
                && Equals(OldItems, other.OldItems);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RepositoryNotification<T> 
                && Equals((RepositoryNotification<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Action;
                hashCode = (hashCode * 397) ^ NewItems.GetHashCode();
                hashCode = (hashCode * 397) ^ OldItems.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RepositoryNotification<T> left, RepositoryNotification<T> right) 
            => left.Equals(right);

        public static bool operator !=(RepositoryNotification<T> left, RepositoryNotification<T> right) 
            => !left.Equals(right);

        #endregion

        public override string ToString()
        {
            return $"[{Action}] oldCount: {OldItems.Count()}, newCount: {NewItems.Count()}";
        }
    }
}

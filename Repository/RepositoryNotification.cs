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

        private readonly ActionType action;
        private readonly IEnumerable<T> newItems;
        private readonly IEnumerable<T> oldItems; 

        /// <summary>
        /// Gets notification action of the producer.
        /// </summary>
        public ActionType Action { get { return action; } }

        /// <summary>
        /// Gets items added or updated from the producer.
        /// </summary>
        public IEnumerable<T> NewItems { get { return newItems; } }

        /// <summary>
        /// Gets items removed or replaced from the producer.
        /// </summary>
        public IEnumerable<T> OldItems { get { return oldItems; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="action">Action of the producer.</param>
        /// <param name="oldItems">Items removed or replaced from the producer.</param>
        /// <param name="newItems">Items added or updated from the producer.</param>
        public RepositoryNotification(ActionType action, IEnumerable<T> oldItems, IEnumerable<T> newItems)
        {
            this.action = action;
            this.oldItems = oldItems ?? emptyCollection;
            this.newItems = newItems ?? emptyCollection;
        }

        #region Equality members

        public bool Equals(RepositoryNotification<T> other)
        {
            return action == other.action 
                && Equals(newItems, other.newItems) 
                && Equals(oldItems, other.oldItems);
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
                var hashCode = (int)action;
                hashCode = (hashCode * 397) ^ newItems.GetHashCode();
                hashCode = (hashCode * 397) ^ oldItems.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(RepositoryNotification<T> left, RepositoryNotification<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RepositoryNotification<T> left, RepositoryNotification<T> right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("[{0}] oldCount: {1}, newCount: {2}", action, oldItems.Count(), newItems.Count());
        }
    }
}

using System;

namespace Observable.Repository.Producers
{
    public class ProducerKey
    {
        private readonly string name;
        private readonly Type type;
        private readonly int hashCode;

        public ProducerKey(string name, Type type)
        {
            this.name = name ?? string.Empty;
            this.type = type ?? typeof(object);

            unchecked
            {
                hashCode = (this.name.GetHashCode() * 397) ^ this.type.GetHashCode();
            }
        }

        #region Equality members

        private bool Equals(ProducerKey other)
        {
            return string.Equals(name, other.name)
                && type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType()
                && Equals((ProducerKey)obj);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public static bool operator ==(ProducerKey left, ProducerKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProducerKey left, ProducerKey right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}

using System;

namespace Observable.Repository.Producers
{
    public class ProducerKey
    {
        private readonly string _name;
        private readonly Type _type;
        private readonly int _hashCode;

        public ProducerKey(string name, Type type)
        {
            this._name = name ?? string.Empty;
            this._type = type ?? typeof(object);

            unchecked
            {
                _hashCode = (this._name.GetHashCode() * 397) ^ this._type.GetHashCode();
            }
        }

        #region Equality members

        private bool Equals(ProducerKey other)
        {
            return string.Equals(_name, other._name)
                && _type == other._type;
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
            return _hashCode;
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

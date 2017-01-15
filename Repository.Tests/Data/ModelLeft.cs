using System;

namespace Observable.Repository.Tests.Data
{
    public class ModelLeft
    {
        public int PrimaryKey { get; set; }

        public int ForeignKey { get; set; }

        public string Idstr { get; set; }

        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        #region Equality members

        protected bool Equals(ModelLeft other)
        {
            return PrimaryKey == other.PrimaryKey && string.Equals(Idstr, other.Idstr) && string.Equals(Name, other.Name) && ForeignKey == other.ForeignKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModelLeft)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PrimaryKey;
                hashCode = (hashCode * 397) ^ (Idstr != null ? Idstr.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ ForeignKey;
                return hashCode;
            }
        }

        public static bool operator ==(ModelLeft left, ModelLeft right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelLeft left, ModelLeft right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("PK: {0}, Name: {1}", PrimaryKey, Name);
        }
    }
}

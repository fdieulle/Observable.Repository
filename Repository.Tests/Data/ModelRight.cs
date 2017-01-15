namespace Observable.Repository.Tests.Data
{
    public class ModelRight
    {
        public int PrimaryKey { get; set; }

        public string Name { get; set; }

        public int ForeignKey { get; set; }

        #region Equality members

        protected bool Equals(ModelRight other)
        {
            return PrimaryKey == other.PrimaryKey && string.Equals(Name, other.Name) && ForeignKey == other.ForeignKey;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModelRight)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PrimaryKey;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ForeignKey;
                return hashCode;
            }
        }

        public static bool operator ==(ModelRight left, ModelRight right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelRight left, ModelRight right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("PK: {0}, FK: {1}, Name: {2}", PrimaryKey, ForeignKey, Name);
        }
    }
}

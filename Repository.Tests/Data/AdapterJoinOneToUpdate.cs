namespace Observable.Repository.Tests.Data
{
    public class AdapterJoinToUpdate
    {
        public ModelLeft ModelLeft { get; set; }
        public ModelRight ModelRight { get; private set; }

        public AdapterJoinToUpdate(ModelLeft modelLeft)
        {
            ModelLeft = modelLeft;
        }

        public void Update(ModelRight modelRight)
        {
            ModelRight = modelRight;
        }

        #region Equality members

        protected bool Equals(AdapterJoinToUpdate other)
        {
            return Equals(ModelLeft, other.ModelLeft) && Equals(ModelRight, other.ModelRight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AdapterJoinToUpdate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelLeft != null ? ModelLeft.GetHashCode() : 0) * 397) ^ (ModelRight != null ? ModelRight.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AdapterJoinToUpdate left, AdapterJoinToUpdate right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AdapterJoinToUpdate left, AdapterJoinToUpdate right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Left: {0}, Right: {1}", ModelLeft, ModelRight);
        }
    }
}

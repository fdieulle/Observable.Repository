namespace Observable.Repository.Tests.Data
{
    public class AdapterJoin
    {
        public ModelLeft ModelLeft { get; private set; }
        public ModelRight ModelRight { get; private set; }

        public AdapterJoin(ModelLeft modelLeft, ModelRight modelRight)
        {
            ModelLeft = modelLeft;
            ModelRight = modelRight;
        }

        #region Equality members

        protected bool Equals(AdapterJoin other)
        {
            return Equals(ModelLeft, other.ModelLeft) && Equals(ModelRight, other.ModelRight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AdapterJoin)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ModelLeft != null ? ModelLeft.GetHashCode() : 0) * 397) ^ (ModelRight != null ? ModelRight.GetHashCode() : 0);
            }
        }

        public static bool operator ==(AdapterJoin left, AdapterJoin right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AdapterJoin left, AdapterJoin right)
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

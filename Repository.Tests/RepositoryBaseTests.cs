using Observable.Repository.Tests.Data;

namespace Observable.Repository.Tests
{
    public class RepositoryBaseTests
    {
        protected static ModelLeft L(int pk, string name, int fk = 0)
        {
            return new ModelLeft { PrimaryKey = pk, Name = name, ForeignKey = fk };
        }

        protected static ModelRight R(int pk, int fk, string name)
        {
            return new ModelRight { PrimaryKey = pk, ForeignKey = fk, Name = name };
        }

        protected static T[] V<T>(params T[] array)
        {
            return array;
        }
    }
}

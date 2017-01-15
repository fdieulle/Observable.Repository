using System.Collections.Generic;

namespace Observable.Repository.Tests.Data
{
    public class AdapterJoinMany
    {
        public ModelLeft ModelLeft { get; private set; }

        private readonly List<ModelRight> modelRights = new List<ModelRight>();
        public List<ModelRight> ModelRights { get { return modelRights; } }

        public AdapterJoinMany(ModelLeft left)
        {
            ModelLeft = left;
        }
    }
}

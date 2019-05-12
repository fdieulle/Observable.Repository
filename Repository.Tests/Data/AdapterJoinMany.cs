using System.Collections.Generic;

namespace Observable.Repository.Tests.Data
{
    public class AdapterJoinMany
    {
        public ModelLeft ModelLeft { get; }

        private readonly List<ModelRight> _modelRights = new List<ModelRight>();
        public List<ModelRight> ModelRights => _modelRights;

        public AdapterJoinMany(ModelLeft left)
        {
            ModelLeft = left;
        }
    }
}

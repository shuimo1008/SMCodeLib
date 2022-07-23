using SMCore.Objects;

namespace SMCore.Models
{
    public abstract class BaseData : ObjectBase
    {
        protected string _guid;

        public string Guid
        {
            get => _guid;
            protected set => _guid = value;
        }

        public BaseData(string guid) => Guid = guid;
    }
}



using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public abstract class TypeModel
    {
        protected TypeModel(string name)
        {
            Name = name;
        }

        public string Name { get; }

        private Type _type;

        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = Type.GetType(Name);

                return _type;
            }
        }

        public IList<string> InterfaceNames { get; set; }
        public IList<InterfaceModel> Interfaces { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (TypeModel) obj;
            return other.Name == Name;
        }

        public override string ToString()
        {
            return Name.Split(',')[0];
        }
    }
}
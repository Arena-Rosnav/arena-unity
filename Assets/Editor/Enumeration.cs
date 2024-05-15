using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PNM
{

    public abstract class Enumeration<T> : IComparable<T> where T : Enumeration<T>
    {
        public readonly string Name;
        public readonly int Id;
        protected Enumeration(int id, string name) => (Id, Name) = (id, name);

        public override string ToString() => Name;

        public static IEnumerable<T> GetAll() =>
            typeof(T).GetFields(BindingFlags.Public |
                                BindingFlags.Static |
                                BindingFlags.DeclaredOnly)
                     .Select(f => f.GetValue(null))
                     .Cast<T>();

        public override bool Equals(object obj)
        {
            if (obj is not Enumeration<T> otherValue)
            {
                return false;
            }

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = Id.Equals(otherValue.Id);

            return typeMatches && valueMatches;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public int CompareTo(T other) => Id.CompareTo(((Enumeration<T>)other).Id);

        public int CompareTo(int other) => Id.CompareTo(other);

    }
}

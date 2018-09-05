using Microsoft.AspNetCore.Routing;
using System;

namespace AspNetCore.Hateoas.Infrastructure
{
    public class ResourceLink<T> : ILinksRequirement
    {
        private readonly Func<T, bool> _isMatch;
        private readonly Func<T, RouteValueDictionary> _values;

        public ResourceLink(string name, Func<T, RouteValueDictionary> values, Func<T, bool> isMatch = null)
        {
            _isMatch = isMatch;
            this.ResourceType = typeof(T);
            this.Name = name;
            this._values = values;
        }

        public string Name { get; }

        public Type ResourceType { get; }

        public RouteValueDictionary GetRouteValues(object input)
        {
            return _values((T)input);
        }

        public bool IsEnabled(object input)
        {
            return _isMatch == null || _isMatch((T)input);
        }

    }
}

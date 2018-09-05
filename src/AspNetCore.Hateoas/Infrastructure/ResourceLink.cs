using Microsoft.AspNetCore.Routing;
using System;

namespace AspNetCore.Hateoas.Infrastructure
{
    public class ResourceLink<T> : ILinksRequirement
    {
        private readonly Func<T, bool> _isMatch;
        private readonly Func<T, RouteValueDictionary> _valuesSelector;

        public ResourceLink(string name, Func<T, RouteValueDictionary> valuesSelector, Func<T, bool> isMatch = null)
        {
            _isMatch = isMatch;
            ResourceType = typeof(T);
            Name = name;
            _valuesSelector = valuesSelector;
        }

        public string Name { get; }

        public Type ResourceType { get; }

        public RouteValueDictionary GetRouteValues(object input)
        {
            return _valuesSelector((T)input);
        }

        public bool IsEnabled(object input)
        {
            return _isMatch == null || _isMatch((T)input);
        }
    }
}

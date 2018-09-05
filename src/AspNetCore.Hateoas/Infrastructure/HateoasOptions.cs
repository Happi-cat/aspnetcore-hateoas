using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace AspNetCore.Hateoas.Infrastructure
{
    public class HateoasOptions
    {
        private readonly List<ILinksRequirement> _links;

        public HateoasOptions()
        {
            _links = new List<ILinksRequirement>();
        }

        public IReadOnlyList<ILinksRequirement> Requirements => _links.AsReadOnly();

        public HateoasOptions AddLink<T>(string routeName) where T : class
        {
            return AddLink<T>(routeName, null);
        }

        public HateoasOptions AddLink<T>(string routeName, Func<T, object> values) where T : class
        {
            return Add(new ResourceLink<T>(routeName, WrapRouteValuesSelector(values)));
        }

        public HateoasOptions AddLinkWhen<T>(string routeName, Func<T, bool> predicate)
            where T : class
        {
            return AddLinkWhen<T>(routeName, predicate, null);
        }

        public HateoasOptions AddLinkWhen<T>(string routeName,
            Func<T, bool> predicate,
            Func<T, object> routeValuesSelector)
            where T : class
        {
            return Add(new ResourceLink<T>(
                routeName,
                WrapRouteValuesSelector(routeValuesSelector),
                predicate
            ));
        }

        private HateoasOptions Add(ILinksRequirement req)
        {
            _links.Add(req);

            return this;
        }

        private Func<T, RouteValueDictionary> WrapRouteValuesSelector<T>(Func<T, object> routeValuesSelector)
        {
            if (routeValuesSelector == null) return r => new RouteValueDictionary();
            return r => new RouteValueDictionary(routeValuesSelector(r));
        }
    }

}

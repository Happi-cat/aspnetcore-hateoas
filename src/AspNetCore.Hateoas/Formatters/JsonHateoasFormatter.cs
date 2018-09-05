using AspNetCore.Hateoas.Infrastructure;
using AspNetCore.Hateoas.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Hateoas.Formatters
{
    public class JsonHateoasFormatter : OutputFormatter
    {
        private const string ApplicationJsonHateoas = "application/json+hateoas";
        private const string ApplicationJson = "application/json";

        private readonly JsonSerializerSettings _serializerSettings;

        public JsonHateoasFormatter()
        {
            SupportedMediaTypes.Add(ApplicationJsonHateoas);

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        private T GetService<T>(OutputFormatterWriteContext context)
        {
            return (T)context.HttpContext.RequestServices.GetService(typeof(T));
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var contextAccessor = GetService<IActionContextAccessor>(context);
            var options = GetService<IOptions<HateoasOptions>>(context).Value;
            var actionDescriptorProvider = GetService<IActionDescriptorCollectionProvider>(context);
            var urlHelper = GetService<IUrlHelperFactory>(context)
                .GetUrlHelper(contextAccessor.ActionContext);
            var response = context.HttpContext.Response;

            switch (context.Object)
            {
                case SerializableError error:
                    return WriteErrorAsync(error, response);
                case Resource existingResource:
                    return WriteResourceAsync(existingResource, response);
                default:
                    return WriteResourceAsync(
                        new ResourceFactory(options, actionDescriptorProvider, urlHelper)
                            .CreateResource(context, context.Object),
                        response
                    );
            }
        }

        private Task WriteErrorAsync(SerializableError error, HttpResponse response)
        {
            var errorOutput = JsonConvert.SerializeObject(error, _serializerSettings);
            response.ContentType = ApplicationJson;
            return response.WriteAsync(errorOutput);
        }

        private Task WriteResourceAsync(Resource resource, HttpResponse response)
        {
            var output = JsonConvert.SerializeObject(resource, _serializerSettings);
            response.ContentType = ApplicationJsonHateoas;
            return response.WriteAsync(output);
        }

        private class ResourceFactory
        {
            private readonly HateoasOptions _options;
            private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;
            private readonly IUrlHelper _urlHelper;

            public ResourceFactory(HateoasOptions options,
                IActionDescriptorCollectionProvider actionDescriptorProvider,
                IUrlHelper urlHelper)
            {
                _options = options;
                _actionDescriptorProvider = actionDescriptorProvider;
                _urlHelper = urlHelper;
            }

            public Resource CreateResource(OutputFormatterWriteContext context, object result)
            {
                var (isSequence, elementType) = IsISequence(typeof(IEnumerable<>), context.ObjectType);

                if (!isSequence)
                    return CreateObjectResource(context.ObjectType, context.Object);

                var resourceList = ((IEnumerable<object>) result)
                    .Select(r => CreateObjectResource(elementType, r))
                    .ToList();
                return CreateListResource(context.ObjectType, resourceList);
            }

            private Resource CreateObjectResource(Type type, object value)
            {
                var resource = new ObjectResource(value);

                return AppendLinksToResource(type, resource, false);
            }

            private Resource CreateListResource(Type type, object value)
            {
                var resource = new ListItemResource(value);

                return AppendLinksToResource(type, resource, true);
            }


            private Link CreateLink(ILinksRequirement option, string method, object routeValues)
            {
                var url = _urlHelper.Link(option.Name, routeValues).ToLower();
                var link = new Link(option.Name, url, method);
                return link;
            }

            private Resource AppendLinksToResource(Type type, Resource resource, bool isEnumerable)
            {
                var resourceOptions = _options.Requirements
                    .Where(r => r.ResourceType == type)
                    .Where(r => r.IsEnabled(resource.Data));

                foreach (var option in resourceOptions)
                {
                    var route = _actionDescriptorProvider
                            .ActionDescriptors
                            .Items
                            .FirstOrDefault(i => i.AttributeRouteInfo.Name == option.Name)
                        ?? throw new ArgumentException($"Route with name {option.Name} cannot be found");

                    var method = route
                        .ActionConstraints
                        .OfType<HttpMethodActionConstraint>()
                        .First()
                        .HttpMethods
                        .First();

                    var routeValues = isEnumerable
                        ? default(object)
                        : option.GetRouteValues(resource.Data);

                    var link = CreateLink(option, method, routeValues);
                    resource.Links.Add(link);
                }

                return resource;
            }

            private static (bool, Type) IsISequence(Type sequenceInterface, Type source)
            {
                var type = source.GetInterface(sequenceInterface.Name, false);
                if (type == null && source.IsGenericType && source.GetGenericTypeDefinition() == sequenceInterface)
                {
                    type = source;
                }
                if (type == null)
                {
                    return (false, null);
                }

                var element = type.GetGenericArguments()[0];
                return (!element.IsGenericParameter, element);
            }
        }
    }
}

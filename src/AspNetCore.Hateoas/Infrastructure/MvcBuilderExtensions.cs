﻿using AspNetCore.Hateoas.Formatters;
using AspNetCore.Hateoas.Infrastructure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddHateoas(this IMvcBuilder builder, Action<HateoasOptions> options = null)
        {
            if (options != null)
            {
                builder.Services.Configure(options);
            }
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.AddMvcOptions(o => o.OutputFormatters.Insert(0, new JsonHateoasFormatter()));
            return builder;
        }
    }
}

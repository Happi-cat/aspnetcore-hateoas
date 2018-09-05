using AspNetCore.Hateoas.Formatters;
using AspNetCore.Hateoas.Infrastructure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
			builder.Services.TryAdd(ServiceDescriptor
				.Singleton(serviceProvider => new JsonHateoasFormatter(
					serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value.SerializerSettings,
					serviceProvider.GetRequiredService<ArrayPool<char>>()))
			);

			builder.Services.TryAddEnumerable(ServiceDescriptor
				.Transient<IConfigureOptions<MvcOptions>, MvcJsonHateoasMvcOptionsSetup>());
			return builder;
		}
	}
}
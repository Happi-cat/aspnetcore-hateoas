using System;
using System.Buffers;
using AspNetCore.Hateoas.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
	public class MvcJsonHateoasMvcOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly ArrayPool<char> _charPool;

		public MvcJsonHateoasMvcOptionsSetup(IOptions<MvcJsonOptions> jsonOptions, ArrayPool<char> charPool)
		{
			if (jsonOptions == null)
				throw new ArgumentNullException(nameof(jsonOptions));

			_jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
			_charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
		}

		public void Configure(MvcOptions options)
		{
			options.OutputFormatters.Add(new JsonHateoasFormatter(_jsonSerializerSettings, _charPool));
			options.FormatterMappings.SetMediaTypeMappingForFormat(
				"json+hateoas",
				MediaTypeHeaderValue.Parse((StringSegment) "application/json+hateoas")
			);
		}
	}
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using Json.Pointer;

namespace Json.Path;

internal static class ReferenceHandler
{
	internal static void Handle(EvaluationContext context)
	{
		if (!context.Options.ExperimentalFeatures.ProcessDataReferences) return;

		foreach (var match in context.Current.ToList())
		{
			if (!IsReference(match.Value, out var reference)) continue;

			var (success, newData) = ResolveReference(reference, context.Options).GetAwaiter().GetResult();
			if (!success) continue;

			var newMatch = new PathMatch(newData, match.Location);
			var index = context.Current.IndexOf(match);
			context.Current.RemoveAt(index);
			context.Current.Insert(index, newMatch);
		}
	}

	private static bool IsReference(JsonNode? element, [NotNullWhen(true)] out Uri? uri)
	{
		uri = null;
		if (element is not JsonObject obj) return false;

		if (obj.Count != 1 ||
			!obj.TryGetValue("$ref", out var v, out _) ||
			v is not JsonValue value ||
			!value.TryGetValue(out string? reference))
			return false;

		return Uri.TryCreate(reference, UriKind.Absolute, out uri);
	}

	private static async Task<(bool, JsonNode?)> ResolveReference(Uri uri, PathEvaluationOptions options)
	{
		var fragment = uri.Fragment;
		var baseUri = string.IsNullOrWhiteSpace(fragment)
			? uri
			: new Uri(uri.OriginalString.Replace(fragment, string.Empty));

		var (success, document) = await options.ExperimentalFeatures.DataReferenceDownload(baseUri);
		if (!success) return (false, null);
		if (string.IsNullOrWhiteSpace(fragment)) return (true, document);
		if (!JsonPointer.TryParse(fragment, out var pointer)) return (false, null);
		if (pointer!.TryEvaluate(document, out var node)) return (true, node);
		return (false, null);
	}

	internal static async Task<(bool, JsonNode?)> DefaultDownload(Uri uri)
	{
		using var httpClient = new HttpClient();
		try
		{
			return (true, JsonNode.Parse(await httpClient.GetStreamAsync(uri)));
		}
		catch (Exception)
		{
			return (false, null);
		}
	}
}
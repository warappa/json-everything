﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Json.More;

namespace Json.Schema;

/// <summary>
/// Manages which keywords are known by the system.
/// </summary>
/// <remarks>
/// Because the deserialization process relies on keywords being registered,
/// this class cannot be an instance class like the other registries in this
/// library.  Therefore keywords are registered for all schemas.
/// </remarks>
public static class SchemaKeywordRegistry
{
	[DebuggerDisplay("{Name} / {Priority}")]
	private class KeywordMetaData
	{
		public string Name { get; }
		public Type Type { get; }
		// ReSharper disable MemberHidesStaticFromOuterClass
		public long Priority { get; set; }
		public bool ProducesDependentAnnotations { get; set; }
		// ReSharper restore MemberHidesStaticFromOuterClass
		public IJsonSchemaKeyword? NullValue { get; set; }
		public SpecVersion SupportedVersions { get; set; } 
		public JsonSerializerContext? SerializerContext { get; }

		public KeywordMetaData(Type type, JsonSerializerContext? context = null)
		{
			Type = type;
			SerializerContext = context;
			
			var nameAttribute = type.GetCustomAttribute<SchemaKeywordAttribute>() ??
			                    throw new ArgumentException($"Keyword implementation `{type.Name}` does not carry `{nameof(SchemaKeywordAttribute)}`");
			Name = nameAttribute.Name;
			
			var supportedVersionAttributes = type.GetCustomAttributes<SchemaSpecVersionAttribute>();
			SupportedVersions = supportedVersionAttributes.Aggregate(SpecVersion.Unspecified, (total, current) => total | current.Version);
		}
	}

	private static readonly MultiLookupConcurrentDictionary<KeywordMetaData> _keywordData;

	internal static IEnumerable<Type> KeywordTypes => _keywordData.Select(x => x.Value.Type).Distinct();

	static SchemaKeywordRegistry()
	{
		var keywordData = new[]
		{
			typeof(AdditionalItemsKeyword),
			typeof(AdditionalPropertiesKeyword),
			typeof(AllOfKeyword),
			typeof(AnchorKeyword),
			typeof(AnyOfKeyword),
			typeof(CommentKeyword),
			typeof(ConstKeyword),
			typeof(ContainsKeyword),
			typeof(ContentEncodingKeyword),
			typeof(ContentMediaTypeKeyword),
			typeof(ContentSchemaKeyword),
			typeof(DefaultKeyword),
			typeof(DefinitionsKeyword),
			typeof(DefsKeyword),
			typeof(DependenciesKeyword),
			typeof(DependentRequiredKeyword),
			typeof(DependentSchemasKeyword),
			typeof(DeprecatedKeyword),
			typeof(DescriptionKeyword),
			typeof(DynamicAnchorKeyword),
			typeof(DynamicRefKeyword),
			typeof(ElseKeyword),
			typeof(EnumKeyword),
			typeof(ExamplesKeyword),
			typeof(ExclusiveMaximumKeyword),
			typeof(ExclusiveMinimumKeyword),
			typeof(FormatKeyword),
			typeof(IdKeyword),
			typeof(IfKeyword),
			typeof(ItemsKeyword),
			typeof(MaxContainsKeyword),
			typeof(MaximumKeyword),
			typeof(MaxItemsKeyword),
			typeof(MaxLengthKeyword),
			typeof(MaxPropertiesKeyword),
			typeof(MinContainsKeyword),
			typeof(MinimumKeyword),
			typeof(MinItemsKeyword),
			typeof(MinLengthKeyword),
			typeof(MinPropertiesKeyword),
			typeof(MultipleOfKeyword),
			typeof(NotKeyword),
			typeof(OneOfKeyword),
			typeof(PatternKeyword),
			typeof(PatternPropertiesKeyword),
			typeof(PrefixItemsKeyword),
			typeof(PropertiesKeyword),
			typeof(PropertyDependenciesKeyword),
			typeof(PropertyNamesKeyword),
			typeof(ReadOnlyKeyword),
			typeof(RecursiveAnchorKeyword),
			typeof(RecursiveRefKeyword),
			typeof(RefKeyword),
			typeof(RequiredKeyword),
			typeof(SchemaKeyword),
			typeof(ThenKeyword),
			typeof(TitleKeyword),
			typeof(TypeKeyword),
			typeof(UnevaluatedItemsKeyword),
			typeof(UnevaluatedPropertiesKeyword),
			typeof(UniqueItemsKeyword),
			typeof(VocabularyKeyword),
			typeof(WriteOnlyKeyword),
		};

		_keywordData = [];
		_keywordData.AddLookup(x => x.Name);
		_keywordData.AddLookup(x => x.Type);
		foreach (var type in keywordData)
		{
			var metaData = new KeywordMetaData(type, JsonSchemaSerializerContext.Default);
			_keywordData.Add(metaData);
		}

		RegisterNullValue(new ConstKeyword(null));
		RegisterNullValue(new DefaultKeyword(null));

		EvaluateDependencies();
	}

	private static void EvaluateDependencies()
	{
		var toCheck = _keywordData.Select(x => x.Value).Distinct().ToList();

		var keyword = _keywordData[SchemaKeyword.Name];
		keyword.Priority = -2;
		toCheck.Remove(keyword);
		keyword = _keywordData[IdKeyword.Name];
		keyword.Priority = -1;
		toCheck.Remove(keyword);
		keyword = _keywordData[UnevaluatedItemsKeyword.Name];
		keyword.Priority = long.MaxValue;
		toCheck.Remove(keyword);
		keyword = _keywordData[UnevaluatedPropertiesKeyword.Name];
		keyword.Priority = long.MaxValue;
		toCheck.Remove(keyword);

		var priority = 0;
		while (toCheck.Count != 0)
		{
			var unprioritized = toCheck.Select(x => x.Type).ToArray();
			for (var i = 0; i < toCheck.Count; i++)
			{
				keyword = toCheck[i];
				var dependencies = keyword.Type.GetCustomAttributes<DependsOnAnnotationsFromAttribute>()
					.Select(x => x.DependentType);
				foreach (var dependency in dependencies)
				{
					var metaData = _keywordData[dependency];
					metaData.ProducesDependentAnnotations = true;
				}

				var matches = dependencies.Intersect(unprioritized);
				if (matches.Any()) continue;

				keyword.Priority = priority;
				toCheck.Remove(keyword);
				i--;
			}

			priority++;
		}
	}

	/// <summary>
	/// Registers a new keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	[RequiresDynamicCode("For AOT support, use Register<T> that takes a JsonTypeInfo. Using this method requires reflection later.")]
	public static void Register<T>()
		where T : IJsonSchemaKeyword
	{
		_keywordData.Add(new KeywordMetaData(typeof(T)));

		EvaluateDependencies();
	}

	/// <summary>
	/// Registers a new keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	/// <param name="typeContext">JsonTypeInfo for the keyword type</param>
	public static void Register<T>(JsonSerializerContext typeContext)
		where T : IJsonSchemaKeyword
	{
		var typeInfo = typeContext.GetTypeInfo(typeof(T)) ??
					   throw new ArgumentException($"Keyword implementation `{typeof(T).Name}` does not have a JsonTypeInfo");
		_ = typeInfo.Converter as IWeaklyTypedJsonConverter ??
			throw new ArgumentException("Keyword Converter must implement IWeaklyTypedJsonConverter or WeaklyTypedJsonConverter to be AOT compatible");

		_keywordData.Add(new KeywordMetaData(typeof(T), typeContext));

		EvaluateDependencies();
	}

	/// <summary>
	/// Unregisters a keyword type.
	/// </summary>
	/// <typeparam name="T">The keyword type.</typeparam>
	public static void Unregister<T>()
		where T : IJsonSchemaKeyword
	{
		if (_keywordData.TryGetValue(typeof(T), out var metaData))
			_keywordData.Remove(metaData);
	}

	/// <summary>
	/// Gets the implementation for a given keyword name.
	/// </summary>
	/// <param name="keyword">The keyword name.</param>
	/// <returns>The keyword type, if registered; otherwise null.</returns>
	public static Type? GetImplementationType(string keyword)
	{
		return _keywordData.GetValueOrDefault(keyword)?.Type;
	}

	internal static JsonTypeInfo? GetTypeInfo(Type keywordType)
	{
		// A keyword was registered without a JsonTypeInfo; use reflection
		if (_keywordData.TryGetValue(keywordType, out var metaData)) return metaData.SerializerContext?.GetTypeInfo(keywordType);

		// The keyword is unknown
		return JsonSchemaSerializerContext.Default.GetTypeInfo(typeof(UnrecognizedKeyword))!;
	}

	/// <summary>
	/// Registers a null-value for a keyword.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="nullKeyword"></param>
	/// <remarks>
	/// This is important for keywords that accept null, like `default` and `const`.  Without
	/// this step, the serializer will skip keywords that have nulls.
	/// </remarks>
	public static void RegisterNullValue<T>(T nullKeyword)
		where T : IJsonSchemaKeyword
	{
		if (!_keywordData.TryGetValue(typeof(T), out var metaData))
			throw new ArgumentException($"Keyword type `{typeof(T)}` not registered.");

		metaData.NullValue = nullKeyword;
	}

	internal static IJsonSchemaKeyword? GetNullValuedKeyword(Type keywordType)
	{
		return _keywordData.GetValueOrDefault(keywordType)?.NullValue;
	}

	/// <summary>
	/// Gets the keyword string.
	/// </summary>
	/// <param name="keywordType">The keyword type.</param>
	/// <returns>The keyword string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="keywordType"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
	public static string Keyword(this Type keywordType)
	{
		if (keywordType == typeof(UnrecognizedKeyword))
			throw new ArgumentException($"Keyword type `{keywordType}` requires an instance to know the keyword."); // shouldn't happen

		if (!_keywordData.TryGetValue(keywordType, out var metaData))
			throw new ArgumentException($"Keyword type `{keywordType}` not registered.");

		return metaData.Name;
	}

	/// <summary>
	/// Gets the keyword string.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The keyword string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">The keyword does not carry the <see cref="SchemaKeywordAttribute"/>.</exception>
	public static string Keyword(this IJsonSchemaKeyword keyword)
	{
		if (keyword is UnrecognizedKeyword unrecognized) return unrecognized.Name;

		var type = keyword.GetType();
		if (!_keywordData.TryGetValue(type, out var metaData))
			throw new ArgumentException($"Keyword type `{type}` not registered.");

		return metaData.Name;
	}

	/// <summary>
	/// Gets the keyword priority.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The priority.</returns>
	public static long Priority(this IJsonSchemaKeyword keyword)
	{
		if (keyword is UnrecognizedKeyword) return 0;

		var type = keyword.GetType();
		if (!_keywordData.TryGetValue(type, out var metaData))
			throw new ArgumentException($"Keyword type `{type}` not registered.");

		return metaData.Priority;
	}

	/// <summary>
	/// Determines if a keyword is declared by a given version of the JSON Schema specification.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="version">The queried version.</param>
	/// <returns>true if the keyword is supported by the version; false otherwise</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaSpecVersionAttribute"/> declarations.</exception>
	public static bool SupportsVersion(this IJsonSchemaKeyword keyword, SpecVersion version)
	{
		if (keyword is UnrecognizedKeyword) return true;
	
		var type = keyword.GetType();
		if (!_keywordData.TryGetValue(type, out var metaData))
			throw new ArgumentException($"Keyword type `{type}` not registered.");

		return metaData.SupportedVersions.HasFlag(version);
	}

	/// <summary>
	/// Gets the specification versions supported by a keyword.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <returns>The specification versions as a single flags value.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="keyword"/> is null.</exception>
	/// <exception cref="InvalidOperationException">Thrown if the keyword has no <see cref="SchemaSpecVersionAttribute"/> declarations.</exception>
	public static SpecVersion VersionsSupported(this IJsonSchemaKeyword keyword)
	{
		if (keyword is UnrecognizedKeyword) return SpecVersion.All;
	
		var type = keyword.GetType();
		if (!_keywordData.TryGetValue(type, out var metaData))
			throw new ArgumentException($"Keyword type `{type}` not registered.");

		return metaData.SupportedVersions;
	}

	internal static bool ProducesDependentAnnotations(this Type keywordType)
	{
		if (!_keywordData.TryGetValue(keywordType, out var metaData))
			throw new ArgumentException($"Keyword type `{keywordType}` not registered.");

		return metaData.ProducesDependentAnnotations;
	}
}
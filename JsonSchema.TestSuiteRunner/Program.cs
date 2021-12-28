/*
This is @ether's interface from https://metacpan.org/dist/JSON-Schema-Modern/view/script/json-schema-eval.
I want to support as much as possible.  However the goal is to determine the minimal interface required to run the test suite.

    json-schema-eval \
      [ --specification_version|version <version> ] \
      [ --output_format <format> ] \
      [ --short_circuit ] \
      [ --max_traversal_depth <depth> ] \
      [ --validate_formats ] \
      [ --validate_content_schemas ] \
      [ --collect_annotations ] \
      [ --annotate_unknown_keywords ] \
      [ --data <filename> ] \
      [ --schema <filename> ]

    Produces output to console.

    Exit codes:
      -1 - invalid
      0 - valid

Proposed:

    json-schema-eval \
      [ --instance <filename with path to data> ] \
      [ --schema <filename with path to schema> ] \
      [ --spec-version <version> ] \
      [ --validate_formats ]

    Produces output to console.

    Exit codes:
      -1 - invalid
      0 - valid
      1 - error (input, system, etc)
      2 - unsupported

I think this represents the minimum API needed to run the suite for any implementation.

Test runners will also need to know where http://localhost:1234/ is supposed to point so that they can fetch or preload the files in those folders.
*/

using System.Text.Json;
using Json.More;
using Json.Pointer;

namespace Json.Schema.TestSuiteRunner; // Note: actual namespace depends on the project name.

internal class Program
{
	/// <param name="schema">An option whose argument is parsed as an int</param>
	/// <param name="instance">An option whose argument is parsed as a bool</param>
	/// <param name="specVersion">An option whose argument is parsed as a FileInfo</param>
	public static void Main(Uri schema, Uri instance, string? specVersion = null)
    {
        Draft? draft = null;
        if (specVersion != null)
        {
            draft = specVersion switch
            {
                "6" => Draft.Draft6,
                "06" => Draft.Draft6,
                "draft-6" => Draft.Draft6,
                "draft-06" => Draft.Draft6,
                "7" => Draft.Draft7,
                "07" => Draft.Draft7,
                "draft-7" => Draft.Draft7,
                "draft-07" => Draft.Draft7,
                "2019-09" => Draft.Draft201909,
                "draft-2019-09" => Draft.Draft201909,
                "2020-12" => Draft.Draft202012,
                "draft-2020-12" => Draft.Draft202012,
                _ => null
            };

            if (draft == null)
            {
                Console.WriteLine($"`{specVersion}` is not supported.");
                Environment.ExitCode = 2;
                return;
            }
        }

        string schemaText;
        try
        {
            schemaText = GetFileContext(schema);
        }
        catch (Exception e)
        {
            Console.WriteLine("Cannot determine schema from URI provided.");
            Console.Error.WriteLine(e);
            Environment.ExitCode = 1;
            return;
        }

        string instanceText;
        try
        {
            instanceText = GetFileContext(instance);
        }
        catch (Exception e)
        {
            Console.WriteLine("Cannot determine instance from URI provided.");
            Console.Error.WriteLine(e);
            Environment.ExitCode = 1;
            return;
        }

        try
        {
            Environment.ExitCode = Run(schemaText, instanceText, draft);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred trying to run the test.");
            Console.Error.WriteLine(e);
            Environment.ExitCode = 1;
        }
	}

    private static (string, string) GetPathAndPointer(Uri fileAndPointer)
    {
        var parts = fileAndPointer.OriginalString.Split('#');

        if (parts.Length != 2)
            throw new ArgumentException($"Cannot process URI `{fileAndPointer}`");

		return (parts[0], parts[1]);
    }

	private static string GetFileContext(Uri fileAndPointer)
    {
        var (localPath, contentPointer) = GetPathAndPointer(fileAndPointer);
        var fileText = File.ReadAllText(localPath);
        var pointer = JsonPointer.Parse(contentPointer);
        var jsonContent = JsonDocument.Parse(fileText);
        return pointer.Evaluate(jsonContent.RootElement)!.Value.ToJsonString();
    }

    private static int Run(string schemaText, string instanceText, Draft? draft)
    {
        var schema = JsonSerializer.Deserialize<JsonSchema>(schemaText);
        var instance = JsonDocument.Parse(instanceText).RootElement;


        var options = new ValidationOptions();
		if (draft.HasValue)
            options.ValidateAs = draft.Value;

        var result = schema.Validate(instance, options);
        return result.IsValid ? 0 : -1;
    }
}
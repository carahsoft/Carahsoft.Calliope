using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Carahsoft.Calliope.Components;

namespace Carahsoft.Calliope.Forms.Json
{
    /// <summary>
    /// Builds a <see cref="Form"/> from an object's JSON Schema, as produced by
    /// <see cref="System.Text.Json.Schema.JsonSchemaExporter"/>, and turns its
    /// values into a JSON object.
    /// </summary>
    /// <remarks>
    /// Each property becomes a field: <c>enum</c>s are choices, <c>integer</c>
    /// (including <see cref="long"/>, exported as <c>["string","integer"]</c>),
    /// <c>number</c> and <c>boolean</c> types get their own fields, strings with
    /// <c>format: date</c> are dates, other strings are text, and anything else
    /// (objects, arrays…) is typed as raw JSON. A <c>"null"</c> type just makes
    /// the property optional. <c>default</c>, <c>description</c> and
    /// <c>required</c> are used too.
    /// </remarks>
    public static class JsonSchemaForm
    {
        /// <summary>
        /// A form with a field for each of the schema's properties. A schema
        /// with no properties gives an empty form.
        /// </summary>
        public static FormState Create(JsonNode? schema)
        {
            var required = schema?["required"] is JsonArray names
                ? names.Select(n => n?.GetValue<string>()).OfType<string>().ToHashSet()
                : [];

            var fields = new List<FormField>();
            if (schema?["properties"] is JsonObject properties)
            {
                foreach (var (name, property) in properties)
                {
                    if (property != null)
                        fields.Add(FieldOf(name, property, required.Contains(name)));
                }
            }

            return Form.Create(fields);
        }

        /// <summary>
        /// Parses the schema from JSON text and builds a form from it
        /// </summary>
        public static FormState Create(string schemaJson) => Create(JsonNode.Parse(schemaJson));

        /// <summary>
        /// The submitted values as a JSON object: dates as yyyy-MM-dd, choices
        /// as their text, and raw JSON as typed. Use it once
        /// <see cref="FormState.Status"/> is <see cref="FormStatus.Submitted"/>.
        /// </summary>
        public static JsonObject ToJson(FormState state)
        {
            if (state.Values is not { } values)
                throw new InvalidOperationException("The form hasn't been submitted");

            var json = new JsonObject();
            foreach (var field in state.Fields)
            {
                if (values.TryGetValue(field.Name, out var value))
                    json[field.Name] = ToNode(value);
            }
            return json;
        }

        /// <summary>
        /// Validates and submits the form. Returns the submitted form and the
        /// values as JSON text, or the form with errors set and null.
        /// </summary>
        public static (FormState State, string? Json) Submit(FormState state)
        {
            state = Form.Submit(state);
            return (state, state.Status == FormStatus.Submitted ? ToJson(state).ToJsonString() : null);
        }

        private static JsonNode? ToNode(object? value) => value switch
        {
            null => null,
            string s => JsonValue.Create(s),
            long n => JsonValue.Create(n),
            decimal d => JsonValue.Create(d),
            bool b => JsonValue.Create(b),
            DateOnly date => JsonValue.Create(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            JsonNode node => node.DeepClone(),
            _ => JsonSerializer.SerializeToNode(value),
        };

        /// <summary>
        /// The "type" keyword can be a string or an array like ["string","null"]
        /// </summary>
        private static List<string> Types(JsonNode schema) => schema["type"] switch
        {
            JsonArray a => a.Select(t => t?.GetValue<string>()).OfType<string>().ToList(),
            JsonValue v when v.GetValueKind() == JsonValueKind.String => [v.GetValue<string>()],
            _ => [],
        };

        private static string? StringOf(JsonNode? node) =>
            node is JsonValue v && v.GetValueKind() == JsonValueKind.String ? v.GetValue<string>() : null;

        private static FormField FieldOf(string name, JsonNode schema, bool required)
        {
            var defaultValue = schema["default"] switch
            {
                null => "",
                JsonValue v when v.GetValueKind() == JsonValueKind.String => v.GetValue<string>(),
                var d => d.ToJsonString(),
            };

            FormField field;
            if (schema["enum"] is JsonArray values)
            {
                var options = values.Where(v => v != null).Select(v => v!.ToString()).ToList();
                field = FormField.Choice(name, options, defaultValue);
            }
            else
            {
                var types = Types(schema).Where(t => t != "null").ToList();
                var format = StringOf(schema["format"]);

                // Check integer before string: Int64 is exported as ["string","integer"]
                var kind = types.Contains("integer") ? FormFieldKind.Integer
                    : types.Contains("number") ? FormFieldKind.Number
                    : types.Contains("boolean") ? FormFieldKind.Boolean
                    : types is ["string"] && format == "date" ? FormFieldKind.Date
                    : types is ["string"] ? FormFieldKind.Text
                    : FormFieldKind.RawJson;

                field = kind == FormFieldKind.Boolean
                    ? FormField.Boolean(name, defaultValue == "true")
                    : new FormField(name, kind) { Input = TextFieldState.Of(defaultValue) };
            }

            return field with { Description = StringOf(schema["description"]), Required = required };
        }
    }
}

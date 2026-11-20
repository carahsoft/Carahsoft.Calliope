using System.Text.Json.Schema;
using System.Text.Json;
using Carahsoft.Calliope.Components;
using Carahsoft.Calliope.Forms.Json;
using static Carahsoft.Calliope.Tests.Keys;

namespace Carahsoft.Calliope.Tests;

// The schemas are the ones kmc's FormTests use, as exported from its request types
public class JsonSchemaFormTests
{
    private const string MonitorNameSchema = """{"type":["object","null"],"properties":{"monitorName":{"type":"string"}},"required":["monitorName"]}""";

    private static FormState Type(string text, FormState form) => text.Aggregate(form, (f, c) => Form.Update(f, Char(c)));

    private static string? Submit(FormState form) => JsonSchemaForm.Submit(form).Json;

    [Fact]
    public void Schema_with_no_properties_gives_an_empty_form()
    {
        Assert.True(Form.IsEmpty(JsonSchemaForm.Create("""{"type":["object","null"]}""")));
        Assert.Equal("{}", Submit(JsonSchemaForm.Create("""{"type":["object","null"]}""")));
    }

    [Fact]
    public void Required_string()
    {
        var form = JsonSchemaForm.Create(MonitorNameSchema);
        var field = Assert.Single(form.Fields);
        Assert.Equal("monitorName", field.Name);
        Assert.True(field.Required);
        Assert.Equal(FormFieldKind.Text, field.Kind);

        var (withErrors, json) = JsonSchemaForm.Submit(form);
        Assert.Null(json);
        Assert.Equal("Required", withErrors.Fields[0].Error);

        Assert.Equal("""{"monitorName":"TeamsCdrProcessor"}""", Submit(Type("TeamsCdrProcessor", form)));
    }

    [Fact]
    public void Boolean_with_a_default()
    {
        var form = JsonSchemaForm.Create("""{"type":["object","null"],"properties":{"all":{"type":"boolean","default":false}}}""");
        Assert.Equal(FormFieldKind.Boolean, form.Fields[0].Kind);
        Assert.False(form.Fields[0].Toggle);

        Assert.Equal("""{"all":true}""", Submit(Form.Update(form, Space)));
    }

    [Fact]
    public void Date_and_optional_text()
    {
        var form = JsonSchemaForm.Create(
            """{"type":["object","null"],"properties":{"date":{"type":"string","format":"date"},"term":{"type":["string","null"],"default":null}},"required":["date"]}""");

        Assert.Equal(FormFieldKind.Date, form.Fields[0].Kind);
        Assert.Equal(FormFieldKind.Text, form.Fields[1].Kind);
        Assert.False(form.Fields[1].Required);

        Assert.Equal("Must be a date like 2026-10-13", JsonSchemaForm.Submit(Type("not a date", form)).State.Fields[0].Error);
        Assert.Equal("""{"date":"2026-10-13"}""", Submit(Type("2026-10-13", form)));
    }

    [Fact]
    public void Int64_is_exported_as_string_or_integer()
    {
        var form = JsonSchemaForm.Create("""{"type":["object","null"],"properties":{"count":{"type":["string","integer"],"pattern":"^-?(?:0|[1-9]\\d*)$"}},"required":["count"]}""");
        Assert.Equal(FormFieldKind.Integer, form.Fields[0].Kind);
        Assert.Equal("""{"count":42}""", Submit(Type("42", form)));
    }

    [Fact]
    public void Enum_choices()
    {
        var form = JsonSchemaForm.Create("""{"type":"object","properties":{"kind":{"enum":["Query","Mutation"],"default":"Mutation"}}}""");
        Assert.Equal(FormFieldKind.Choice, form.Fields[0].Kind);
        Assert.Equal(["Query", "Mutation"], form.Fields[0].Choices.Options);
        Assert.Equal("""{"kind":"Mutation"}""", Submit(form));
        Assert.Equal("""{"kind":"Query"}""", Submit(Form.Update(form, Right)));
    }

    [Fact]
    public void Objects_fall_back_to_raw_JSON()
    {
        var form = JsonSchemaForm.Create("""{"type":"object","properties":{"record":{"type":["object","null"],"properties":{}}}}""");
        Assert.Equal(FormFieldKind.RawJson, form.Fields[0].Kind);
        Assert.Equal("""{"record":{"a":1}}""", Submit(Type("""{"a":1}""", form)));
        Assert.StartsWith("Invalid JSON", JsonSchemaForm.Submit(Type("{", form)).State.Fields[0].Error);
    }

    [Fact]
    public void Explicit_JSON_null_is_kept()
    {
        var form = JsonSchemaForm.Create("""{"type":"object","properties":{"record":{"type":["object","null"],"properties":{}}},"required":["record"]}""");
        Assert.Equal("""{"record":null}""", Submit(Type("null", form)));
    }

    [Fact]
    public void Descriptions_are_shown()
    {
        var form = JsonSchemaForm.Create("""{"type":"object","properties":{"name":{"type":"string","description":"Who to greet"}}}""");
        Assert.Equal("Who to greet", form.Fields[0].Description);
    }

    [Fact]
    public void Submitting_twice_gives_the_same_JSON()
    {
        var form = Form.Submit(Type("[1]", JsonSchemaForm.Create("""{"type":"object","properties":{"a":{"type":"array"}}}""")));
        Assert.Equal(JsonSchemaForm.ToJson(form).ToJsonString(), JsonSchemaForm.ToJson(form).ToJsonString());
    }

    private record Request(string Name, int Count, DateOnly Day, bool All, Kind Kind, long? Limit = null);
    private enum Kind { Query, Mutation }

    [Fact]
    public void Works_with_JsonSchemaExporter()
    {
        var options = new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
        };
        var schema = options.GetJsonSchemaAsNode(typeof(Request));

        var form = JsonSchemaForm.Create(schema);
        Assert.Equal(
            [FormFieldKind.Text, FormFieldKind.Integer, FormFieldKind.Date, FormFieldKind.Boolean, FormFieldKind.Choice, FormFieldKind.Integer],
            form.Fields.Select(f => f.Kind));

        form = Type("bob", form);
        form = Type("3", Form.Update(form, Tab));
        form = Type("2026-10-13", Form.Update(form, Tab));
        var json = Submit(form)!;

        var request = JsonSerializer.Deserialize<Request>(json, options)!;
        Assert.Equal(new Request("bob", 3, new DateOnly(2026, 10, 13), false, Kind.Query), request);
    }
}

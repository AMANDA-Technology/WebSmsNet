using System.Text.Json;
using System.Text.Json.Serialization;
using Shouldly;
using WebSmsNet.Abstractions.Models.Enums;
using WebSmsNet.Abstractions.Serialization;

namespace WebSmsNet.UnitTests.Serialization;

[TestFixture]
public class WebSmsJsonSerializationTests
{
    [Test]
    public void DefaultOptions_IsNotNull()
    {
        WebSmsJsonSerialization.DefaultOptions.ShouldNotBeNull();
    }

    [Test]
    public void DefaultOptions_ReturnsNewInstanceEachTime()
    {
        var first = WebSmsJsonSerialization.DefaultOptions;
        var second = WebSmsJsonSerialization.DefaultOptions;

        ReferenceEquals(first, second).ShouldBeFalse();
    }

    [Test]
    public void DefaultOptions_WriteIndented_IsFalse()
    {
        WebSmsJsonSerialization.DefaultOptions.WriteIndented.ShouldBeFalse();
    }

    [Test]
    public void DefaultOptions_DefaultIgnoreCondition_IsWhenWritingNull()
    {
        WebSmsJsonSerialization.DefaultOptions.DefaultIgnoreCondition
            .ShouldBe(JsonIgnoreCondition.WhenWritingNull);
    }

    [Test]
    public void DefaultOptions_PropertyNameCaseInsensitive_IsTrue()
    {
        // Web defaults enable case-insensitive property name matching
        WebSmsJsonSerialization.DefaultOptions.PropertyNameCaseInsensitive.ShouldBeTrue();
    }

    [Test]
    public void DefaultOptions_ContainsWebSmsWebhookRequestConverter()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;

        var hasConverter = options.Converters.Any(c => c is WebSmsWebhookRequestConverter);

        hasConverter.ShouldBeTrue();
    }

    [Test]
    public void DefaultOptions_ContainsJsonStringEnumConverter()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;

        var hasConverter = options.Converters.Any(c => c is JsonStringEnumConverter);

        hasConverter.ShouldBeTrue();
    }

    [Test]
    public void DefaultOptions_SerializesEnumAsCamelCaseString()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;

        var json = JsonSerializer.Serialize(AddressType.International, options);

        json.ShouldBe("\"international\"");
    }

    [Test]
    public void DefaultOptions_DeserializesEnumFromCamelCaseString()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;

        var value = JsonSerializer.Deserialize<AddressType>("\"international\"", options);

        value.ShouldBe(AddressType.International);
    }

    [Test]
    public void DefaultOptions_OmitsNullProperties()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;
        var obj = new NullableTestModel { Name = "test", Description = null };

        var json = JsonSerializer.Serialize(obj, options);

        json.ShouldNotContain("description");
        json.ShouldContain("\"name\":\"test\"");
    }

    [Test]
    public void DefaultOptions_SerializesWebhookMessageType_DeliveryReport_AsCamelCase()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;

        var json = JsonSerializer.Serialize(WebhookMessageType.DeliveryReport, options);

        json.ShouldBe("\"deliveryReport\"");
    }

    [Test]
    public void DefaultOptions_DeserializesWebhookMessageType_DeliveryReport_FromCamelCase()
    {
        var options = WebSmsJsonSerialization.DefaultOptions;

        var value = JsonSerializer.Deserialize<WebhookMessageType>("\"deliveryReport\"", options);

        value.ShouldBe(WebhookMessageType.DeliveryReport);
    }

    private sealed record NullableTestModel
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
    }
}

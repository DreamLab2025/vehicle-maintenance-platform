namespace Verendar.Location.Tests.Serialization;

using System.Text.Json;
using Verendar.Common.Shared;
using Verendar.Location.Application.Dtos;

/// <summary>Contract check: JSON shape clients see (camelCase) matches <see cref="ApiResponse{T}"/> + DTOs.</summary>
public class LocationApiResponseJsonTests
{
    private static readonly JsonSerializerOptions CamelCaseJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void ProvinceList_success_serializes_expected_envelope_and_fields()
    {
        var data = new List<ProvinceResponse>
        {
            new()
            {
                Code = "91",
                Name = "An Giang",
                AdministrativeRegionId = 8,
                AdministrativeRegionName = "Đồng bằng sông Cửu Long",
                AdministrativeUnitId = 2,
                AdministrativeUnitName = "Tỉnh"
            }
        };
        var api = ApiResponse<List<ProvinceResponse>>.SuccessResponse(data, "Lấy danh sách tỉnh thành công");

        var json = JsonSerializer.Serialize(api, CamelCaseJson);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        root.GetProperty("statusCode").GetInt32().Should().Be(200);
        root.GetProperty("message").GetString().Should().Be("Lấy danh sách tỉnh thành công");
        (!root.TryGetProperty("metadata", out var meta) || meta.ValueKind == JsonValueKind.Null).Should().BeTrue(
            "metadata should be omitted or null for typical success responses");

        var first = root.GetProperty("data")[0];
        first.GetProperty("code").GetString().Should().Be("91");
        first.GetProperty("name").GetString().Should().Be("An Giang");
        first.GetProperty("administrativeRegionId").GetInt32().Should().Be(8);
        first.GetProperty("administrativeRegionName").GetString().Should().Be("Đồng bằng sông Cửu Long");
        first.GetProperty("administrativeUnitId").GetInt32().Should().Be(2);
        first.GetProperty("administrativeUnitName").GetString().Should().Be("Tỉnh");
    }

    [Fact]
    public void Province_not_found_serializes_expected_envelope()
    {
        var api = ApiResponse<ProvinceResponse>.NotFoundResponse("Tỉnh không tồn tại");
        var json = JsonSerializer.Serialize(api, CamelCaseJson);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("isSuccess").GetBoolean().Should().BeFalse();
        root.GetProperty("statusCode").GetInt32().Should().Be(404);
        root.GetProperty("message").GetString().Should().Be("Tỉnh không tồn tại");
        root.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}

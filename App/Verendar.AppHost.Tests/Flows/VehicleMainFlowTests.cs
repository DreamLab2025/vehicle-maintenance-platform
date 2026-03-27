using System.Net;
using System.Text.Json;
using FluentAssertions;
using Verendar.AppHost.Tests.Fixtures;
using Verendar.AppHost.Tests.Support;
using Xunit;

namespace Verendar.AppHost.Tests.Flows;

[Collection(AppHostCollection.Name)]
public class VehicleMainFlowTests(AppHostFixture fixture)
{
    private static readonly Guid SeedVariantId = Guid.Parse("e0000001-0000-0000-0000-000000000001");

    [Fact]
    public async Task CreateVehicle_WhenValidRequest_ShouldReturnCreatedAndNeedsOnboarding()
    {
        var tokens = await GatewayAuthHelper.LoginAndGetTokensAsync(fixture.ApiGatewayClient);
        var variantId = SeedVariantId;

        var createVehicle = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Post,
            "/api/v1/user-vehicles",
            tokens.AccessToken,
            new
            {
                vehicleVariantId = variantId,
                licensePlate = $"IT-{Guid.NewGuid():N}".Substring(0, 12),
                vin = Guid.NewGuid().ToString("N")[..17],
                purchaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
                currentOdometer = 1200
            });

        createVehicle.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdPayload = await createVehicle.Content.ReadAsStringAsync();
        var userVehicleId = ReadDataGuidProperty(createdPayload, "id");
        userVehicleId.Should().NotBeEmpty();

        var partsResponse = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Get,
            $"/api/v1/user-vehicles/{userVehicleId}/parts",
            tokens.AccessToken);

        partsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var partsPayload = await partsResponse.Content.ReadAsStringAsync();
        ReadAnyArrayItemBooleanProperty(partsPayload, "isDeclared").Should().BeFalse();
    }

    [Fact]
    public async Task ApplyTracking_WhenOnboardingPartSubmitted_ShouldCreatePartTrackingAndActiveCycle()
    {
        var tokens = await GatewayAuthHelper.LoginAndGetTokensAsync(fixture.ApiGatewayClient);
        var variantId = SeedVariantId;

        var createVehicle = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Post,
            "/api/v1/user-vehicles",
            tokens.AccessToken,
            new
            {
                vehicleVariantId = variantId,
                licensePlate = $"IT-{Guid.NewGuid():N}".Substring(0, 12),
                vin = Guid.NewGuid().ToString("N")[..17],
                purchaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
                currentOdometer = 2000
            });

        createVehicle.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPayload = await createVehicle.Content.ReadAsStringAsync();
        var userVehicleId = ReadDataGuidProperty(createdPayload, "id");

        var partsResponse = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Get,
            $"/api/v1/user-vehicles/{userVehicleId}/parts",
            tokens.AccessToken);
        partsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var partsPayload = await partsResponse.Content.ReadAsStringAsync();
        var partCategorySlug = ReadFirstArrayItemStringProperty(partsPayload, "partCategorySlug");
        partCategorySlug.Should().NotBeNullOrWhiteSpace();

        var applyTracking = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Post,
            $"/api/v1/user-vehicles/{userVehicleId}/apply-tracking",
            tokens.AccessToken,
            new
            {
                partCategorySlug = partCategorySlug!,
                lastReplacementOdometer = 1800,
                predictedNextOdometer = 4500,
                confidenceScore = 0.95
            });

        applyTracking.StatusCode.Should().Be(HttpStatusCode.OK);
        var applyPayload = await applyTracking.Content.ReadAsStringAsync();
        ReadNestedDataStringProperty(applyPayload, "activeCycle", "status").Should().Be("Active");
    }

    [Fact]
    public async Task ReminderLevel_WhenFreshCycle_ShouldStartAtNormalOrLow()
    {
        var (token, vehicleId) = await CreateVehicleWithAppliedTrackingAsync();

        var remindersPayload = await GetRemindersPayloadAsync(token, vehicleId);
        var levels = ReadReminderLevels(remindersPayload);
        levels.Should().NotBeEmpty();
        levels.Should().Contain(level => level == "Normal" || level == "Low");
    }

    [Fact]
    public async Task ReminderLevel_WhenOdometerIncreases_ShouldEscalateToMedium()
    {
        var (token, vehicleId) = await CreateVehicleWithAppliedTrackingAsync();

        await PatchOdometerAsync(token, vehicleId, 3900);

        var remindersPayload = await GetRemindersPayloadAsync(token, vehicleId);
        ReadReminderLevels(remindersPayload).Should().Contain("Medium");
    }

    [Fact]
    public async Task ReminderLevel_WhenNearDue_ShouldEscalateToHighOrCritical()
    {
        var (token, vehicleId) = await CreateVehicleWithAppliedTrackingAsync();

        await PatchOdometerAsync(token, vehicleId, 4200);
        ReadReminderLevels(await GetRemindersPayloadAsync(token, vehicleId)).Should().Contain("High");

        await PatchOdometerAsync(token, vehicleId, 4400);
        ReadReminderLevels(await GetRemindersPayloadAsync(token, vehicleId)).Should().Contain("Critical");
    }

    [Fact]
    public async Task ReminderEvent_WhenCritical_ShouldBePublishedForNotification()
    {
        var (token, vehicleId) = await CreateVehicleWithAppliedTrackingAsync();

        await PatchOdometerAsync(token, vehicleId, 3900);
        await PatchOdometerAsync(token, vehicleId, 4200);
        await PatchOdometerAsync(token, vehicleId, 4400);

        var hasNotification = await PollUntilAsync(
            async () =>
            {
                using var response = await GatewayAuthHelper.SendAuthorizedAsync(
                    fixture.ApiGatewayClient,
                    HttpMethod.Get,
                    "/api/v1/notifications?pageNumber=1&pageSize=20",
                    token);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                return ReadDataArrayCount(json) > 0;
            },
            maxAttempts: 40,
            delayMs: 500);

        hasNotification.Should().BeTrue("Critical reminder should produce at least one in-app notification via MassTransit");
    }

    private async Task<(string Token, Guid VehicleId)> CreateVehicleWithAppliedTrackingAsync()
    {
        var tokens = await GatewayAuthHelper.LoginAndGetTokensAsync(fixture.ApiGatewayClient);
        var token = tokens.AccessToken;

        var createVehicle = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Post,
            "/api/v1/user-vehicles",
            token,
            new
            {
                vehicleVariantId = SeedVariantId,
                licensePlate = $"IT-{Guid.NewGuid():N}".Substring(0, 12),
                vin = Guid.NewGuid().ToString("N")[..17],
                purchaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
                currentOdometer = 2000
            });

        createVehicle.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPayload = await createVehicle.Content.ReadAsStringAsync();
        var userVehicleId = ReadDataGuidProperty(createdPayload, "id");
        userVehicleId.Should().NotBeEmpty();

        var partsResponse = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Get,
            $"/api/v1/user-vehicles/{userVehicleId}/parts",
            token);
        partsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var partsPayload = await partsResponse.Content.ReadAsStringAsync();
        var partCategorySlug = ReadFirstArrayItemStringProperty(partsPayload, "partCategorySlug");
        partCategorySlug.Should().NotBeNullOrWhiteSpace();

        var applyTracking = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Post,
            $"/api/v1/user-vehicles/{userVehicleId}/apply-tracking",
            token,
            new
            {
                partCategorySlug = partCategorySlug!,
                lastReplacementOdometer = 1800,
                predictedNextOdometer = 4500,
                confidenceScore = 0.95
            });

        applyTracking.StatusCode.Should().Be(HttpStatusCode.OK);

        return (token, userVehicleId);
    }

    private async Task PatchOdometerAsync(string token, Guid vehicleId, int currentOdometer)
    {
        using var response = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Patch,
            $"/api/v1/odometer-history/{vehicleId}",
            token,
            new { currentOdometer });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> GetRemindersPayloadAsync(string token, Guid vehicleId)
    {
        using var response = await GatewayAuthHelper.SendAuthorizedAsync(
            fixture.ApiGatewayClient,
            HttpMethod.Get,
            $"/api/v1/user-vehicles/{vehicleId}/reminders",
            token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync();
    }

    private static List<string> ReadReminderLevels(string apiResponseJson)
    {
        using var document = JsonDocument.Parse(apiResponseJson);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var list = new List<string>();
        foreach (var item in data.EnumerateArray())
        {
            if (item.TryGetProperty("level", out var level) && level.ValueKind == JsonValueKind.String)
            {
                var s = level.GetString();
                if (!string.IsNullOrEmpty(s))
                {
                    list.Add(s);
                }
            }
        }

        return list;
    }

    private static int ReadDataArrayCount(string apiResponseJson)
    {
        using var document = JsonDocument.Parse(apiResponseJson);
        if (!document.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return data.GetArrayLength();
    }

    private static async Task<bool> PollUntilAsync(Func<Task<bool>> check, int maxAttempts, int delayMs)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            if (await check())
            {
                return true;
            }

            await Task.Delay(delayMs);
        }

        return false;
    }

    private static Guid ReadDataGuidProperty(string apiResponseJson, string propertyName)
    {
        using var document = JsonDocument.Parse(apiResponseJson);
        if (!document.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Object ||
            !data.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(property.GetString(), out var id) ? id : Guid.Empty;
    }

    private static string? ReadFirstArrayItemStringProperty(string apiResponseJson, string propertyName)
    {
        using var document = JsonDocument.Parse(apiResponseJson);
        if (!document.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array ||
            data.GetArrayLength() == 0)
        {
            return null;
        }

        return data[0].TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static bool ReadAnyArrayItemBooleanProperty(string apiResponseJson, string propertyName)
    {
        using var document = JsonDocument.Parse(apiResponseJson);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in data.EnumerateArray())
        {
            if (item.TryGetProperty(propertyName, out var property) &&
                (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False))
            {
                return property.GetBoolean();
            }
        }

        return false;
    }

    private static string? ReadNestedDataStringProperty(string apiResponseJson, string objectPropertyName, string nestedPropertyName)
    {
        using var document = JsonDocument.Parse(apiResponseJson);
        if (!document.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Object ||
            !data.TryGetProperty(objectPropertyName, out var nestedObject) ||
            nestedObject.ValueKind != JsonValueKind.Object ||
            !nestedObject.TryGetProperty(nestedPropertyName, out var nestedProperty) ||
            nestedProperty.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return nestedProperty.GetString();
    }
}

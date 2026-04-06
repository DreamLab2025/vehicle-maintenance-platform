using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Verendar.Garage.Infrastructure.Configuration;
using Verendar.Garage.Infrastructure.ExternalServices;

namespace Verendar.Garage.Tests.ExternalServices;

public class VietQRBusinessLookupServiceTests
{
    [Fact]
    public async Task LookupBusinessAsync_WhenVietQrReturnsSuccess_MapsBusinessInfoDto()
    {
        var handler = new TestHttpMessageHandler
        {
            SendImpl = req =>
            {
                req.RequestUri!.AbsolutePath.Should().Be("/v2/business/0123456789");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {"code":"00","desc":"OK","data":{"id":"0123456789","name":"Công ty ABC","internationalName":"ABC Co","shortName":"ABC","address":"HN","status":"Active"}}
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("https://api.vietqr.io") });

        var options = Options.Create(new VietQRSettings { BaseUrl = "https://api.vietqr.io" });
        var sut = new VietQRBusinessLookupService(
            options,
            factory.Object,
            NullLogger<VietQRBusinessLookupService>.Instance);

        var result = await sut.LookupBusinessAsync("0123456789");

        result.Should().NotBeNull();
        result!.TaxCode.Should().Be("0123456789");
        result.Name.Should().Be("Công ty ABC");
        result.InternationalName.Should().Be("ABC Co");
        result.ShortName.Should().Be("ABC");
        result.Address.Should().Be("HN");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task LookupBusinessAsync_WhenHttpNotFound_ReturnsNull()
    {
        var handler = new TestHttpMessageHandler
        {
            SendImpl = _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        };

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("https://api.vietqr.io") });

        var options = Options.Create(new VietQRSettings { BaseUrl = "https://api.vietqr.io" });
        var sut = new VietQRBusinessLookupService(
            options,
            factory.Object,
            NullLogger<VietQRBusinessLookupService>.Instance);

        var result = await sut.LookupBusinessAsync("0000000000");

        result.Should().BeNull();
    }

    [Fact]
    public async Task LookupBusinessAsync_WhenCodeIsNot00_ReturnsNull()
    {
        var handler = new TestHttpMessageHandler
        {
            SendImpl = _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"code":"01","desc":"Not found","data":null}""",
                    Encoding.UTF8,
                    "application/json")
            }
        };

        var factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("https://api.vietqr.io") });

        var options = Options.Create(new VietQRSettings { BaseUrl = "https://api.vietqr.io" });
        var sut = new VietQRBusinessLookupService(
            options,
            factory.Object,
            NullLogger<VietQRBusinessLookupService>.Instance);

        var result = await sut.LookupBusinessAsync("0123456789");

        result.Should().BeNull();
    }
}

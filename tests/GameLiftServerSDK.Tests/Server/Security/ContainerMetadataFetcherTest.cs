using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Aws.GameLift.Server.Security;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server.Security
{
    [TestFixture]
    public class ContainerMetadataFetcherTest
    {
        private const string EnvironmentVariableContainerMetadataUri = "ECS_CONTAINER_METADATA_URI_V4";
        private const string ContainerTaskMetadataResponse = @"
        {
            ""Cluster"": ""default"",
            ""TaskARN"": ""arn:aws:ecs:us-west-2:211125306013:task/HelloWorldCluster/5c1a9b3178434e158ed1f2c16be69d14"",
        }";

        private Mock<HttpMessageHandler> httpMessageHandlerMock;
        private HttpClient httpClient;
        private ContainerMetadataFetcher metadataFetcher;


        [SetUp]
        public void SetUp()
        {
            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpClient = new HttpClient(httpMessageHandlerMock.Object);
            metadataFetcher = new ContainerMetadataFetcher(httpClient);
        }

        [Test]
        public void GIVEN_nullHttpClient_WHEN_constructor_THEN_throwArgumentNullException()
        {
            // GIVEN
            HttpClient nullHttpClient = null;

            // WHEN / THEN
            Assert.Throws<ArgumentNullException>(() => new ContainerMetadataFetcher(nullHttpClient));
        }

        [Test]
        public void
            GIVEN_containerTaskMetadataResponse_WHEN_fetchContainerMetadata_THEN_returnContainerTaskMetadata()
        {
            // GIVEN
            Environment.SetEnvironmentVariable(EnvironmentVariableContainerMetadataUri, "http://169.254.170.2/v4");
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ContainerTaskMetadataResponse)
                });

            // WHEN
            var result = metadataFetcher.FetchContainerTaskMetadata();

            // THEN
            Assert.AreEqual("5c1a9b3178434e158ed1f2c16be69d14", result.TaskId);
        }


        [Test]
        public void
            GIVEN_missingEnvironmentVariable_WHEN_fetchContainerTaskMetadata_THEN_throwInvalidOperationException()
        {
            // GIVEN
            Environment.SetEnvironmentVariable(EnvironmentVariableContainerMetadataUri, null);

            // WHEN / THEN
            var ex = Assert.Throws<InvalidOperationException>(() => metadataFetcher.FetchContainerTaskMetadata());
            Assert.AreEqual("The environment variable ECS_CONTAINER_METADATA_URI_V4 is not set.", ex.Message);
        }

        [Test]
        public void GIVEN_httpRequestFails_WHEN_fetchContainerTaskMetadata_THEN_throwHttpRequestException()
        {
            // GIVEN
            Environment.SetEnvironmentVariable(EnvironmentVariableContainerMetadataUri, "http://169.254.170.2/v4");
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // WHEN / THEN
            Assert.Throws<HttpRequestException>(() => metadataFetcher.FetchContainerTaskMetadata());
        }

        [Test]
        public void
            GIVEN_missingTaskArnInResponse_WHEN_fetchContainerTaskMetadata_THEN_throwInvalidOperationException()
        {
            // GIVEN
            Environment.SetEnvironmentVariable(EnvironmentVariableContainerMetadataUri, "http://169.254.170.2/v4");
            const string jsonResponseWithoutTaskArn = "{}";

            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponseWithoutTaskArn)
                });

            // WHEN / THEN
            var ex = Assert.Throws<InvalidOperationException>(() => metadataFetcher.FetchContainerTaskMetadata());
            Assert.AreEqual("TaskArn is not available in container task metadata", ex.Message);
        }
    }
}
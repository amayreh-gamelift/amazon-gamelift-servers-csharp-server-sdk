/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

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
    public class ContainerCredentialsFetcherTest
    {
        private const string ContainerCredentialsProviderResponse = @"
        {
            ""AccessKeyId"": ""AKIAIOSFODNN7EXAMPLE"",
            ""SecretAccessKey"": ""wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"",
            ""Token"": ""AQoDYXdzEJr...<remainder of security token>"",
            ""Expiration"": ""2024-08-08T18:44:24Z""
        }";

        private Mock<HttpMessageHandler> httpMessageHandlerMock;
        private HttpClient httpClient;
        private ContainerCredentialsFetcher credentialsFetcher;


        [SetUp]
        public void SetUp()
        {
            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpClient = new HttpClient(httpMessageHandlerMock.Object);
            credentialsFetcher = new ContainerCredentialsFetcher(httpClient);
        }

        [Test]
        public void GIVEN_nullHttpClient_WHEN_constructor_THEN_throwArgumentNullException()
        {
            // GIVEN
            HttpClient nullHttpClient = null;

            // WHEN / THEN
            Assert.Throws<ArgumentNullException>(() => new ContainerCredentialsFetcher(nullHttpClient));
        }

        [Test]
        public void
            GIVEN_containerCredentialsProviderResponse_WHEN_fetchContainerCredentials_THEN_returnAwsCredentials()
        {
            // GIVEN
            Environment.SetEnvironmentVariable("AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", "/v2/credentials");
            var expectedUri = "http://169.254.170.2/v2/credentials";

            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(request => request.RequestUri.ToString() == expectedUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ContainerCredentialsProviderResponse)
                });

            // WHEN
            var result = credentialsFetcher.FetchContainerCredentials();

            // THEN
            Assert.AreEqual("AKIAIOSFODNN7EXAMPLE", result.AccessKey);
            Assert.AreEqual("wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY", result.SecretKey);
            Assert.AreEqual("AQoDYXdzEJr...<remainder of security token>", result.SessionToken);
        }

        [Test]
        public void
            GIVEN_missingEnvironmentVariable_WHEN_fetchContainerCredentials_THEN_throwInvalidOperationException()
        {
            // GIVEN
            Environment.SetEnvironmentVariable("AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", null);

            // WHEN / Then
            var ex = Assert.Throws<InvalidOperationException>(() => credentialsFetcher.FetchContainerCredentials());
            Assert.AreEqual("The environment variable AWS_CONTAINER_CREDENTIALS_RELATIVE_URI is not set.", ex.Message);
        }

        [Test]
        public void GIVEN_httpRequestFails_WHEN_fetchContainerCredentials_THEN_throwHttpRequestException()
        {
            // GIVEN
            Environment.SetEnvironmentVariable("AWS_CONTAINER_CREDENTIALS_RELATIVE_URI", "/v2/credentials");

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
            Assert.Throws<HttpRequestException>(() => credentialsFetcher.FetchContainerCredentials());
        }
    }
}
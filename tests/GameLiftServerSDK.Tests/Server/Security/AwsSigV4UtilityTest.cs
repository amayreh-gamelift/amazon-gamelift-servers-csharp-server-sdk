using System;
using System.Collections.Generic;
using Aws.GameLift.Server.Security;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server.Security
{
    [TestFixture]
    public class AwsSigV4UtilityTest
    {
        [Test]
        public void GIVEN_validSigV4Parameters_WHEN_generateSigV4QueryString_THEN_returnExpectedQueryString()
        {
            // GIVEN
            var parameters = GenerateSigV4Parameters();

            // WHEN
            var result = AwsSigV4Utility.GenerateSigV4QueryString(parameters);

            // THEN
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Authorization=SigV4"));
            Assert.IsTrue(result.Contains("X-Amz-Algorithm=AWS4-HMAC-SHA256"));
            Assert.IsTrue(result.Contains("X-Amz-Credential=testAccessKey%2F20240805%2Fus-east-1%2Fgamelift%2Faws4_request"));
            Assert.IsTrue(result.Contains("X-Amz-Date=20240805T100000Z"));
            Assert.IsTrue(result.Contains("X-Amz-Security-Token=testSessionToken"));
            Assert.IsTrue(result.Contains("X-Amz-Signature=2601fe291f4b43a63f6ffb0e1d9085a1edbaa2a866c96511e153af3408bfe771"));
        }

        [Test]
        public void GIVEN_nullSigV4Parameters_WHEN_generateSigV4QueryString_THEN_throwArgumentNullException()
        {
            // GIVEN / WHEN / THEN
            Assert.Throws<ArgumentNullException>(() => AwsSigV4Utility.GenerateSigV4QueryString(null));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithNullAwsCredentials_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithNullValue = GenerateSigV4Parameters();
            parametersWithNullValue.AwsCredentials = null;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(
                () => AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithNullValue.AwsCredentials)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithNullAccessKey_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithNullValue = GenerateSigV4Parameters();
            parametersWithNullValue.AwsCredentials.AccessKey = null;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(
                () => AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithNullValue.AwsCredentials.AccessKey)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithEmptyAccessKey_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithEmptyValue = GenerateSigV4Parameters();
            parametersWithEmptyValue.AwsCredentials.AccessKey = string.Empty;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(() =>
                AwsSigV4Utility.GenerateSigV4QueryString(parametersWithEmptyValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithEmptyValue.AwsCredentials.AccessKey)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithNullSecretKey_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithNullValue = GenerateSigV4Parameters();
            parametersWithNullValue.AwsCredentials.SecretKey = null;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(
                () => AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithNullValue.AwsCredentials.SecretKey)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithEmptySecretKey_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithEmptyValue = GenerateSigV4Parameters();
            parametersWithEmptyValue.AwsCredentials.SecretKey = string.Empty;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(() =>
                AwsSigV4Utility.GenerateSigV4QueryString(parametersWithEmptyValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithEmptyValue.AwsCredentials.SecretKey)));
        }

        [Test]
        public void GIVEN_sigV4ParametersWithNullSessionToken_WHEN_generateSigV4QueryString_THEN_returnExpectedQueryString()
        {
            // GIVEN
            var parametersWithNullSessionToken = GenerateSigV4Parameters();
            parametersWithNullSessionToken.AwsCredentials.SessionToken = null;

            // WHEN
            var result = AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullSessionToken);

            // THEN
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Authorization=SigV4"));
            Assert.IsTrue(result.Contains("X-Amz-Algorithm=AWS4-HMAC-SHA256"));
            Assert.IsTrue(result.Contains("X-Amz-Credential=testAccessKey%2F20240805%2Fus-east-1%2Fgamelift%2Faws4_request"));
            Assert.IsTrue(result.Contains("X-Amz-Date=20240805T100000Z"));
            Assert.IsFalse(result.Contains("X-Amz-Security-Token="));
            Assert.IsTrue(result.Contains("X-Amz-Signature=2601fe291f4b43a63f6ffb0e1d9085a1edbaa2a866c96511e153af3408bfe771"));
        }

        [Test]
        public void GIVEN_sigV4ParametersWithEmptySessionToken_WHEN_generateSigV4QueryString_THEN_returnExpectedQueryString()
        {
            // GIVEN
            var parametersWithEmptySessionToken = GenerateSigV4Parameters();
            parametersWithEmptySessionToken.AwsCredentials.SessionToken = string.Empty;

            // WHEN
            var result = AwsSigV4Utility.GenerateSigV4QueryString(parametersWithEmptySessionToken);

            // THEN
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("Authorization=SigV4"));
            Assert.IsTrue(result.Contains("X-Amz-Algorithm=AWS4-HMAC-SHA256"));
            Assert.IsTrue(result.Contains("X-Amz-Credential=testAccessKey%2F20240805%2Fus-east-1%2Fgamelift%2Faws4_request"));
            Assert.IsTrue(result.Contains("X-Amz-Date=20240805T100000Z"));
            Assert.IsFalse(result.Contains("X-Amz-Security-Token="));
            Assert.IsTrue(result.Contains("X-Amz-Signature=2601fe291f4b43a63f6ffb0e1d9085a1edbaa2a866c96511e153af3408bfe771"));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithNullAwsRegion_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithNullValue = GenerateSigV4Parameters();
            parametersWithNullValue.AwsRegion = null;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(
                () => AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithNullValue.AwsRegion)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithEmptyAwsRegion_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithEmptyValue = GenerateSigV4Parameters();
            parametersWithEmptyValue.AwsRegion = string.Empty;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(() =>
                AwsSigV4Utility.GenerateSigV4QueryString(parametersWithEmptyValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithEmptyValue.AwsRegion)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithNullRequestTime_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithNullValue = GenerateSigV4Parameters();
            parametersWithNullValue.RequestTime = default;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(
                () => AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithNullValue.RequestTime)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithNullQueryParams_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithNullValue = GenerateSigV4Parameters();
            parametersWithNullValue.QueryParams = null;

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(
                () => AwsSigV4Utility.GenerateSigV4QueryString(parametersWithNullValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithNullValue.QueryParams)));
        }

        [Test]
        public void
            GIVEN_sigV4ParametersWithEmptyQueryParams_WHEN_generateSigV4QueryString_THEN_throwArgumentException()
        {
            // GIVEN
            var parametersWithEmptyValue = GenerateSigV4Parameters();
            parametersWithEmptyValue.QueryParams = new Dictionary<string, string>();

            // WHEN / THEN
            var e = Assert.Throws<ArgumentException>(() =>
                AwsSigV4Utility.GenerateSigV4QueryString(parametersWithEmptyValue));
            Assert.IsTrue(e.Message.Contains(nameof(parametersWithEmptyValue.QueryParams)));
        }

        private SigV4Parameters GenerateSigV4Parameters()
        {
            return new SigV4Parameters
            {
                AwsCredentials = new AwsCredentials()
                {
                    AccessKey = "testAccessKey",
                    SecretKey = "testSecretKey",
                    SessionToken = "testSessionToken",
                },
                AwsRegion = "us-east-1",
                QueryParams = new Dictionary<string, string>
                {
                    { "param1", "value1" },
                    { "param2", "value2" },
                },
                RequestTime = new DateTime(2024, 08, 05, 10, 00, 00, DateTimeKind.Utc),
            };
        }
    }
}
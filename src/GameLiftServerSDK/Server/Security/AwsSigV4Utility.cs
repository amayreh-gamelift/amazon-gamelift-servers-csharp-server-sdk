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
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Aws.GameLift.Server.Security
{
    /// <summary>
    /// A utility class for SigV4 signature generation.
    /// </summary>
    public static class AwsSigV4Utility
    {
        private const string DateFormat = "yyyyMMdd";
        private const string DateTimeFormat = "yyyyMMddTHHmmssZ";
        private const string ServiceName = "gamelift";
        private const string TerminationString = "aws4_request";
        private const string SignatureSecretKeyPrefix = "AWS4";
        private const string Algorithm = "AWS4-HMAC-SHA256";
        private const string AuthorizationKey = "Authorization";
        private const string AuthorizationValue = "SigV4";
        private const string AmzAlgorithmKey = "X-Amz-Algorithm";
        private const string AmzCredentialKey = "X-Amz-Credential";
        private const string AmzDateKey = "X-Amz-Date";
        private const string AmzSecurityTokenHeadersKey = "X-Amz-Security-Token";
        private const string AmzSignatureKey = "X-Amz-Signature";

        /// <summary>
        /// Generates a SigV4 QueryString based on the passed SignatureParameters.
        /// </summary>
        /// <param name="parameters">An object that holds parameters needed for SigV4 signature generation.</param>
        /// <returns>The SigV4 query parameters as string.</returns>
        public static string GenerateSigV4QueryString(SigV4Parameters parameters)
        {
            ValidateParameters(parameters);
            var formattedRequestDate = parameters.RequestTime.ToString(DateFormat);
            var formattedRequestDateTime = parameters.RequestTime.ToString(DateTimeFormat);

            var canonicalRequest = ToSortedEncodedQueryString(parameters.QueryParams);
            var hashedCanonicalRequest = ComputeSha256Hash(canonicalRequest);

            var scope = $"{formattedRequestDate}/{parameters.AwsRegion}/{ServiceName}/{TerminationString}";

            var stringToSign = $"{Algorithm}\n{formattedRequestDateTime}\n{scope}\n{hashedCanonicalRequest}";

            var credential = $"{parameters.AwsCredentials.AccessKey}/{scope}";

            var signature = GenerateSignature(
                parameters.AwsRegion,
                parameters.AwsCredentials.SecretKey,
                formattedRequestDate,
                ServiceName,
                stringToSign);

            var sigV4QueryString = GenerateSigV4QueryString(credential, formattedRequestDateTime,
                parameters.AwsCredentials.SessionToken,
                signature);
            return sigV4QueryString;
        }

        private static void ValidateParameters(SigV4Parameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (string.IsNullOrWhiteSpace(parameters.AwsRegion))
            {
                throw new ArgumentException("AwsRegion is required", nameof(parameters.AwsRegion));
            }

            if (parameters.AwsCredentials == null)
            {
                throw new ArgumentException("AwsCredentials is required", nameof(parameters.AwsCredentials));
            }

            if (string.IsNullOrWhiteSpace(parameters.AwsCredentials.AccessKey))
            {
                throw new ArgumentException("AccessKey is required", nameof(parameters.AwsCredentials.AccessKey));
            }

            if (string.IsNullOrWhiteSpace(parameters.AwsCredentials.SecretKey))
            {
                throw new ArgumentException("SecretKey is required", nameof(parameters.AwsCredentials.SecretKey));
            }

            if (parameters.QueryParams == null || !parameters.QueryParams.Any())
            {
                throw new ArgumentException("QueryParams is required", nameof(parameters.QueryParams));
            }

            if (parameters.RequestTime == default)
            {
                throw new ArgumentException("RequestTime is required", nameof(parameters.RequestTime));
            }
        }

        private static string GenerateSignature(
            string region,
            string secretKey,
            string formattedRequestDateTime,
            string serviceName,
            string stringToSign)
        {
            var encodedKeySecret = Encoding.UTF8.GetBytes($"{SignatureSecretKeyPrefix}{secretKey}");
            var hashDate = ComputeHmacSha256(encodedKeySecret, formattedRequestDateTime);
            var hashRegion = ComputeHmacSha256(hashDate, region);
            var hashService = ComputeHmacSha256(hashRegion, serviceName);
            var signingKey = ComputeHmacSha256(hashService, TerminationString);

            var signature = ToHex(ComputeHmacSha256(signingKey, stringToSign));
            return signature;
        }

        private static string GenerateSigV4QueryString(
            string credential,
            string formattedRequestDateTime,
            string sessionToken,
            string signature)
        {
            var sigV4QueryParameters = new Dictionary<string, string>
            {
                {
                    AuthorizationKey,
                    AuthorizationValue
                },
                {
                    AmzAlgorithmKey,
                    Algorithm
                },
                {
                    AmzCredentialKey,
                    credential
                },
                {
                    AmzDateKey,
                    formattedRequestDateTime
                },
                {
                    AmzSignatureKey,
                    signature
                },
            };
            if (!string.IsNullOrEmpty(sessionToken))
            {
                sigV4QueryParameters.Add(AmzSecurityTokenHeadersKey, sessionToken);
            }
            return ToSortedEncodedQueryString(sigV4QueryParameters);
        }

        private static string ToSortedEncodedQueryString(Dictionary<string, string> queryParameters)
        {
            return string.Join("&", queryParameters
                .OrderBy(keyValuePair => keyValuePair.Key)
                .Select(keyValuePair =>
                    $"{Uri.EscapeDataString(keyValuePair.Key)}={Uri.EscapeDataString(keyValuePair.Value)}"));
        }

        private static string ComputeSha256Hash(string data)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private static byte[] ComputeHmacSha256(byte[] key, string data)
        {
            byte[] keyBytes = key;
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (HMACSHA256 hmacSha256 = new HMACSHA256(keyBytes))
            {
                return hmacSha256.ComputeHash(dataBytes);
            }
        }

        private static string ToHex(byte[] hashBytes)
        {
            StringBuilder hashStringBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashStringBuilder.Append(b.ToString("x2"));
            }
            return hashStringBuilder.ToString();
        }
    }
}
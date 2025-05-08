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
using System.Net.Http;
using Newtonsoft.Json;

namespace Aws.GameLift.Server.Security
{
    /// <summary>
    /// This class is a utility that fetches container credentials.
    /// </summary>
    public class ContainerCredentialsFetcher
    {
        private const string ContainerCredentialProviderUrl = "http://169.254.170.2";
        private const string EnvironmentVariableContainerCredentialsRelativeUri =
            "AWS_CONTAINER_CREDENTIALS_RELATIVE_URI";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerCredentialsFetcher"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to be used for fetching Credentials.</param>
        public ContainerCredentialsFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Fetches container credentials from Container Credentials Provider.
        /// </summary>
        /// <returns>The AWS Credentials for the container.</returns>
        public AwsCredentials FetchContainerCredentials()
        {
            var relativeUri = Environment.GetEnvironmentVariable(EnvironmentVariableContainerCredentialsRelativeUri);
            if (string.IsNullOrEmpty(relativeUri))
            {
                throw new InvalidOperationException(
                    $"The environment variable {EnvironmentVariableContainerCredentialsRelativeUri} is not set.");
            }

            var credentialsProviderUri = $"{ContainerCredentialProviderUrl}{relativeUri}";

            var response = httpClient.GetAsync(credentialsProviderUri).Result;
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<AwsCredentials>(content);
        }
    }
}
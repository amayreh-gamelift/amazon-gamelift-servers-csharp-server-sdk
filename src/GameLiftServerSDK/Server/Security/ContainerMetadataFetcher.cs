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
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Aws.GameLift.Server.Security
{
    /// <summary>
    /// This class handles fetching container metadata.
    /// </summary>
    public class ContainerMetadataFetcher
    {
        private const string EnvironmentVariableContainerMetadataUri = "ECS_CONTAINER_METADATA_URI_V4";
        private const string TaskMetadataRelativePath = "task";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerMetadataFetcher"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to be used for fetching metadata.</param>
        public ContainerMetadataFetcher(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Fetches container task Metadata.
        /// </summary>
        /// <returns>The Metadata of the container task.</returns>
        public ContainerTaskMetadata FetchContainerTaskMetadata()
        {
            var containerMetadataUri = Environment.GetEnvironmentVariable(EnvironmentVariableContainerMetadataUri);
            if (string.IsNullOrEmpty(containerMetadataUri))
            {
                throw new InvalidOperationException(
                    $"The environment variable {EnvironmentVariableContainerMetadataUri} is not set.");
            }

            var containerTaskMetadataUri = $"{containerMetadataUri}/{TaskMetadataRelativePath}";
            var response = httpClient.GetAsync(containerTaskMetadataUri).Result;
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;
            var taskMetadata = JObject.Parse(content);
            var taskArn = taskMetadata["TaskARN"]?.ToString();
            if (string.IsNullOrEmpty(taskArn))
            {
                throw new InvalidOperationException("TaskArn is not available in container task metadata");
            }

            var taskId = taskArn.Split('/').Last();
            return new ContainerTaskMetadata()
            {
                TaskId = taskId,
            };
        }
    }
}
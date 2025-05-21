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
using Newtonsoft.Json;

namespace Aws.GameLift.Server.Security
{
    /// <summary>
    /// This class holds AWS Credentials.
    /// </summary>
    public class AwsCredentials
    {
        /// <summary>
        /// Gets or sets the access key of the AWS Credentials.
        /// </summary>
        [JsonProperty("AccessKeyId")]
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the secret key of the AWS Credentials.
        /// </summary>
        [JsonProperty("SecretAccessKey")]
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the session token of the AWS Credentials.
        /// </summary>
        [JsonProperty("Token")]
        public string SessionToken { get; set; }

        /// <summary>
        /// Gets or sets the expiration of the AWS Credentials.
        /// </summary>
        [JsonProperty("Expiration")]
        public DateTime Expiration { get; set; }
    }
}
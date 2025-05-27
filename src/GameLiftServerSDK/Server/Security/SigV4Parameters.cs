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

namespace Aws.GameLift.Server.Security
{
    /// <summary>
    /// This class holds SigV4 Signing Parameters.
    /// </summary>
    public class SigV4Parameters
    {
        /// <summary>
        /// Gets or sets the AWS Region.
        /// </summary>
        public string AwsRegion { get; set; }

        /// <summary>
        /// Gets or sets AWS Credentials.
        /// </summary>
        public AwsCredentials AwsCredentials { get; set; }

        /// <summary>
        /// Gets or sets the QueryParams to be included in the Canonical Request.
        /// </summary>
        public Dictionary<string, string> QueryParams { get; set; }

        /// <summary>
        /// Gets or sets the RequestTime.
        /// </summary>
        public DateTime RequestTime { get; set; }
    }
}
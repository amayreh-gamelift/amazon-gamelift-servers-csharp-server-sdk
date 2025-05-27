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

namespace Aws.GameLift.Server
{
    /// <summary>
    /// Connection information and methods for maintaining the connection between Amazon GameLift Servers
    /// and your game server.
    /// </summary>
    public struct ServerParameters : IEquatable<ServerParameters>
    {
        public string WebSocketUrl { get; set; }

        public string ProcessId { get; set; }

        public string HostId { get; set; }

        public string FleetId { get; set; }

        public string AuthToken { get; set; }

        public string AwsRegion { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string SessionToken { get; set; }

        public ServerParameters(
            string webSocketUrl,
            string processId,
            string hostId,
            string fleetId,
            string authToken)
        {
            WebSocketUrl = webSocketUrl;
            ProcessId = processId;
            HostId = hostId;
            FleetId = fleetId;
            AuthToken = authToken;
            AwsRegion = null;
            AccessKey = null;
            SecretKey = null;
            SessionToken = null;
        }

        public ServerParameters(
            string webSocketUrl,
            string processId,
            string hostId,
            string fleetId,
            string awsRegion,
            string accessKey,
            string secretKey,
            string sessionToken)
        {
            WebSocketUrl = webSocketUrl;
            ProcessId = processId;
            HostId = hostId;
            FleetId = fleetId;
            AuthToken = null;
            AwsRegion = awsRegion;
            AccessKey = accessKey;
            SecretKey = secretKey;
            SessionToken = sessionToken;
        }

        public bool Equals(ServerParameters other)
        {
            return WebSocketUrl == other.WebSocketUrl && ProcessId == other.ProcessId && HostId == other.HostId &&
                   FleetId == other.FleetId && AuthToken == other.AuthToken && AwsRegion == other.AwsRegion &&
                   AccessKey == other.AccessKey && SecretKey == other.SecretKey && SessionToken == other.SessionToken;
        }
    }
}

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

namespace Aws.GameLift.Server
{
    /// <summary>
    /// Connection information and methods for maintaining the connection between GameLift
    /// and your game server.
    /// </summary>
    public struct ServerParameters
    {
        public string webSocketUrl { get; set; }
        public string processId { get; set; }
        public string hostId { get; set; }
        public string fleetId { get; set; }
        public string authToken { get; set; }

        public ServerParameters(string webSocketUrl, string processId, string hostId, string fleetId, string authToken)
        {
            this.webSocketUrl = webSocketUrl;
            this.processId = processId;
            this.hostId = hostId;
            this.fleetId = fleetId;
            this.authToken = authToken;
        }
    }
}

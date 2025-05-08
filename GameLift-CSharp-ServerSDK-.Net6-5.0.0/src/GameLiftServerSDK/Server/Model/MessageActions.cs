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

namespace Aws.GameLift.Server.Model
{
    public static class MessageActions
    {
        public const string ACCEPT_PLAYER_SESSION = "AcceptPlayerSession";
        public const string ACTIVATE_GAME_SESSION = "ActivateGameSession";
        public const string TERMINATE_SERVER_PROCESS = "TerminateServerProcess";
        public const string ACTIVATE_SERVER_PROCESS = "ActivateServerProcess";
        public const string UPDATE_PLAYER_SESSION_CREATION_POLICY = "UpdatePlayerSessionCreationPolicy";
        public const string CREATE_GAME_SESSION = "CreateGameSession";
        public const string UPDATE_GAME_SESSION = "UpdateGameSession";
        public const string START_MATCH_BACKFILL = "StartMatchBackfill";
        public const string TERMINATE_PROCESS = "TerminateProcess";
        public const string DESCRIBE_PLAYER_SESSIONS = "DescribePlayerSessions";
        public const string STOP_MATCH_BACKFILL = "StopMatchBackfill";
        public const string HEARTBEAT_SERVER_PROCESS = "HeartbeatServerProcess";
        public const string GET_COMPUTE_CERTIFICATE = "GetComputeCertificate";
        public const string GET_FLEET_ROLE_CREDENTIALS = "GetFleetRoleCredentials";
        public const string REFRESH_CONNECTION = "RefreshConnection";
        public const string REMOVE_PLAYER_SESSION = "RemovePlayerSession";
    }
}

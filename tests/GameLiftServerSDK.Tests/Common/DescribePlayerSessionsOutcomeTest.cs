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

using System.Collections.Generic;
using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class DescribePlayerSessionsOutcomeTest
    {
        private const string NextToken = "nextToken";
        private static readonly List<PlayerSession> PlayerSessions = new List<PlayerSession>();

        [Test]
        public void GIVEN_describePlayerSessionsResult_WHEN_creatingDescribePlayerSessionsOutCome_THEN_noError()
        {
            // GIVEN
            var result = new DescribePlayerSessionsResult(PlayerSessions, NextToken);

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(result);

            // THEN
            Assert.AreEqual(result, outcome.Result);
            Assert.IsNull(outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftError_WHEN_creatingDescribePlayerSessionsOutCome_THEN_setsError()
        {
            // GIVEN
            var error = new GameLiftError();

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(error);

            // THEN
            Assert.AreEqual(error, outcome.Error);
            Assert.IsNull(outcome.Result);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void GIVEN_gameLiftErrorAndResult_WHEN_creatingDescribePlayerSessionsOutCome_THEN_setsErrorAndResult()
        {
            // GIVEN
            var error = new GameLiftError();
            var result = new DescribePlayerSessionsResult(PlayerSessions, NextToken);

            // WHEN
            var outcome = new DescribePlayerSessionsOutcome(error, result);

            // THEN
            Assert.AreEqual(error, outcome.Error);
            Assert.AreEqual(result, outcome.Result);
            Assert.IsFalse(outcome.Success);
        }
    }
}

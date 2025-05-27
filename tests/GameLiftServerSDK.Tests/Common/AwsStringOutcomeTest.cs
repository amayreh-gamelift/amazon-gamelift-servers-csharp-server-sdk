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

using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class AwsStringOutcomeTest
    {
        [Test]
        public void ResultAlwaysCreatesOutcomeWithSuccessAndNoError()
        {
            // Given
            // When
            AwsStringOutcome outcome = new AwsStringOutcome("result");

            // Then
            Assert.AreEqual("result", outcome.Result);
            Assert.IsNull(outcome.Error);
            Assert.IsTrue(outcome.Success);
        }

        [Test]
        public void ErrorAlwaysCreatesOutcomeWithNoSuccessAndNoResult()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            AwsStringOutcome outcome = new AwsStringOutcome(error);

            // Then
            Assert.IsNull(outcome.Result);
            Assert.AreEqual(error, outcome.Error);
            Assert.IsFalse(outcome.Success);
        }

        [Test]
        public void ErrorAndResultAlwaysCreatesOutcomeWithNoSuccessAndResult()
        {
            // Given
            GameLiftError error = new GameLiftError();

            // When
            AwsStringOutcome outcome = new AwsStringOutcome(error, "result");

            // Then
            Assert.AreEqual("result", outcome.Result);
            Assert.AreEqual(error, outcome.Error);
            Assert.IsFalse(outcome.Success);
        }
    }
}

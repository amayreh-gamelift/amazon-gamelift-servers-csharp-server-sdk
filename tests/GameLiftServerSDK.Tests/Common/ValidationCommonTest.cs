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

using System.Text.RegularExpressions;
using Aws.GameLift.Server;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class ValidationCommonTest
    {
        private const string ValidString = "validstring";
        private const string InvalidString = "invalidstring!";
        private const string Pattern = @"^[a-zA-Z0-9]+$";
        private const string FieldName = "FieldName";
        private const int MaxLength = 100;
        private const string RequiredError = "FieldName is required.";
        private const string LengthError = "FieldName is invalid. Length must be between 1 and 100 characters.";
        private const string LengthErrorTooShortNoMax = "FieldName is invalid. Length must be at least 2 characters.";
        private const string LengthErrorTooShort = "FieldName is invalid. Length must be between 2 and 100 characters.";
        private const string OverrideErrorMessage = "FieldName is invalid because of some other reason.";
        private const string FormatError = "FieldName is invalid. Must match the pattern: ^[a-zA-Z0-9]+$.";

        private readonly Regex testRegex = new Regex(Pattern);

        [Test]
        public void GIVEN_validString_WHEN_validateString_THEN_success()
        {
            // Given
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, ValidString, true, maxLength: MaxLength,
                regex: testRegex);

            // Then
            Assert.True(outcome.Success);
        }

        [Test]
        public void GIVEN_emptyOrNullString_WHEN_validateStringNotRequired_THEN_success()
        {
            // Given
            string emptyString = "";
            string nullString = null;
            // When
            GenericOutcome outcomeEmpty = ValidationCommon.ValidateString(FieldName, emptyString, false, maxLength: MaxLength,
                regex: testRegex);
            GenericOutcome outcomeNull = ValidationCommon.ValidateString(FieldName, nullString, false, maxLength: MaxLength,
                regex: testRegex);
            // Then
            Assert.True(outcomeEmpty.Success);
            Assert.True(outcomeNull.Success);
        }

        [Test]
        public void GIVEN_emptyString_WHEN_validateString_AND_Required_THEN_error()
        {
            // Given
            string emptyString = "";
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, emptyString, true, maxLength: MaxLength,
                regex: testRegex);
            // Then
            Assert.False(outcome.Success);
            Assert.AreEqual(RequiredError, outcome.Error.ErrorMessage);
        }

        [Test]
        public void GIVEN_tooLongString_WHEN_validateString_THEN_error()
        {
            // Given
            string tooLongString = new string('a', MaxLength + 1);
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, tooLongString, true, maxLength: MaxLength,
                regex: testRegex);
            // Then
            Assert.False(outcome.Success);
            Assert.AreEqual(LengthError, outcome.Error.ErrorMessage);
        }

        [Test]
        public void GIVEN_tooShortString_WHEN_validateString_THEN_error()
        {
            // Given
            string tooShortString = "a";
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, tooShortString, true, minLength: 2, maxLength: MaxLength,
                regex: testRegex);
            // Then
            Assert.False(outcome.Success);
            Assert.AreEqual(LengthErrorTooShort, outcome.Error.ErrorMessage);
        }

        [Test]
        public void GIVEN_tooShortString_AND_noMax_WHEN_validateString_THEN_error()
        {
            // Given
            string tooShortString = "a";
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, tooShortString, true, minLength: 2,
                regex: testRegex);
            // Then
            Assert.False(outcome.Success);
            Assert.AreEqual(LengthErrorTooShortNoMax, outcome.Error.ErrorMessage);
        }

        [Test]
        public void GIVEN_invalidString_WHEN_validateString_THEN_error()
        {
            // Given
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, InvalidString, true, maxLength: MaxLength,
                regex: testRegex);
            // Then
            Assert.False(outcome.Success);
            Assert.AreEqual(FormatError, outcome.Error.ErrorMessage);
        }

        [Test]
        public void GIVEN_invalidString_AND_overrideErrorMessage_WHEN_validateString_THEN_errorWithCustomMessage()
        {
            // Given
            // When
            GenericOutcome outcome = ValidationCommon.ValidateString(FieldName, InvalidString, true, maxLength: MaxLength,
                regex: testRegex, overrideErrorMessage: OverrideErrorMessage);
            // Then
            Assert.False(outcome.Success);
            Assert.AreEqual(OverrideErrorMessage, outcome.Error.ErrorMessage);
        }
    }
}
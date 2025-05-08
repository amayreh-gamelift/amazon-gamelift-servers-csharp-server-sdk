using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class GameLiftErrorsTest
    {
        [Test]
        public void JustErrorTypeCreatesObjectWithDefaultNameAndMessage()
        {
            // Given
            // When
            GameLiftError error = new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED);

            // Then
            Assert.AreEqual(GameLiftErrorType.SERVICE_CALL_FAILED, error.ErrorType);
            Assert.AreEqual("Service call failed.", error.ErrorName);
            Assert.AreEqual("An AWS service call has failed. See the root cause error for more information.", error.ErrorMessage);
        }

        [Test]
        public void AllParamsCreateFullErrorObject()
        {
            // Given
            string errorName = "Error.";
            string errorMessage = "Error Message.";

            // When
            GameLiftError error = new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED, errorName, errorMessage);

            // Then
            Assert.AreEqual(GameLiftErrorType.SERVICE_CALL_FAILED, error.ErrorType);
            Assert.AreEqual(errorName, error.ErrorName);
            Assert.AreEqual(errorMessage, error.ErrorMessage);
        }

        [Test]
        [TestCaseSource(nameof(GetClientSideErrors))]
        public void GIVEN_4xxStatusCode_WHEN_constructor_THEN_convertsToErrorType(int statusCode)
        {
            // Given
            string errorMessage = "Error Message.";

            // When
            GameLiftError error = new GameLiftError(statusCode, errorMessage);

            // Then
            Assert.AreEqual(GameLiftErrorType.BAD_REQUEST_EXCEPTION, error.ErrorType);
            Assert.AreEqual(errorMessage, error.ErrorMessage);
        }

        public static IEnumerable<int> GetClientSideErrors()
        {
            return Enumerable.Range(400, 100);
        }

        [Test]
        [TestCaseSource(nameof(GetServerSideErrors))]
        public void GIVEN_5xxStatusCode_WHEN_constructor_THEN_convertsToErrorType(int statusCode)
        {
            // Given
            string errorMessage = "Error Message.";

            // When
            GameLiftError error = new GameLiftError(statusCode, errorMessage);

            // Then
            Assert.AreEqual(GameLiftErrorType.INTERNAL_SERVICE_EXCEPTION, error.ErrorType);
            Assert.AreEqual(errorMessage, error.ErrorMessage);
        }

        public static IEnumerable<int> GetServerSideErrors()
        {
            return Enumerable.Range(500, 100);
        }
    }
}

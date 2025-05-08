using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Aws.GameLift
{
    [TestFixture]
    public class GameLiftErrorsTest
    {
        [Test]
        public void justErrorTypeCreatesObjectWithDefaultNameAndMessage()
        {
            // Given
            // When
            GameLiftError error = new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED);

            // Then
            Assert.AreEqual(error.ErrorType, GameLiftErrorType.SERVICE_CALL_FAILED);
            Assert.AreEqual(error.ErrorName, "Service call failed.");
            Assert.AreEqual(error.ErrorMessage, "An AWS service call has failed. See the root cause error for more information.");
        }

        [Test]
        public void allParamsCreateFullErrorObject()
        {
            // Given
            string errorName = "Error.";
            string errorMessage = "Error Message.";

            // When
            GameLiftError error = new GameLiftError(GameLiftErrorType.SERVICE_CALL_FAILED, errorName, errorMessage);

            // Then
            Assert.AreEqual(error.ErrorType, GameLiftErrorType.SERVICE_CALL_FAILED);
            Assert.AreEqual(error.ErrorName, errorName);
            Assert.AreEqual(error.ErrorMessage, errorMessage);
        }

        [Test, TestCaseSource(nameof(GetClientSideErrors))]
        public void GIVEN_4xxStatusCode_WHEN_constructor_THEN_convertsToErrorType(int statusCode)
        {
            // Given
            string errorMessage = "Error Message.";
            
            // When
            GameLiftError error = new GameLiftError(statusCode, errorMessage);
            
            // Then
            Assert.AreEqual(error.ErrorType, GameLiftErrorType.BAD_REQUEST_EXCEPTION);
            Assert.AreEqual(error.ErrorMessage, errorMessage);
        }

        public static IEnumerable<int> GetClientSideErrors()
        {
            return Enumerable.Range(400, 100);
        }

        [Test, TestCaseSource(nameof(GetServerSideErrors))]
        public void GIVEN_5xxStatusCode_WHEN_constructor_THEN_convertsToErrorType(int statusCode)
        {
            // Given
            string errorMessage = "Error Message.";
            
            // When
            GameLiftError error = new GameLiftError(statusCode, errorMessage);
            
            // Then
            Assert.AreEqual(error.ErrorType, GameLiftErrorType.INTERNAL_SERVICE_EXCEPTION);
            Assert.AreEqual(error.ErrorMessage, errorMessage);
        }

        public static IEnumerable<int> GetServerSideErrors()
        {
            return Enumerable.Range(500, 100);
        }
    }
}
using Aws.GameLift.Server.Model;
using NUnit.Framework;

namespace Aws.GameLift.Tests.Server.Model
{
    [TestFixture]
    public class PlayerSessionCreationPolicyTest
    {
        [Test]
        public void AcceptAllNameIsAlwaysMappedToPlayerSessionPolicy()
        {
            // Given
            // When
            PlayerSessionCreationPolicy policy = PlayerSessionCreationPolicyMapper.GetPlayerSessionCreationPolicyForName("ACCEPT_ALL");

            // Then
            Assert.AreEqual(PlayerSessionCreationPolicy.ACCEPT_ALL, policy);
        }

        [Test]
        public void DenyAllNameIsAlwaysMappedToPlayerSessionPolicy()
        {
            // Given
            // When
            PlayerSessionCreationPolicy policy = PlayerSessionCreationPolicyMapper.GetPlayerSessionCreationPolicyForName("DENY_ALL");

            // Then
            Assert.AreEqual(PlayerSessionCreationPolicy.DENY_ALL, policy);
        }

        [Test]
        public void NotSetNameIsAlwaysMappedToPlayerSessionPolicy()
        {
            // Given
            // When
            PlayerSessionCreationPolicy policy = PlayerSessionCreationPolicyMapper.GetPlayerSessionCreationPolicyForName(string.Empty);

            // Then
            Assert.AreEqual(PlayerSessionCreationPolicy.NOT_SET, policy);
        }

        [Test]
        public void AcceptAllPolicyIsAlwaysMappedToName()
        {
            // Given
            // When
            string name = PlayerSessionCreationPolicyMapper.GetNameForPlayerSessionCreationPolicy(PlayerSessionCreationPolicy.ACCEPT_ALL);

            // Then
            Assert.AreEqual("ACCEPT_ALL", name);
        }

        [Test]
        public void DenyAllPolicyIsAlwaysMappedToName()
        {
            // Given
            // When
            string name =
                PlayerSessionCreationPolicyMapper.GetNameForPlayerSessionCreationPolicy(PlayerSessionCreationPolicy.DENY_ALL);

            // Then
            Assert.AreEqual("DENY_ALL", name);
        }

        [Test]
        public void NotSetPolicyIsAlwaysMappedToName()
        {
            // Given
            // When
            string name =
                PlayerSessionCreationPolicyMapper.GetNameForPlayerSessionCreationPolicy(PlayerSessionCreationPolicy.NOT_SET);

            // Then
            Assert.AreEqual("NOT_SET", name);
        }
    }
}

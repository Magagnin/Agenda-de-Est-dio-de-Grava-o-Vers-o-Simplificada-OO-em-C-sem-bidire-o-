using EstudioGravacao.Domain.Entities;
using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;
using Xunit;

namespace EstudioGravacao.Tests.Domain
{
    public class SessionTests
    {
        private readonly DateRange _validRange = new(DateTime.Now, DateTime.Now.AddHours(2));
        private readonly Musician _musician1 = new("John");
        private readonly Musician _musician2 = new("Paul");

        [Fact]
        public void Create_WithNullParticipants_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Session(_validRange, null!));
        }

        [Fact]
        public void Create_WithEmptyParticipants_ThrowsDomainException()
        {
            // Arrange
            var participants = new List<Musician>();

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => new Session(_validRange, participants));
            Assert.Equal("Session must have at least one participant.", ex.Message);
        }

        [Fact]
        public void Create_WithDuplicateParticipants_ThrowsDomainException()
        {
            // Arrange
            var participants = new List<Musician> { _musician1, _musician2, _musician1 }; // Duplicado

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => new Session(_validRange, participants));
            Assert.Contains("Duplicate musician detected", ex.Message);
        }

        [Fact]
        public void Create_WithValidParticipants_Success()
        {
            // Arrange
            var participants = new List<Musician> { _musician1, _musician2 };

            // Act
            var session = new Session(_validRange, participants);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(2, session.Participants.Count);
        }
    }
}

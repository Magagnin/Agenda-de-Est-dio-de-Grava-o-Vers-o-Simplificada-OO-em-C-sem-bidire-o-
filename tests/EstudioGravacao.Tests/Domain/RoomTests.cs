using EstudioGravacao.Domain.Entities;
using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;
using Xunit;

namespace EstudioGravacao.Tests.Domain
{
    public class RoomTests
    {
        private readonly DateTime _baseTime = new(2025, 1, 1, 12, 0, 0);
        private readonly Musician _musician1 = new("Ringo");
        private readonly List<Musician> _participants;
        private readonly Room _room;

        public RoomTests()
        {
            _participants = new List<Musician> { _musician1 };
            _room = new Room("Studio A");
        }

        [Fact]
        public void BookSession_FirstBooking_Success()
        {
            // Arrange
            var range = new DateRange(_baseTime, _baseTime.AddHours(2)); // 12:00-14:00

            // Act
            var session = _room.BookSession(range, _participants);

            // Assert
            Assert.NotNull(session);
            Assert.Single(_room.Sessions);
            Assert.Equal(range, session.When);
        }

        [Fact]
        public void BookSession_WithCollision_ThrowsDomainException()
        {
            // Arrange
            // Agendamento 1: 12:00-14:00
            var firstRange = new DateRange(_baseTime, _baseTime.AddHours(2));
            _room.BookSession(firstRange, _participants);

            // Agendamento 2 (Colisão): 13:00-15:00
            var collidingRange = new DateRange(_baseTime.AddHours(1), _baseTime.AddHours(3));

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => _room.BookSession(collidingRange, _participants));
            Assert.Equal("Booking collision detected. The room is already booked for this time range.", ex.Message);
        }

        [Fact]
        public void BookSession_WithoutCollision_Success()
        {
            // Arrange
            // Agendamento 1: 12:00-14:00
            var firstRange = new DateRange(_baseTime, _baseTime.AddHours(2));
            _room.BookSession(firstRange, _participants);

            // Agendamento 2 (Sem Colisão): 14:00-16:00
            var secondRange = new DateRange(_baseTime.AddHours(2), _baseTime.AddHours(4));

            // Act
            var session = _room.BookSession(secondRange, _participants);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(2, _room.Sessions.Count);
        }

        [Fact]
        public void BookSession_WithInvalidParticipants_PropagatesException()
        {
            // Arrange
            var range = new DateRange(_baseTime, _baseTime.AddHours(2));
            var emptyParticipants = new List<Musician>(); // Inválido

            // Act & Assert
            // A exceção vem da Session, provando que a Room delegou a validação.
            var ex = Assert.Throws<DomainException>(() => _room.BookSession(range, emptyParticipants));
            Assert.Equal("Session must have at least one participant.", ex.Message);
        }
    }
}

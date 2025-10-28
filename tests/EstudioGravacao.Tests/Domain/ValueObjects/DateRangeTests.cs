using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;
using Xunit;

namespace EstudioGravacao.Tests.Domain.ValueObjects
{
    public class DateRangeTests
    {
        private readonly DateTime _baseTime = new(2025, 1, 1, 10, 0, 0);

        [Fact]
        public void Create_StartDateAfterEndDate_ThrowsDomainException()
        {
            // Arrange
            var start = _baseTime;
            var end = _baseTime.AddHours(-1);

            // Act & Assert
            Assert.Throws<DomainException>(() => new DateRange(start, end));
        }

        [Fact]
        public void Create_StartDateEqualsEndDate_ThrowsDomainException()
        {
            // Arrange
            var start = _baseTime;
            var end = _baseTime;

            // Act & Assert
            Assert.Throws<DomainException>(() => new DateRange(start, end));
        }

        // Testes de Overlap
        // Range Base: [10:00 --- 12:00]

        [Fact]
        public void Overlaps_WhenCompletelyInside_ReturnsTrue()
        {
            // Arrange
            var baseRange = new DateRange(_baseTime, _baseTime.AddHours(2)); // 10:00-12:00
            var testRange = new DateRange(_baseTime.AddMinutes(30), _baseTime.AddHours(1)); // 10:30-11:00

            // Act & Assert
            Assert.True(baseRange.Overlaps(testRange));
            Assert.True(testRange.Overlaps(baseRange));
        }

        [Fact]
        public void Overlaps_WhenStartsBeforeAndEndsInside_ReturnsTrue()
        {
            // Arrange
            var baseRange = new DateRange(_baseTime, _baseTime.AddHours(2)); // 10:00-12:00
            var testRange = new DateRange(_baseTime.AddHours(-1), _baseTime.AddHours(1)); // 09:00-11:00

            // Act & Assert
            Assert.True(baseRange.Overlaps(testRange));
        }

        [Fact]
        public void Overlaps_WhenStartsInsideAndEndsAfter_ReturnsTrue()
        {
            // Arrange
            var baseRange = new DateRange(_baseTime, _baseTime.AddHours(2)); // 10:00-12:00
            var testRange = new DateRange(_baseTime.AddHours(1), _baseTime.AddHours(3)); // 11:00-13:00

            // Act & Assert
            Assert.True(baseRange.Overlaps(testRange));
        }

        [Fact]
        public void Overlaps_WhenCompletelyWraps_ReturnsTrue()
        {
            // Arrange
            var baseRange = new DateRange(_baseTime, _baseTime.AddHours(2)); // 10:00-12:00
            var testRange = new DateRange(_baseTime.AddHours(-1), _baseTime.AddHours(3)); // 09:00-13:00

            // Act & Assert
            Assert.True(baseRange.Overlaps(testRange));
        }

        [Fact]
        public void Overlaps_WhenCompletelyBefore_ReturnsFalse()
        {
            // Arrange
            var baseRange = new DateRange(_baseTime, _baseTime.AddHours(2)); // 10:00-12:00
            var testRange = new DateRange(_baseTime.AddHours(-2), _baseTime.AddHours(-1)); // 08:00-09:00

            // Act & Assert
            Assert.False(baseRange.Overlaps(testRange));
        }

        [Fact]
        public void Overlaps_WhenTouchingEnd_ReturnsFalse()
        {
            // Arrange
            // [10:00 --- 12:00] e [12:00 --- 13:00]
            var baseRange = new DateRange(_baseTime, _baseTime.AddHours(2));
            var testRange = new DateRange(_baseTime.AddHours(2), _baseTime.AddHours(3));

            // Act & Assert
            // Não há sobreposição; uma sessão pode começar exatamente quando a outra termina.
            Assert.False(baseRange.Overlaps(testRange));
        }
    }
}

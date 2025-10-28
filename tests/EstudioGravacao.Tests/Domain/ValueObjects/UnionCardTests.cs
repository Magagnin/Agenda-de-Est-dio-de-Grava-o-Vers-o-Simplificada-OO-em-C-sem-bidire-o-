using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;
using Xunit;

namespace EstudioGravacao.Tests.Domain.ValueObjects
{
    public class UnionCardTests
    {
        [Fact]
        public void Create_ValidNumber_Success()
        {
            // Arrange
            var number = "OMB-12345";
            
            // Act
            var card = new UnionCard(number);

            // Assert
            Assert.Equal(number, card.Number);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_NullOrEmpty_ThrowsDomainException(string invalidNumber)
        {
            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => new UnionCard(invalidNumber));
            Assert.Equal("Union card number cannot be empty.", ex.Message);
        }

        [Fact]
        public void Create_InvalidFormat_ThrowsDomainException()
        {
            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => new UnionCard("INVALID-123"));
            Assert.Equal("Invalid union card format. Must start with 'OMB-'.", ex.Message);
        }

        [Fact]
        public void Equality_SameNumber_ReturnsTrue()
        {
            // Arrange
            var card1 = new UnionCard("OMB-777");
            var card2 = new UnionCard("OMB-777");

            // Act & Assert
            Assert.True(card1 == card2);
            Assert.Equal(card1, card2);
            Assert.NotSame(card1, card2); // Prova que são instâncias diferentes, mas valores iguais
        }
    }
}

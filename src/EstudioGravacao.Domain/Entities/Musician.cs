using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;

namespace EstudioGravacao.Domain.Entities
{
    public class Musician
    {
        public Guid Id { get; }
        public string Name { get; }
        
        // Multiplicidade 0..1 (Um músico PODE ter uma carteira sindical)
        public UnionCard? Card { get; }

        public Musician(string name, UnionCard? card = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Musician name cannot be empty.");
            }

            Id = Guid.NewGuid();
            Name = name;
            Card = card;
        }
    }
}

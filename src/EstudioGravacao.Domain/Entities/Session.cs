using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;
using System.Collections.Generic;

namespace EstudioGravacao.Domain.Entities
{
    public class Session
    {
        public Guid Id { get; }
        
        // Multiplicidade 1:1 (Sessão DEVE ter um intervalo)
        public DateRange When { get; }

        // Multiplicidade 1..N (Sessão DEVE ter participantes)
        private readonly List<Musician> _participants = new();
        public IReadOnlyCollection<Musician> Participants => _participants.AsReadOnly();

        public Session(DateRange when, IEnumerable<Musician> participants)
        {
            Id = Guid.NewGuid();
            When = when ?? throw new DomainException("Session must have a date range.");

            ArgumentNullException.ThrowIfNull(participants);

            // Invariante: Sem participantes duplicados
            var uniqueParticipants = new HashSet<Guid>();
            foreach (var musician in participants)
            {
                if (!uniqueParticipants.Add(musician.Id))
                {
                    throw new DomainException($"Duplicate musician detected: {musician.Name}");
                }
                _participants.Add(musician);
            }

            // Invariante: Deve ter pelo menos 1 participante
            if (_participants.Count == 0)
            {
                throw new DomainException("Session must have at least one participant.");
            }
        }
    }
}

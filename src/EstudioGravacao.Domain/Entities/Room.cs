using EstudioGravacao.Domain.Exceptions;
using EstudioGravacao.Domain.ValueObjects;
using System.Collections.Generic;

namespace EstudioGravacao.Domain.Entities
{
    /// <summary>
    /// Representa a Sala de Gravação (Aggregate Root).
    /// É responsável por gerenciar seu próprio calendário (Sessões).
    /// </summary>
    public class Room
    {
        public Guid Id { get; }
        public string Name { get; }

        // Navegabilidade 1..N (Room -> Session)
        // A Sala "conhece" suas sessões.
        // A Sessão NÃO conhece sua sala (sem bidireção).
        private readonly List<Session> _sessions = new();
        public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

        public Room(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Room name cannot be empty.");
            }
            Id = Guid.NewGuid();
            Name = name;
        }

        /// <summary>
        /// Método de fábrica para criar e agendar uma nova sessão.
        /// Protege o invariante de colisão de horários.
        /// </summary>
        public Session BookSession(DateRange when, List<Musician> participants)
        {
            // Invariante: Proteção contra colisão de horários
            CheckForBookingCollision(when);

            // Delega a criação da sessão (e suas regras) para a própria entidade Session
            var newSession = new Session(when, participants);

            _sessions.Add(newSession);
            return newSession;
        }

        private void CheckForBookingCollision(DateRange newRange)
        {
            bool hasCollision = _sessions.Any(session => session.When.Overlaps(newRange));
            
            if (hasCollision)
            {
                throw new DomainException("Booking collision detected. The room is already booked for this time range.");
            }
        }
    }
}

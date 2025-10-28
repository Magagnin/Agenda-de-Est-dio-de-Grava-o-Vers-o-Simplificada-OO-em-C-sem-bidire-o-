using EstudioGravacao.Domain.Exceptions;

namespace EstudioGravacao.Domain.ValueObjects
{
    public record DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateRange(DateTime start, DateTime end)
        {
            if (start >= end)
            {
                throw new DomainException("Start date must be before end date.");
            }

            Start = start;
            End = end;
        }

        /// <summary>
        /// Verifica se dois intervalos de tempo se sobrepõem.
        /// </summary>
        public bool Overlaps(DateRange other)
        {
            // Lógica padrão de verificação de sobreposição (excluindo limites exatos)
            return this.Start < other.End && other.Start < this.End;
        }
    }
}

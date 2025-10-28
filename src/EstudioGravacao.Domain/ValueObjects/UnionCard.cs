using EstudioGravacao.Domain.Exceptions;

namespace EstudioGravacao.Domain.ValueObjects
{
    // Usa 'record' para imutabilidade e igualdade semântica (baseada em valor)
    public record UnionCard
    {
        public string Number { get; }

        public UnionCard(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                throw new DomainException("Union card number cannot be empty.");
            }
            // Exemplo de regra: Deve conter 'OMB'
            if (!number.StartsWith("OMB-"))
            {
                throw new DomainException("Invalid union card format. Must start with 'OMB-'.");
            }
            
            Number = number;
        }
    }
}

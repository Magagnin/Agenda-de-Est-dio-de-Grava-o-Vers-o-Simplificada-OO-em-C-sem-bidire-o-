namespace EstudioGravacao.Domain.Exceptions
{
    // Exceção customizada para falhas de regras de negócio (Invariantes)
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}

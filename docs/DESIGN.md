# Documento de Design: Agenda de Estúdio de Gravação

Este documento descreve as decisões de modelagem de domínio (OO) para o sistema de agendamento de estúdios, com foco em navegabilidade, invariantes e Value Objects.

## 1. Decisões de Modelagem e Navegabilidade

O núcleo do domínio é composto pelas entidades `Room`, `Session` e `Musician`.

### Navegabilidade e Multiplicidade (Sem Bidireção)

Para manter o encapsulamento e evitar grafos de objetos complexos, a navegabilidade foi definida em uma única direção, seguindo o padrão de *Aggregate Root*:

1.  **`Room` (Aggregate Root):** A `Room` é a raiz de agregação para agendamentos. Ela "possui" as sessões.
    * **`Room` 1..N `Session`:** A `Room` conhece sua lista de `Session` (`private readonly List<Session> _sessions`).
    * **Evidência (Código):** `src/EstudioGravacao.Domain/Entities/Room.cs`
        ```csharp
        public class Room
        {
            // ...
            private readonly List<Session> _sessions = new();
            public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
            // ...
        }
        ```

2.  **`Session` (Entidade Filha):** A `Session` é "propriedade" da `Room`.
    * **`Session` N..1 `Room` (REMOVIDO):** A `Session` **não** conhece a `Room` onde está agendada. Isso força que toda interação de agendamento passe pela `Room`, que é a única que pode garantir que não há colisões.

3.  **Participantes da Sessão:**
    * **`Session` 1..N `Musician`:** A `Session` conhece seus participantes. Uma sessão deve ter no mínimo 1 músico.
    * **`Musician` N..1 `Session` (REMOVIDO):** O `Musician` não conhece as sessões das quais participa. Essa informação pode ser obtida por consultas (Repositories) na camada de aplicação, se necessário.
    * **Evidência (Código):** `src/EstudioGravacao.Domain/Entities/Session.cs`
        ```csharp
        public class Session
        {
            // ...
            private readonly List<Musician> _participants = new();
            public IReadOnlyCollection<Musician> Participants => _participants.AsReadOnly();
            // ...
        }
        ```

4.  **Multiplicidade Nula (0..1):**
    * O `Musician` pode ou não ter uma `UnionCard` (carteira sindical). Isso é modelado com *Nullable Reference Types* (NRT).
    * **Evidência (Código):** `src/EstudioGravacao.Domain/Entities/Musician.cs`
        ```csharp
        public class Musician
        {
            // ...
            // Multiplicidade 0..1
            public UnionCard? Card { get; }
            // ...
        }
        ```

## 2. Value Objects (Imutabilidade e Igualdade)

Dois conceitos foram modelados como Value Objects (VOs) imutáveis e com igualdade semântica (baseada em valor), usando o tipo `record` do C#.

1.  **`DateRange`:** Representa um intervalo de tempo.
    * **Imutabilidade:** `record` com propriedades `init` (implícito).
    * **Igualdade Semântica:** `record` implementa `IEquatable<T>` automaticamente.
    * **Invariante:** `Start` deve ser sempre anterior a `End`.
    * **Evidência (Teste):** `tests/EstudioGravacao.Tests/Domain/ValueObjects/DateRangeTests.cs`
        ```csharp
        [Fact]
        public void Create_StartDateAfterEndDate_ThrowsDomainException()
        {
            // Arrange
            var start = new DateTime(2025, 1, 1, 10, 0, 0);
            var end = start.AddHours(-1);

            // Act & Assert
            Assert.Throws<DomainException>(() => new DateRange(start, end));
        }
        ```

2.  **`UnionCard`:** Representa a carteira sindical.
    * **Imutabilidade e Igualdade:** `record`.
    * **Invariante:** Validação de formato (não nulo e prefixo "OMB-").

## 3. Proteção de Invariantes (Guards)

As regras de negócio (invariantes) são protegidas dentro das entidades, lançando `DomainException` quando violadas.

### Invariante 1: Sem Colisão de Sessões (em `Room`)

A `Room`, como Aggregate Root, é responsável por garantir que nenhuma `Session` nova se sobreponha a uma existente.

* **Evidência (Código):** `src/EstudioGravacao.Domain/Entities/Room.cs`
    ```csharp
    public Session BookSession(DateRange when, List<Musician> participants)
    {
        // Invariante: Proteção contra colisão de horários
        CheckForBookingCollision(when);
        // ...
    }

    private void CheckForBookingCollision(DateRange newRange)
    {
        bool hasCollision = _sessions.Any(session => session.When.Overlaps(newRange));
        
        if (hasCollision)
        {
            throw new DomainException("Booking collision detected...");
        }
    }
    ```
* **Evidência (Teste):** `tests/EstudioGravacao.Tests/Domain/RoomTests.cs`
    ```csharp
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
    ```

### Invariante 2: Sem Participantes Duplicados (em `Session`)

A `Session` garante que um mesmo `Musician` não pode ser adicionado duas vezes na mesma sessão.

* **Evidência (Código):** `src/EstudioGravacao.Domain/Entities/Session.cs`
    ```csharp
    public Session(DateRange when, IEnumerable<Musician> participants)
    {
        // ...
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
        //...
    }
    ```
* **Evidência (Teste):** `tests/EstudioGravacao.Tests/Domain/SessionTests.cs`
    ```csharp
    [Fact]
    public void Create_WithDuplicateParticipants_ThrowsDomainException()
    {
        // Arrange
        var participants = new List<Musician> { _musician1, _musician2, _musician1 }; // Duplicado

        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => new Session(_validRange, participants));
        Assert.Contains("Duplicate musician detected", ex.Message);
    }
    ```

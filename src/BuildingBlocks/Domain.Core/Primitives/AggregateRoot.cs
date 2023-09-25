using CSharpFunctionalExtensions;
using Domain.Core.Events;

namespace Domain.Core.Primitives;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : IComparable<TId>
{
    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent newEvent)
    {
        _domainEvents.Add(newEvent);
    }

    public void ClearEvents()
    {
        _domainEvents.Clear();
    }
}

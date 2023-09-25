using Domain.Core.Primitives;
using Domain.Core.UnitOfWork;

namespace Domain.Core.Repositories;

public interface IRepository<TAggregateRoot, TId>
    where TAggregateRoot : AggregateRoot<TId>
    where TId : IComparable<TId>
{
    IUnitOfWork UnitOfWork { get; }
}

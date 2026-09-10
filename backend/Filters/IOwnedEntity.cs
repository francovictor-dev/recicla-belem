// Backend/Authorization/IOwnedEntity.cs
namespace Backend.Filters;

public interface IOwnedEntity
{
    Guid UserId { get; }
}
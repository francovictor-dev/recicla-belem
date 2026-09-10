using Backend.Filters;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;


public class OwnershipFilter<TEntity>(AppDbContext content) : IAsyncActionFilter where TEntity : class, IOwnedEntity
{
    private readonly AppDbContext _context = content;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var routeId = context.RouteData.Values["id"]?.ToString();
        var tokenUserId = context.HttpContext.User.FindFirst("sub")?.Value;
        
        if (routeId is null || !Guid.TryParse(routeId, out var entityId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var entity = await _context.Set<TEntity>().FindAsync(entityId);

        if (entity is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        if (entity.UserId.ToString() != tokenUserId)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
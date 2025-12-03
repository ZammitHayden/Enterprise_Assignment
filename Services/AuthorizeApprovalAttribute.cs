using Enterprise_Assignment.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

public class AuthorizeApprovalAttribute : ActionFilterAttribute
{
    private readonly ItemsDbRepository _dbRepository;

    public AuthorizeApprovalAttribute(ItemsDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        const string SessionKey = "LoggedInUser";

        var userJson = context.HttpContext.Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(userJson))
        {
            context.Result = new ForbidResult();
            return;
        }

        var userElement = JsonSerializer.Deserialize<JsonElement>(userJson);
        var userEmail = "";
        var isSiteAdmin = false;

        if (userElement.TryGetProperty("Email", out var emailElement))
        {
            userEmail = emailElement.GetString();
        }

        if (userElement.TryGetProperty("IsSiteAdmin", out var isAdminElement))
        {
            isSiteAdmin = isAdminElement.GetBoolean();
        }

        if (context.ActionArguments.TryGetValue("selectedItems", out var itemsParam) &&
            itemsParam is string[] itemIds && itemIds != null && itemIds.Length > 0)
        {
            foreach (var itemId in itemIds)
            {
                if (!string.IsNullOrEmpty(itemId))
                {
                    var item = _dbRepository.GetItemByIdString(itemId);
                    if (item != null)
                    {
                        var validators = item.GetValidators();

                        if (!validators.Contains(userEmail, StringComparer.OrdinalIgnoreCase) && !isSiteAdmin)
                        {
                            context.Result = new ForbidResult();
                            return;
                        }
                    }
                }
            }
        }
    }
}
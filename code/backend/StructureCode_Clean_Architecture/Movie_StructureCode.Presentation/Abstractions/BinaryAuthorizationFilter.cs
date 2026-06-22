using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Movie_StructureCode.Contract.Abstractions.Shared;
using System.Text.Json;

namespace Movie_StructureCode.Presentation.Abstractions
{
    public class PermissionRequirement
    {
        public string Group { get; set; }
        public ActionType ActionValue { get; set; }

        public PermissionRequirement(string group, ActionType actionValue)
        {
            Group = group;
            ActionValue = actionValue;
        }
    }

    public class BinaryAuthorizationFilter : IAuthorizationFilter, IEndpointFilter
    {
        public List<PermissionRequirement> Requirements { get; set; } = new();

        public BinaryAuthorizationFilter(string group, ActionType actionValue)
        {
            Requirements.Add(new PermissionRequirement(group, actionValue));
        }

        public BinaryAuthorizationFilter(params PermissionRequirement[] requirements)
        {
            Requirements.AddRange(requirements);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasRole = CanAccessToAction(context.HttpContext);
            if (!hasRole)
            {
                context.Result = new ForbidResult();
            }
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var hasRole = CanAccessToAction(context.HttpContext);
            if (!hasRole)
            {
                return Results.Forbid();
            }

            return await next(context);
        }

        private bool CanAccessToAction(HttpContext httpContext)
        {
            if (httpContext.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
                return false;

            var roleClaims = httpContext.User.FindAll(ClaimTypes.Role);
            string? claimRoleValue = null;
            foreach (var claim in roleClaims)
            {
                if (claim.Value != null && claim.Value.TrimStart().StartsWith("{"))
                {
                    claimRoleValue = claim.Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(claimRoleValue))
                return false;

            var log = httpContext.RequestServices.GetRequiredService<ILogger<BinaryAuthorizationFilter>>();
            try
            {
                var rawRoles = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(claimRoleValue);
                if (rawRoles == null)
                    return false;

                // Create case-insensitive dictionaries for robust matching
                var roles = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in rawRoles)
                {
                    var innerDict = kvp.Value != null
                        ? new Dictionary<string, string>(kvp.Value, StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    roles[kvp.Key] = innerDict;
                }

                // Loop through all requirements (OR logic)
                foreach (var req in Requirements)
                {
                    var actionValue = (int)req.ActionValue;
                    var upperGroup = req.Group.Trim().ToUpper();

                    // Loop through all roles and check if any role has the required permission category and action
                    foreach (var kvp in roles)
                    {
                        var groupPermissions = kvp.Value;
                        if (groupPermissions != null && groupPermissions.TryGetValue(upperGroup, out var binaryValue))
                        {
                            var roleValue = Convert.ToInt32(binaryValue, 2);
                            var checkValue = (roleValue & actionValue) == actionValue;
                            if (checkValue)
                                return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error while validating binary authorization.");
            }

            return false;
        }
    }
}

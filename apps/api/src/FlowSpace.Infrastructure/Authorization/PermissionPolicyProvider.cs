using Microsoft.AspNetCore.Authorization;
using FlowSpace.Domain.Authorization;

namespace FlowSpace.Infrastructure.Authorization;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options) 
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        // If the policyName matches one of our permissions, create a dynamic policy
        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}

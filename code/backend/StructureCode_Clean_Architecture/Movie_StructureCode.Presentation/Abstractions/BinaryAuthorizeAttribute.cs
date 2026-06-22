using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Movie_StructureCode.Contract.Abstractions.Shared;

namespace Movie_StructureCode.Presentation.Abstractions
{
    public class BinaryAuthorizeAttribute : TypeFilterAttribute
    {
        public List<PermissionRequirement> Requirements { get; set; } = new();

        public BinaryAuthorizeAttribute(string group, ActionType actionValue) : base(typeof(BinaryAuthorizationFilter))
        {
            Requirements.Add(new PermissionRequirement(group, actionValue));
            Arguments = new object[] { Requirements.ToArray() };
        }

        public BinaryAuthorizeAttribute(params PermissionRequirement[] requirements) : base(typeof(BinaryAuthorizationFilter))
        {
            Requirements.AddRange(requirements);
            Arguments = new object[] { Requirements.ToArray() };
        }
    }
}

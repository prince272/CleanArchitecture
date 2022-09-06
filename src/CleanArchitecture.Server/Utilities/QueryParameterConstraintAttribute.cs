using CleanArchitecture.Core.Helpers;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace CleanArchitecture.Server.Utilities
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class QueryParameterConstraintAttribute : Attribute, IActionConstraint
    {
        private readonly string _parameterName;
        private readonly object? _parameterValue;

        public QueryParameterConstraintAttribute(string parameterName, object? parameterValue = null)
        {
            _parameterName = parameterName;
            _parameterValue = parameterValue;
        }

        public bool Accept(ActionConstraintContext context)
        {

            if (context.RouteContext.HttpContext.Request.Query.TryGetValue(_parameterName, out var value) && _parameterValue != null)
            {
                if (TypeHelper.TryParseObject(value, _parameterValue.GetType(), out var parsedValue))
                {
                    return _parameterValue == parsedValue;
                }
            }

            return false;
        }

        public int Order { get; }
    }
}
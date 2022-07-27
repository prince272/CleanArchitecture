using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Helpers
{
    public class ExpressionHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var stack = new Stack<string>();
            
            Expression? expression1 = expression.Body;

            while (expression1 != null)
            {
                if (expression1.NodeType == ExpressionType.Call)
                {
                    var methodCallExpression = (MethodCallExpression)expression1;
                    if (IsSingleArgumentIndexer(methodCallExpression))
                    {
                        stack.Push(string.Empty);
                        expression1 = methodCallExpression.Object;
                    }
                    else break;
                }
                else if (expression1.NodeType == ExpressionType.ArrayIndex)
                {
                    var binaryExpression = (BinaryExpression)expression1;
                    stack.Push(string.Empty); expression1 = binaryExpression.Left;
                }
                else if (expression1.NodeType == ExpressionType.MemberAccess) { var memberExpression = (MemberExpression)expression1; stack.Push("." + memberExpression.Member.Name); expression1 = memberExpression.Expression; } else if (expression1.NodeType == ExpressionType.Parameter) { stack.Push(string.Empty); expression1 = null; } else if (expression1.NodeType == ExpressionType.Convert) { var memberExp = ((UnaryExpression)expression1).Operand as MemberExpression; stack.Push("." + memberExp.Member.Name); expression1 = memberExp.Expression; } else break;
            }

            if (stack.Count > 0 && string.Equals(stack.Peek(), ".model", StringComparison.OrdinalIgnoreCase))
                stack.Pop();

            if (stack.Count <= 0)
                return string.Empty;

            return (stack).Aggregate(((left, right) => left + right)).TrimStart(new[] { '.' });
        }

        private static bool IsSingleArgumentIndexer(Expression expression)
        {
            var methodExpression = expression as MethodCallExpression;
            if (methodExpression == null || methodExpression.Arguments.Count != 1)
                return false;

            return (methodExpression.Method.DeclaringType!.GetDefaultMembers()).OfType<PropertyInfo>().Any((p => p.GetGetMethod() == methodExpression.Method));
        }
    }
}
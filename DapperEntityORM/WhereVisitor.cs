using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;

namespace DapperEntityORM
{
    internal class WhereVisitor : ExpressionVisitor
    {
        private DynamicParameters _dynamicParameters = new DynamicParameters();
        private readonly List<string> _clauses;

        public string WhereClause => _clauses.Any() ? " WHERE " + string.Join(" AND ", _clauses) : "";
        public DynamicParameters DynamicParameters => _dynamicParameters;
        public WhereVisitor(DynamicParameters parameters)
        {
            _dynamicParameters = parameters;
            _clauses = new List<string>();
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = node.Left;
            var right = node.Right;

            if(left is MemberExpression leftMember && right is ConstantExpression rightConstant)
            {
                var paramName = "@" + leftMember.Member.Name;
                _dynamicParameters.Add(paramName, rightConstant.Value);

                _clauses.Add($"{leftMember.Member.Name} {GetOperator(node.NodeType)} {paramName}");
            }

            return base.VisitBinary(node);
        }

        private string GetOperator(ExpressionType nodeType)
        {
            if (nodeType == ExpressionType.AndAlso)
                return " AND ";
            else if (nodeType == ExpressionType.OrElse)
                return " OR ";
            else if (nodeType == ExpressionType.Equal)
                return " = ";
            else if (nodeType == ExpressionType.NotEqual)
                return " <> ";
            else if (nodeType == ExpressionType.GreaterThan)
                return " > ";
            else if (nodeType == ExpressionType.GreaterThanOrEqual)
                return " >= ";
            else if (nodeType == ExpressionType.LessThan)
                return " < ";
            else if (nodeType == ExpressionType.LessThanOrEqual)
                return " <= ";
            else
                throw new NotSupportedException($"The binary operator '{nodeType}' is not supported");

        }

        

    }
}

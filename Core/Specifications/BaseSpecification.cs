using Ecommerce.Core.Interfaces;
using System.Linq.Expressions;

namespace Ecommerce.Core.Specifications
{
    public class BaseSpecification<T>(Expression<Func<T, bool>>? _criteria) : ISpecification<T>
    {
        protected BaseSpecification(): this(null) { }
        public Expression<Func<T, bool>>? Criteria => _criteria;

        public Expression<Func<T, object>>? OrderBy { get; private set; }

        public Expression<Func<T, object>>? OrderByDescending { get; private set; }

        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }
    }
}

using Microsoft.EntityFrameworkCore.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ManaxTests.Mocks
{
    internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(inner.MoveNext());
        }

        public T Current => inner.Current;

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return new ValueTask();
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            Type resultType = typeof(TResult);

            if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(Task<>))
                return _inner.Execute<TResult>(expression);
            Type itemType = resultType.GetGenericArguments()[0];
                
            if (itemType.IsGenericType && 
                (itemType.GetGenericTypeDefinition() == typeof(List<>) ||
                 itemType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                MethodInfo? executeMethod = typeof(IQueryProvider)
                    .GetMethod(nameof(IQueryProvider.Execute))
                    ?.MakeGenericMethod(itemType);

                if (executeMethod == null) return _inner.Execute<TResult>(expression);
                object? result = executeMethod.Invoke(_inner, [expression]);
                object? listResult = typeof(Task).GetMethod(nameof(Task.FromResult))
                    ?.MakeGenericMethod(itemType)
                    .Invoke(null, [result]);
                            
                return (TResult)listResult!;
            }
            else
            {
                object? result = _inner.Execute(expression);
                object? taskResult = typeof(Task).GetMethod(nameof(Task.FromResult))
                    ?.MakeGenericMethod(itemType)
                    .Invoke(null, [result]);
                    
                return (TResult)taskResult!;
            }
        }
    }

    internal class TestAsyncEnumerable<T> : IAsyncEnumerable<T>, IQueryable<T>, IOrderedQueryable<T>
    {
        private readonly IQueryable<T> _inner;

        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : this(enumerable.AsQueryable()) { }

        public TestAsyncEnumerable(Expression expression)
        {
            _inner = new EnumerableQuery<T>(expression);
        }

        public TestAsyncEnumerable(IQueryable<T> inner)
        {
            _inner = inner;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_inner.ToList().GetEnumerator());
        }

        public IEnumerator<T> GetEnumerator() => _inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
        public Type ElementType => _inner.ElementType;
        public Expression Expression => _inner.Expression;
        public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);
    }
}

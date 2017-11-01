using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.QueryStack
{
    internal class QueryHandlerDelegateBuilder
    {
        #region From QueryHandler

        internal static QueryHandlerDelegate<TResult> FromQueryHandler<TQuery, TResult>(IQueryAsyncHandler<TQuery, TResult> queryAsyncHandler)
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>(async (q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    throw ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType());
                }

                return await queryAsyncHandler.HandleAsync(query, ct).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromQueryHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler)
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>((q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    return TaskUtility.FromException<TResult>(ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType()));
                }

                try
                {
                    TResult result = queryHandler.Handle(query);
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
                }
            });
        }

        #endregion From QueryHandler

        #region From Factory

        internal static QueryHandlerDelegate<TResult> FromFactory<TQuery, TResult>(Func<IQueryAsyncHandler<TQuery, TResult>> queryHandlerFactory)
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>(async (q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    throw ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType());
                }

                IQueryAsyncHandler<TQuery, TResult> instance;

                if (!TryRetrieveInstanceFromFactory(queryHandlerFactory, out instance))
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IQueryAsyncHandler<TQuery, TResult>>();
                }

                return await instance.HandleAsync(query, ct).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromFactory<TQuery, TResult>(Func<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>((q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    return TaskUtility.FromException<TResult>(ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType()));
                }
                
                IQueryHandler<TQuery, TResult> instance;

                if (!TryRetrieveInstanceFromFactory(queryHandlerFactory, out instance))
                {
                    return TaskUtility.FromException<TResult>(ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IQueryHandler<TQuery, TResult>>());
                }

                try
                {
                    TResult result = instance.Handle(query);
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
                }
            });
        }

        #endregion From Factory

        #region From Delegate

        internal static QueryHandlerDelegate<TResult> FromDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TQuery, Task<TResult>> asyncFunc)
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>(async (q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    throw ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType());
                }

                TAttributed instance;

                if (!TryRetrieveInstanceFromFactory(attributedObjectFactory, out instance))
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IQueryHandler<TQuery, TResult>>();
                }

                return await asyncFunc.Invoke(instance, query).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TQuery, CancellationToken, Task<TResult>> asyncCancellableFunc)
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>(async (q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    throw ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType());
                }

                TAttributed instance;

                if (!TryRetrieveInstanceFromFactory(attributedObjectFactory, out instance))
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IQueryHandler<TQuery, TResult>>();
                }

                return await asyncCancellableFunc.Invoke(instance, query, ct).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TQuery, TResult> func)
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>((q, ct) =>
            {
                TQuery query = q as TQuery;
                if (query == null)
                {
                    return TaskUtility.FromException<TResult>(ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), q.GetType()));
                }

                TAttributed instance;

                if (!TryRetrieveInstanceFromFactory(attributedObjectFactory, out instance))
                {
                    return TaskUtility.FromException<TResult>(ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<IQueryHandler<TQuery, TResult>>());
                }

                try
                {
                    TResult result = func.Invoke(instance, query);
                    return Task.FromResult(result);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
                }
            });
        }

        #endregion From Delegate

        #region Functions

        private static bool TryRetrieveInstanceFromFactory<TInstance>(Func<TInstance> factory, out TInstance instance)
        {
            instance = default(TInstance);

            try
            {
                instance = factory.Invoke();

                if (instance == null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        #endregion Functions
    }
}
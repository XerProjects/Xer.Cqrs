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
            return new QueryHandlerDelegate<TResult>(async (inputQuery, ct) =>
            {
                TQuery query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                return await queryAsyncHandler.HandleAsync(query, ct).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromQueryHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler)
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>((inputQuery, ct) =>
            {
                TQuery query;
                try
                {
                    query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
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
            return new QueryHandlerDelegate<TResult>(async (inputQuery, ct) =>
            {
                TQuery query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                IQueryAsyncHandler<TQuery, TResult> instance = EnsureInstanceFromFactory(queryHandlerFactory);

                return await instance.HandleAsync(query, ct).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromFactory<TQuery, TResult>(Func<IQueryHandler<TQuery, TResult>> queryHandlerFactory)
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>((inputQuery, ct) =>
            {
                TQuery query;
                try
                {
                    query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
                }

                IQueryHandler<TQuery, TResult> instance;
                try
                {
                    instance = EnsureInstanceFromFactory(queryHandlerFactory);
                }
                catch (Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
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
            return new QueryHandlerDelegate<TResult>(async (inputQuery, ct) =>
            {
                TQuery query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                return await asyncFunc.Invoke(instance, query).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TQuery, CancellationToken, Task<TResult>> asyncCancellableFunc)
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>(async (inputQuery, ct) =>
            {
                TQuery query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                TAttributed instance = EnsureInstanceFromFactory(attributedObjectFactory);
                
                return await asyncCancellableFunc.Invoke(instance, query, ct).ConfigureAwait(false);
            });
        }

        internal static QueryHandlerDelegate<TResult> FromDelegate<TAttributed, TQuery, TResult>(Func<TAttributed> attributedObjectFactory, Func<TAttributed, TQuery, TResult> func)
            where TAttributed : class
            where TQuery : class, IQuery<TResult>
        {
            return new QueryHandlerDelegate<TResult>((inputQuery, ct) =>
            {
                TQuery query;
                try
                {
                    query = EnsureValidQuery<TQuery, TResult>(inputQuery);
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
                }

                TAttributed instance;
                try
                {
                    instance = EnsureInstanceFromFactory(attributedObjectFactory);
                }
                catch(Exception ex)
                {
                    return TaskUtility.FromException<TResult>(ex);
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

        private static TInstance EnsureInstanceFromFactory<TInstance>(Func<TInstance> factory)
        {
            try
            {
                TInstance instance = factory.Invoke();

                if (instance == null)
                {
                    throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>();
                }

                return instance;
            }
            catch (Exception ex)
            {
                throw ExceptionBuilder.FailedToRetrieveInstanceFromFactoryDelegateException<TInstance>(ex);
            }
        }

        private static TQuery EnsureValidQuery<TQuery, TResult>(IQuery<TResult> inputQuery) where TQuery : class, IQuery<TResult>
        {
            if (inputQuery == null)
            {
                throw new ArgumentNullException(nameof(inputQuery));
            }

            TQuery query = inputQuery as TQuery;
            if (query == null)
            {
                throw ExceptionBuilder.InvalidQueryTypeArgumentException(typeof(TQuery), inputQuery.GetType());
            }

            return query;
        }

        #endregion Functions
    }
}
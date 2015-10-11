﻿using System;
using System.Linq;
using System.Linq.Expressions;
using AccountAtAGlance.Model;
using Microsoft.Data.Entity;
using Microsoft.Framework.DependencyInjection;
using AccountAtAGlance.Repository.Interfaces;

namespace AccountAtAGlance.Repository
{
    public abstract class RepositoryBase<TContext> : IDisposable
        where TContext : DbContext, IDisposedTracker, new()
    {
        TContext _DataContext;
        IServiceProvider _ServiceProvider; 

        public RepositoryBase(TContext context, IServiceProvider serviceProvider)
        {
            DataContext = context;
            _ServiceProvider = serviceProvider;
        }

        public TContext DataContext { get; set; }

        //protected virtual TContext DataContext
        //{
        //    get
        //    {
        //        if (_DataContext == null || _DataContext.IsDisposed)
        //        {
        //            _DataContext = _ServiceProvider.GetService<TContext>();
        //        }
        //        return _DataContext;
        //    }
        //}

        protected virtual T Get<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            if (predicate != null)
            {
                using (DataContext)
                {
                    return DataContext.Set<T>().Where(predicate).SingleOrDefault();
                }
            }
            else
            {
                throw new Exception("Predicate value must be passed to Get<T>.");
            }
        }

        protected virtual IQueryable<T> GetList<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                var coll = DataContext.Set<T>();
                if (predicate != null)
                {
                    return coll.Where(predicate);
                }
                return coll;

            }
            catch (Exception ex)
            {
                //Log error
            }
            return null;
        }

        protected virtual IQueryable<T> GetList<T, TKey>(Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy) where T : class
        {
            try
            {
                return GetList(predicate).OrderBy(orderBy);
            }
            catch (Exception ex)
            {
                //Log error
            }
            return null;
        }

        protected virtual IQueryable<T> GetList<T, TKey>(Expression<Func<T, TKey>> orderBy) where T : class
        {
            try
            {
                return GetList<T>().OrderBy(orderBy);
            }
            catch (Exception ex)
            {
                //Log error
            }
            return null;
        }

        protected virtual IQueryable<T> GetList<T>() where T : class
        {
            try
            {
                return DataContext.Set<T>();
            }
            catch (Exception ex)
            {
                //Log error
            }
            return null;
        }

        protected OperationStatus ExecuteStoreCommand(string cmdText, params object[] parameters)
        {
            var opStatus = new OperationStatus { Status = true };

            try
            {
                DataContext.Database.ExecuteSqlCommand(cmdText, parameters);
            }
            catch (Exception exp)
            {
                OperationStatus.CreateFromException("Error executing store command: ", exp);
            }
            return opStatus;
        }

        protected virtual OperationStatus Save<T>(T entity) where T : class
        {
            OperationStatus opStatus = new OperationStatus { Status = true };

            try
            {
                //Custom attaching/adding of entity could be done here
                opStatus.Status = DataContext.SaveChanges() > 0;
            }
            catch (Exception exp)
            {
                opStatus = OperationStatus.CreateFromException("Error saving " + typeof(T) + ".", exp);
            }

            return opStatus;
        }

        public virtual void Dispose()
        {
            if (DataContext != null) DataContext.Dispose();
        }
    }
}
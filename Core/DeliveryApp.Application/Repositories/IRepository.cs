﻿using DeliveryApp.Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryApp.Application.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        DbSet<T> Table { get; }

        IQueryable<T> GetAll(bool tracking = true);
        IQueryable<T> GetWhere(Expression<Func<T, bool>> method, bool tracking = true);
        Task<T> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true);
        Task<T> GetByIdAsync(int id, bool tracking = true);

        Task<bool> AddAsync(T model);
        //Task<bool> AddRangeAsync(List<T> datas);
        bool Remove(T model);
        //bool RemoveRange(List<T> datas);
        Task<bool> RemoveAsync(int id);
        bool Update(T model);

        Task<bool> SaveAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.IRepositories;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly TalabatContext _context;

        public GenericRepository(TalabatContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        => await _context.Set<T>().ToListAsync();

        public async Task<T> GetByIdAsync(int id)
        => await _context.Set<T>().FindAsync(id);

    }
}

using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public class DataAccessBase<T> where T : class {
        private readonly PlatformAntiquesHandicraftsContext _context;
        private readonly DbSet<T> _dbSet;
        public DataAccessBase(PlatformAntiquesHandicraftsContext context) {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public IQueryable<T> GetAll() {

            return _dbSet;
        }

        public void Create(T entity) {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }
        public void Delete(T entity) {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }
        public void Update(T entity) {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            //_dbSet.Update(entity);
            _context.SaveChanges();
        }
    }
}

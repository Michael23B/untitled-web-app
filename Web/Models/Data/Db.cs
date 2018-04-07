using System.Data.Entity;

namespace Web.Models.Data
{
    public class Db : DbContext
    {
        public DbSet<PageDTO> Pages { get; set; }
    }
}
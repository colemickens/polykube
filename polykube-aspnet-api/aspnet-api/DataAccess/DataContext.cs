using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.DataAccess
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<GuestbookEntryModel> GuestbookEntries { get; set; }
    }
}

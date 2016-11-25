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
            Console.WriteLine("new data context");
        }

        public DbSet<GuestbookEntryModel> GuestbookEntries { get; set; }
    }
}

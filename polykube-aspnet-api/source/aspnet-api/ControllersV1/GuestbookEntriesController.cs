using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Api.ContractsV1;
using Api.DataAccess;
using Api.Models;

namespace Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("/guestbookentries")]
    public class GuestbookEntryController
    {
        private DataContext dataContext;
        private ILogger logger;

        public GuestbookEntryController(ILoggerFactory loggerFactory, DataContext dataContext)
        {
            this.logger = loggerFactory.CreateLogger(nameof(GuestbookEntryController));
            this.dataContext = dataContext;
        }

        [HttpGet()]
        public async Task<List<GuestbookEntryContract>> List() {
            var dbEntries = this.dataContext.GuestbookEntries
                .OrderBy(e => e.Timestamp)
                .Take(100);

            var entries = await dbEntries.Select(e => new GuestbookEntryContract{
                Id = e.Id,
                Author = e.Author,
                Timestamp = e.Timestamp,
                Message = e.Message,
            }).ToListAsync();

            return entries;
        }

        [HttpPost]
        public void Post([FromBody] GuestbookEntryContract entry)
        {
            var dbEntry = new GuestbookEntryModel{
                Author = entry.Author,
                Timestamp = DateTime.UtcNow,
                Message = entry.Message,
            };

            this.dataContext.Add(dbEntry);
            this.dataContext.SaveChanges();

            // if I do this and make it async, I get blowups:
            // await this.dataContext.SaveChangesAsync();
        }
    }
}

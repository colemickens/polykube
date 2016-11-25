using System;
using System.Collections;
using Microsoft.Extensions.PlatformAbstractions;

namespace Api.ContractsV1
{
    public class GuestbookEntryContract
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string Author { get; set; }

        public string Message { get; set; }
    }
}

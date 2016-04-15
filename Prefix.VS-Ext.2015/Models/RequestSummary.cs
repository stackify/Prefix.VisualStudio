using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace Prefix.VSExt2015.Models
{
    public class RequestSummary
    {
        public string ReportingUrl { get; set; }
        public string ID { get; set; }
        public DateTime? Found { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public int TookMs { get; set; }
        public short? Status { get; set; }
        public string RelativeUrl { get; set; }
        public string AppName { get; set; }

        public List<Item> Items = new List<Item>();

        public Item Slowest { get; set; }
        public long DBCalls { get; set; }
        public long WSCalls { get; set; }
        public long CacheCalls { get; set; }
        public long Errors { get; set; }

        public string RawUrl { get; set; }
        public string UrlKey { get; set; }
        public string HttpMethod { get; set; }


        //public short ServiceCalls { get; set; }

        public class Item
        {
            public string ItemType { get; set; }
            public string Name { get; set; }
            public decimal TookMs { get; set; }
            public decimal Percentage { get; set; }
        }
    }
}

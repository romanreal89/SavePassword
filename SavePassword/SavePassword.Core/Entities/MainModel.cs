using System;
using System.Collections.Generic;

namespace SavePassword.Core.Entities
{
    public class MainModel
    {
        public List<PassRecord> PassRecords { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}

using System;

namespace Yumaster.File.Data
{
    public partial class QyCode
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IsUsed { get; set; }
        public long ID { get; set; }
        public string secret { get; set; }
        public string space { get; set; }
        public DateTime? updatetime { get; set; }
        public DateTime? crdate { get; set; }
        public long? filecount { get; set; }
        public string yyspace { get; set; }
    }
}

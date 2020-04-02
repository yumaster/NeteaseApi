using System;
namespace Yumaster.File.Data
{
    public partial class File_Downhistory
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public int? RefID { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}

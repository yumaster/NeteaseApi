using System;

namespace Yumaster.File.Data
{
    public partial class File_UserAuth
    {
        public int? ComId { get; set; }
        public int ID { get; set; }
        public string AuthType { get; set; }
        public string AuthUser { get; set; }
        public int? RefID { get; set; }
        public string RefType { get; set; }
        public string CRUser { get; set; }
        public DateTime? CRDate { get; set; }
    }
}

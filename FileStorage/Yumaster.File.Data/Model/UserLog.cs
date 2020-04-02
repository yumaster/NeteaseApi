namespace Yumaster.File.Data
{
    public partial class UserLog
    {
        public long ID { get; set; }
        public string loginfo { get; set; }
        public string ip { get; set; }
        public string useraction { get; set; }
        public string remark { get; set; }
    }
}

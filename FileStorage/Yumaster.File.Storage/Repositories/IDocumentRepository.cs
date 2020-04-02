using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yumaster.File.Data;

namespace Yumaster.File.Storage.Repositories
{
    public interface IDocumentRepository
    {
        bool Exist(string qycode, string md5);
        Document SaveDocument(string md5, string qycode, string name, string extension, string contentType, string filePath, DateTime realDate, string strfileinfo);
    }
}

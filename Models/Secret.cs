using Aiursoft.Pylon.Models.OSS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Aiursoft.OSS.Models
{
    public class Secret
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public int FileId { get; set; }
        [ForeignKey(nameof(FileId))]
        public OSSFile File { get; set; }

        public bool Used { get; set; }
        public string UseTime { get; set; }
        public string UserIpAddress { get; set; }
    }
}

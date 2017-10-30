using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Aiursoft.OSS.Models.DownloadAddressModels
{
    public class DownloadFileAddressModel
    {
        public string BucketName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string sd { get; set; } = string.Empty;
        [Range(-1, 10000)]
        public int w { get; set; } = -1;
        [Range(-1, 10000)]
        public int h { get; set; } = -1;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sude.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string DeviceType { get; set; }
        public string ProjectFileName { get; set; }
        public string VideoFileName { get; set; }
        public byte[] ProjectFileData { get; set; }
        public byte[] VideoFileData { get; set; }
        public byte[] MainImageFileData { get; set; }
    }
    public class DeviceAsset
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int StepOrder { get; set; }
        public string ContentType { get; set; }
        public string ContentText { get; set; }
        public byte[] ContentData { get; set; }
    }
}

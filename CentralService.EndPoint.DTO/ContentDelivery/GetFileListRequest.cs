using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.ContentDelivery
{
    public struct GetFileListRequest
    {
        public string GameCode { get; set; }
        public int FileListOffset { get; set; }
        public int MaxFileListEntryCount { get; set; }
        public List<string> Attributes { get; set; }

        public GetFileListRequest(string GameCode, int FileListOffset, int MaxFileListEntryCount, List<string> Attributes)
        {
            this.GameCode = GameCode;
            this.FileListOffset = FileListOffset;
            this.MaxFileListEntryCount = MaxFileListEntryCount;
            this.Attributes = Attributes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPlayer.Models
{
    public class FileMedia : MediaData
    {
        public string Path { get; set; }

        public override Uri Uri => new Uri(Path);
    }
}

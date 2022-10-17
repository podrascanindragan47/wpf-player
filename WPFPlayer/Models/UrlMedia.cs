using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPlayer.Models
{
    public class UrlMedia : MediaData
    {
        public string URL { get; set; }

        public override Uri Uri => new Uri(URL);
    }
}

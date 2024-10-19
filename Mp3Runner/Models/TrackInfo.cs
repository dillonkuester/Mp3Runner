using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mp3Runner.Models
{
    public class TrackInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public AlbumInfo Album { get; set; }
        public ArtistInfo[] Artists { get; set; }
        public int DurationMs { get; set; }
        public float Tempo { get; set; }  // BPM
        public int Key { get; set; } // Key (0-11)
        public int Mode { get; set; }     // Mode (0 = Minor, 1 = Major)
        public string KeyString { get; set; } // Property for the key representation
    }
}

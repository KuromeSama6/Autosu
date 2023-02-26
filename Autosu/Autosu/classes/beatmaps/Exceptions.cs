using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autosu.Exceptions {
    // No beatmap is found when calling Beatmap.GetOne()
    public class BeatmapNotFoundException : Exception { }
    public class NothingToInheritException : Exception { 
        public NothingToInheritException(string msg) : base(msg) { }
    }
}

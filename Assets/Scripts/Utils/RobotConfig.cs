using System;
using System.Collections.Generic;


namespace DataObjects
{
    public class RobotConfig
    {
        public List<Dictionary<string, object>> bodies { get; set; }
        public List<Dictionary<string, object>> plugins { get; set; }
    }
}
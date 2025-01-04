using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VollandAPI
{
    public enum Request_Type
    {
        exposure_request = 0,
        trend_request = 1,
        paradigm_request = 2,
        zerodte_request = 3
    }

    public enum Greek
    {
        delta = 0,
        gamma = 1,
        charm = 2,
        vanna = 3,
        vega = 4,
        theta = 5
    }

    public enum Kind
    {
        put = -1,
        both = 0,        
        call = 1
    }
}

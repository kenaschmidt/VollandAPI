using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VollandAPI
{
    public enum Request_Type
    {
        None = 0,
        exposure_request = 1,
        trend_request = 2,
        paradigm_request = 3,
        zerodte_request = 4
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

    public enum Paradigm
    {
        None = 0,
        [Description("GEX-PURE")]
        GEX_Pure = 1,
        [Description("GEX-TARGET")]
        GEX_Target = 2,
        [Description("SIDIAL-MESSY")]
        SIDIAL_Messy = 3
    }
}

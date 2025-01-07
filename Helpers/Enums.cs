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
        [Description("UNDEFINED")]
        UNDEFINED = 0,

        [Description("GEX-LIS")]
        GEX_LIS = 1,

        [Description("AG-LIS")]
        AG_LIS = 2,

        [Description("BOFA-PURE")]
        BOFA_PURE = 3,

        [Description("BOFA-MESSY")]
        BOFA_MESSY = 4,

        [Description("GEX-TARGET")]
        GEX_TARGET = 5,

        [Description("AG-TARGET")]
        AG_TARGET = 6,

        [Description("SIDIAL-BALANCE")]
        SIDIAL_BALANCE = 7,

        [Description("SIDIAL-MESSY")]
        SIDIAL_MESSY = 8,

        [Description("SIDIAL-EXTREME")]
        SIDIAL_EXTREME = 9,

        [Description("BOFA-LIS")]
        BOFA_LIS = 10,

        [Description("GEX-PURE")]
        GEX_PURE = 11,

        [Description("GEX-MESSY")]
        GEX_MESSY = 12,

        [Description("AG-PURE")]
        AG_PURE = 13,

        [Description("AG-MESSY")]
        AG_MESSY = 14
    }
}

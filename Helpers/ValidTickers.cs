﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VollandAPI
{
    internal static class ValidTickers
    {
        internal static bool ValidateTicker(string ticker)
        {
            return validTickers.Contains(ticker.ToUpper());
        }

        internal static List<string> validTickers = new List<string>()
        {
            "AAL",
            "AAPL",
            "ABBV",
            "ABNB",
            "ACN",
            "ADBE",
            "AFRM",
            "AMAT",
            "AMC",
            "AMD",
            "AMR",
            "AMZN",
            "ARKK",
            "ARM",
            "AVGO",
            "BA",
            "BABA",
            "BAC",
            "BB",
            "BBY",
            "BCS",
            "BIDU",
            "BILI",
            "BITO",
            "BKNG",
            "BMY",
            "BP",
            "BRCC",
            "BYND",
            "C",
            "CAN",
            "CARS",
            "CAT",
            "CCJ",
            "CCL",
            "CEI",
            "CGC",
            "CHWY",
            "CLF",
            "CLOV",
            "CMCSA",
            "CMG",
            "COIN",
            "COP",
            "COST",
            "CPNG",
            "CRM",
            "CRWD",
            "CSCO",
            "CVNA",
            "CVS",
            "CVX",
            "DAL",
            "DE",
            "DELL",
            "DG",
            "DIA",
            "DIS",
            "DJT",
            "DKNG",
            "DLTR",
            "DOCU",
            "DVN",
            "DWAC",
            "DXC",
            "EEM",
            "EFA",
            "EL",
            "ENPH",
            "ET",
            "EWZ",
            "F",
            "FCEL",
            "FCX",
            "FDX",
            "FSR",
            "FUBO",
            "FUTU",
            "FXI",
            "GDX",
            "GE",
            "GIS",
            "GLD",
            "GM",
            "GME",
            "GOLD",
            "GOOG",
            "GOOGL",
            "HD",
            "HOOD",
            "HTZ",
            "HUM",
            "HUT",
            "HYG",
            "HYMC",
            "IBM",
            "INTC",
            "IQ",
            "IWM",
            "JD",
            "JETS",
            "JNJ",
            "JPM",
            "KDP",
            "KGC",
            "KO",
            "KR",
            "KRE",
            "KSS",
            "KWEB",
            "LABU",
            "LAC",
            "LAZR",
            "LCID",
            "LI",
            "LLY",
            "LMT",
            "LOW",
            "LQD",
            "LRCX",
            "LULU",
            "M",
            "MA",
            "MARA",
            "MCD",
            "MELI",
            "META",
            "MMM",
            "MO",
            "MOS",
            "MRK",
            "MRNA",
            "MRO",
            "MRVL",
            "MS",
            "MSFT",
            "MSTR",
            "MU",
            "MULN",
            "NCLH",
            "NEE",
            "NET",
            "NFLX",
            "NIO",
            "NKE",
            "NOK",
            "NOW",
            "NVDA",
            "OKTA",
            "OPEN",
            "ORCL",
            "OXY",
            "PANW",
            "PARA",
            "PBR",
            "PDD",
            "PEP",
            "PFE",
            "PG",
            "PINS",
            "PLTR",
            "PLUG",
            "PTON",
            "PYPL",
            "QCOM",
            "QQQ",
            "QS",
            "RBLX",
            "RDDT",
            "RICK",
            "RIOT",
            "RIVN",
            "ROKU",
            "RTX",
            "SBUX",
            "SE",
            "SHOP",
            "SLB",
            "SLV",
            "SMCI",
            "SMH",
            "SNAP",
            "SNDL",
            "SNOW",
            "SOFI",
            "SOXL",
            "SPCE",
            "SPX",
            "SPXU",
            "SPY",
            "SQ",
            "SQQQ",
            "SRPT",
            "T",
            "TBT",
            "TGT",
            "TIGR",
            "TLRY",
            "TLT",
            "TME",
            "TQQQ",
            "TSEM",
            "TSLA",
            "TSM",
            "TTD",
            "TWLO",
            "UAL",
            "UBER",
            "ULTA",
            "UNG",
            "UNH",
            "UPS",
            "UPST",
            "USB",
            "USO",
            "UVXY",
            "V",
            "VALE",
            "VIX",
            "VLY",
            "VXX",
            "VZ",
            "W",
            "WBA",
            "WFC",
            "WISH",
            "WMT",
            "WYNN",
            "X",
            "XBI",
            "XLE",
            "XLF",
            "XLI",
            "XLK",
            "XLU",
            "XLV",
            "XOM",
            "XOP",
            "XPEV",
            "YINN",
            "Z",
            "ZIM",
            "ZM",
            "ZS",
            "ZTO"
        };
    }
}

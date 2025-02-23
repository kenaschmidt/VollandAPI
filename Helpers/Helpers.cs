using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VollandAPI.Helpers
{
    internal static class Helpers
    {

        internal static Trend_Point ToTrendPoint(this Trend_Data_Point? me)
        {

            if (me == null)
                throw new Exception("Attempted to convert null trendDataPoint");

            string x = me.x ?? throw new ArgumentException("X is null");
            double y = me.y ?? throw new ArgumentException("Y is null");

            var dt = DateTime.ParseExact(x, "yyyy-MM-dd", null);

            return new Trend_Point(dt, y);
        }

        internal static List<Exposure_Point> ToExposurePoints(this Exposure_Data? me)
        {
            if (me != null)
            {
                if ((me.strikes?.Length ?? 0) != (me.exposures?.Length ?? 0))
                    throw new Exception("API ERROR: Exposure arrays are not equal lengths");

                var ret = new List<Exposure_Point>();

                if ((me.strikes?.Length ?? 0) == 0)
                {
                    // We've received an empty data array, the expiry doesn't exist in Volland
                    ret.Add(new Exposure_Point(0, 0));
                }
                else
                {
                    for (int i = 0; i < me.strikes?.Length; i++)
                    {
                        var strike = double.Parse(me.strikes[i]);
                        var exposure = me.exposures[i];

                        ret.Add(new Exposure_Point(strike, exposure));
                    }
                }

                return ret;
            }
            else
                throw new ArgumentNullException(nameof(me));
        }

        internal static string ToJsonString<T>(this T me)
        {
            return JsonSerializer.Serialize(me);
        }


        /// <summary>
        /// Converts a string to the enum with matching description tag
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="me"></param>
        internal static TEnum? ToEnumByDescription<TEnum>(this string me) where TEnum : Enum
        {
            TEnum? ret = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().FirstOrDefault(x => x.GetDescription() == me);

            if (ret == null)
                return (TEnum?)Activator.CreateInstance(typeof(TEnum));
            else return ret;
        }

        internal static string? GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string? name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo? field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute? attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a formatted string showing the earliest and latest dates in a list of datetimes
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        internal static string? DateRangeString(this List<DateTime> me)
        {
            if (me.Count == 0)
                return null;

            if (me.Count == 1)
                return me.Single().ToString("yyyy-MM-dd");

            return $"{me.Min().ToString("yyyy-MM-dd")} to {me.Max().ToString("yyyy-MM-dd")}";

        }


        #region Response to Result converters

        internal static TResult ToResult<TResult>(this Response me) where TResult : class
        {
            if (me is Exposure_Response e)
                return e.ToResult() as TResult ?? throw new NullReferenceException();
            if (me is Trend_Response t)
                return t.ToResult() as TResult ?? throw new NullReferenceException();
            if (me is Paradigm_Response p)
                return p.ToResult() as TResult ?? throw new NullReferenceException();
            if (me is ZeroDTE_Response z)
                return z.ToResult() as TResult ?? throw new NullReferenceException();

            throw new Exception("ERROR: Conversion error in ToResult");

        }

        private static Exposure_Result ToResult(this Exposure_Response me)
        {
            if (me.ticker == null ||
                me.greek == null ||
                me.kind == null ||
                me.expirations == null ||
                me.data == null ||
                me.data.currentPrice == null)
                throw new NullReferenceException("CONVERTER ERROR: Exposure_Response contained null fields");

            return new Exposure_Result(me.ticker, me.greek, me.kind, me.expirations, me.data, me.data.currentPrice.Value);
        }

        private static Trend_Result ToResult(this Trend_Response me)
        {
            if (me.ticker == null ||
                me.greek == null ||
                me.data == null)
                throw new NullReferenceException("CONVERTER ERROR: Trend_Response contained null fields");

            return new Trend_Result(
                me.ticker,
                me.greek,
                me.data);
        }

        private static Paradigm_Result ToResult(this Paradigm_Response me)
        {
            if (me.ticker == null ||
                me.data == null ||
                me.data.lastModified == null ||
                me.data.paradigm == null)
                throw new NullReferenceException("CONVERTER ERROR: Paradigm_Response contained null fields");

            return new Paradigm_Result(
                me.ticker,
                me.data.paradigm,
                me.data.target,
                me.data.lis,
                me.data.lastModified);
        }

        private static ZeroDTE_Result ToResult(this ZeroDTE_Response me)
        {
            if (me.ticker == null ||
                me.data == null)
                throw new NullReferenceException("CONVERTER ERROR: ZeroDTE_Response contained null fields");

            return new ZeroDTE_Result(
                me.ticker,
                me.data.dealer_premium,
                me.data.option_volume,
                me.data.zerodte_agg_charm);
        }

        #endregion

    }

}

using System;
using System.Linq;
using System.Text;

namespace Observable.Tools
{
    /// <summary>
    /// Defines the metric unit measure.
    /// </summary>
    public enum Units
    {
        Auto,
        Nanos,
        Micros,
        Millis,
        Secs,
        Mins,
        Hours,
    }

    /// <summary>
    /// Measure metrics 
    /// </summary>
    public class Metrics
    {
        private readonly double min;
        private readonly double mean;
        private readonly double median;
        private readonly double max;
        private readonly double[] measures;
        private readonly Units units;
        private readonly string name;
        private readonly Metrics[] underlyings;

        /// <summary>
        /// Gets name.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Gets the min measured value.
        /// </summary>
        public double Min
        {
            get { return min; }
        }

        /// <summary>
        /// Gets the mean measured value.
        /// </summary>
        public double Mean
        {
            get { return mean; }
        }

        /// <summary>
        /// Gets the median measured value.
        /// </summary>
        public double Median
        {
            get { return median; }
        }

        /// <summary>
        /// Gets the max measured value.
        /// </summary>
        public double Max
        {
            get { return max; }
        }

        /// <summary>
        /// Gets the units of measured value.
        /// </summary>
        public Units Units
        {
            get { return units; }
        }

        /// <summary>
        /// Gets nderlyings metrics
        /// </summary>
        public Metrics[] Underlyings { get { return underlyings; } }

        /// <summary>
        /// Gets all measures.
        /// </summary>
        public double[] Measures { get { return measures; } }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="measures">All measures in milliseconds.</param>
        /// <param name="units">Measure unit to compute.</param>
        /// <param name="name">Metrics name</param>
        /// <param name="underlyings">Underlying metrics</param>
        public Metrics(double[] measures, Units units, string name = null, params Metrics[] underlyings)
        {
            var ordered = measures.OrderBy(p => p).ToArray();
            if (ordered.Length == 0) return;

            var length = ordered.Length;
            min = ordered[0];
            max = ordered[length - 1];

            var sum = 0.0;
            var medianIdx = length / 2;
            for (var i = 0; i < length; i++)
            {
                min = Math.Min(min, measures[i]);
                max = Math.Max(max, measures[i]);
                sum += measures[i];
                if (i == medianIdx)
                    median = measures[i];
            }
            mean = sum / length;

            if (units == Units.Auto)
                units = GetUnits(mean);
            var factor = GetUnitFactor(units);

            min *= factor;
            mean *= factor;
            median *= factor;
            max *= factor;

            this.measures = measures;
            this.units = units;
            this.name = name;
            this.underlyings = underlyings;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            ToStringDeep(sb, this);
            return sb.ToString();
        }

        private static void ToStringDeep(StringBuilder sb, Metrics metrics, string prefix = null)
        {
            sb.AppendFormat("{0}{1} Min: {2}, Mean: {3}, Median: {4}, Max: {5}, Units: {6}", prefix, metrics.name, metrics.min, metrics.mean, metrics.median, metrics.max, metrics.units);
            if (metrics.underlyings == null) return;

            var subPrefix = (prefix ?? string.Empty) + "\t";
            foreach (var underlying in metrics.underlyings)
            {
                sb.AppendLine();
                ToStringDeep(sb, underlying, subPrefix);
            }
        }

        private static Units GetUnits(double ms)
        {
            if (ms < 1e-3) return Units.Nanos;
            if (ms < 1) return Units.Micros;
            if (ms < 1e3) return Units.Millis;
            if (ms < 1e3 * 60) return Units.Secs;
            return ms < 1e3 * 60 * 60 ? Units.Mins : Units.Hours;
        }

        private static double GetUnitFactor(Units units)
        {
            switch (units)
            {
                case Units.Nanos:
                    return 1e6;
                case Units.Micros:
                    return 1e3;
                case Units.Secs:
                    return 1e-3;
                case Units.Mins:
                    return 6e-4;
                case Units.Hours:
                    return 36e-5;
                default:
                    return 1;
            }
        }

    }
}

using System;
using System.Collections.Generic;

namespace Dqe.Web.Controllers
{
    /// <summary>
    /// Provides NHCCI index values for each quarter, and helper to get quarter keys.
    /// </summary>
    public static class NHCCIData
    {
        // Dictionary mapping "Year Q#" to NHCCI index. 
        // Covers from 2003 Q1 (base 1.000) up to 2024 Q4 (latest available).
        public static readonly Dictionary<string, decimal> IndexByQuarter = new Dictionary<string, decimal>
        {
            { "2003 Q1", 1m },
            { "2003 Q2", 1.009624m },
            { "2003 Q3", 1.02399m },
            { "2003 Q4", 1.021636m },
            { "2004 Q1", 1.045945m },
            { "2004 Q2", 1.100941m },
            { "2004 Q3", 1.143055m },
            { "2004 Q4", 1.149222m },
            { "2005 Q1", 1.240895m },
            { "2005 Q2", 1.281448m },
            { "2005 Q3", 1.371839m },
            { "2005 Q4", 1.412496m },
            { "2006 Q1", 1.448616m },
            { "2006 Q2", 1.52135m },
            { "2006 Q3", 1.618392m },
            { "2006 Q4", 1.552662m },
            { "2007 Q1", 1.563617m },
            { "2007 Q2", 1.561181m },
            { "2007 Q3", 1.53754m },
            { "2007 Q4", 1.514275m },
            { "2008 Q1", 1.568627m },
            { "2008 Q2", 1.644059m },
            { "2008 Q3", 1.784774m },
            { "2008 Q4", 1.626675m },
            { "2009 Q1", 1.49997m },
            { "2009 Q2", 1.439771m },
            { "2009 Q3", 1.429229m },
            { "2009 Q4", 1.402628m },
            { "2010 Q1", 1.441913m },
            { "2010 Q2", 1.438427m },
            { "2010 Q3", 1.446513m },
            { "2010 Q4", 1.429966m },
            { "2011 Q1", 1.456828m },
            { "2011 Q2", 1.500597m },
            { "2011 Q3", 1.541206m },
            { "2011 Q4", 1.541088m },
            { "2012 Q1", 1.576907m },
            { "2012 Q2", 1.626998m },
            { "2012 Q3", 1.595538m },
            { "2012 Q4", 1.607117m },
            { "2013 Q1", 1.59082m },
            { "2013 Q2", 1.623509m },
            { "2013 Q3", 1.644783m },
            { "2013 Q4", 1.59307m },
            { "2014 Q1", 1.627779m },
            { "2014 Q2", 1.669892m },
            { "2014 Q3", 1.735101m },
            { "2014 Q4", 1.693793m },
            { "2015 Q1", 1.719765m },
            { "2015 Q2", 1.704847m },
            { "2015 Q3", 1.706301m },
            { "2015 Q4", 1.662658m },
            { "2016 Q1", 1.631144m },
            { "2016 Q2", 1.677906m },
            { "2016 Q3", 1.679756m },
            { "2016 Q4", 1.653447m },
            { "2017 Q1", 1.617234m },
            { "2017 Q2", 1.684628m },
            { "2017 Q3", 1.734304m },
            { "2017 Q4", 1.661883m },
            { "2018 Q1", 1.674676m },
            { "2018 Q2", 1.752068m },
            { "2018 Q3", 1.844641m },
            { "2018 Q4", 1.873043m },
            { "2019 Q1", 1.849188m },
            { "2019 Q2", 1.954851097m },
            { "2019 Q3", 1.971934754m },
            { "2019 Q4", 1.92261743m },
            { "2020 Q1", 1.968679539m },
            { "2020 Q2", 1.96474271m },
            { "2020 Q3", 1.889677421m },
            { "2020 Q4", 1.860110208m },
            { "2021 Q1", 1.91116954m },
            { "2021 Q2", 2.036274797m },
            { "2021 Q3", 2.107518832m },
            { "2021 Q4", 2.18210826m },
            { "2022 Q1", 2.284086099m },
            { "2022 Q2", 2.555465592m },
            { "2022 Q3", 2.78198294m },
            { "2022 Q4", 2.783970683m },
            { "2023 Q1", 2.84260608m },
            { "2023 Q2", 2.968720527m },
            { "2023 Q3", 3.129812483m },
            { "2023 Q4", 3.115808219m },
            { "2024 Q1", 3.191321352m },
            { "2024 Q2", 3.16074104m },
            { "2024 Q3", 3.360492989m },
            { "2024 Q4", 3.232905682m }
        };

        /// <summary>
        /// Given a date, returns the quarter key in "YYYY Q#" format.
        /// </summary>
        public static string GetQuarterKey(DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;  // Determine quarter from month
            return $"{date.Year} Q{quarter}";
        }

        /// <summary>
        /// Gets the latest available NHCCI index value.
        /// </summary>
        public static decimal GetLatestIndex()
        {
            return IndexByQuarter["2024 Q4"]; // Latest available as of 2024 Q4
        }

        /// <summary>
        /// Calculates the inflation-adjusted price using NHCCI index.
        /// </summary>
        /// <param name="originalPrice">The original bid price</param>
        /// <param name="lettingDate">The letting date of the bid</param>
        /// <returns>The inflation-adjusted price in today's terms</returns>
        public static decimal CalculateInflationAdjustedPrice(decimal originalPrice, DateTime lettingDate)
        {
            try
            {
                string quarterKey = GetQuarterKey(lettingDate);
                
                if (!IndexByQuarter.ContainsKey(quarterKey))
                {
                    // If quarter not found, return original price
                    return originalPrice;
                }

                decimal lettingDateIndex = IndexByQuarter[quarterKey];
                decimal latestIndex = GetLatestIndex();
                
                // Calculate inflation factor: Latest Index / Letting Date Index
                decimal inflationFactor = latestIndex / lettingDateIndex;
                
                // Calculate adjusted price: Original Price × Inflation Factor
                return originalPrice * inflationFactor;
            }
            catch
            {
                // Return original price if calculation fails
                return originalPrice;
            }
        }
    }
}

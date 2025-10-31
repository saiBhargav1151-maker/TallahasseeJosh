using System;
using System.Collections.Generic;

namespace Dqe.Web.Controllers
{
    /// <summary>
    /// Provides Florida Fisher Index values for each quarter, and helper to get quarter keys.
    /// Florida Fisher Index - Excludes Lump Sum, Frequency Rule, 3 Standard Deviations Rule, Rolling 3-Quarter Average
    /// Base: 2003 Q1 = 100
    /// </summary>
    public static class NHCCIData
    {
        // Dictionary mapping "Year Q#" to Florida Fisher Index. 
        // Covers from 2003 Q1 (base 100) up to 2025 Q2 (latest available).
        // Base year: 2003 Q1 = 100
        public static readonly Dictionary<string, decimal> IndexByQuarter = new Dictionary<string, decimal>
        {
            { "2003 Q1", 100m },
            { "2003 Q2", 105.0469464m },
            { "2003 Q3", 102.8286778m },
            { "2003 Q4", 105.8473179m },
            { "2004 Q1", 107.7564393m },
            { "2004 Q2", 113.201871m },
            { "2004 Q3", 116.8376847m },
            { "2004 Q4", 121.6472632m },
            { "2005 Q1", 129.098547m },
            { "2005 Q2", 144.9902834m },
            { "2005 Q3", 158.7232848m },
            { "2005 Q4", 170.0519814m },
            { "2006 Q1", 173.8555861m },
            { "2006 Q2", 179.2631103m },
            { "2006 Q3", 177.5294253m },
            { "2006 Q4", 181.0382843m },
            { "2007 Q1", 184.2249072m },
            { "2007 Q2", 184.4754227m },
            { "2007 Q3", 172.7804229m },
            { "2007 Q4", 160.4587702m },
            { "2008 Q1", 150.3031403m },
            { "2008 Q2", 139.4190166m },
            { "2008 Q3", 133.6251096m },
            { "2008 Q4", 130.6247615m },
            { "2009 Q1", 129.0634386m },
            { "2009 Q2", 121.8219328m },
            { "2009 Q3", 118.2602438m },
            { "2009 Q4", 115.9095944m },
            { "2010 Q1", 112.5942953m },
            { "2010 Q2", 110.3628499m },
            { "2010 Q3", 107.6890342m },
            { "2010 Q4", 104.9570498m },
            { "2011 Q1", 103.3886879m },
            { "2011 Q2", 101.7432152m },
            { "2011 Q3", 100.4698926m },
            { "2011 Q4", 96.80830836m },
            { "2012 Q1", 98.02127799m },
            { "2012 Q2", 99.42879332m },
            { "2012 Q3", 100.8313781m },
            { "2012 Q4", 104.8091692m },
            { "2013 Q1", 108.2252094m },
            { "2013 Q2", 110.7418847m },
            { "2013 Q3", 106.5544974m },
            { "2013 Q4", 101.4694184m },
            { "2014 Q1", 102.9584248m },
            { "2014 Q2", 111.6957155m },
            { "2014 Q3", 118.7463479m },
            { "2014 Q4", 121.822554m },
            { "2015 Q1", 123.1479068m },
            { "2015 Q2", 128.7368095m },
            { "2015 Q3", 126.6828422m },
            { "2015 Q4", 129.4533497m },
            { "2016 Q1", 131.9791722m },
            { "2016 Q2", 141.3926031m },
            { "2016 Q3", 145.2463324m },
            { "2016 Q4", 146.6871177m },
            { "2017 Q1", 148.6126979m },
            { "2017 Q2", 150.5503354m },
            { "2017 Q3", 154.4956281m },
            { "2017 Q4", 151.294595m },
            { "2018 Q1", 146.4493605m },
            { "2018 Q2", 146.2137062m },
            { "2018 Q3", 146.9510667m },
            { "2018 Q4", 150.3437907m },
            { "2019 Q1", 148.9549323m },
            { "2019 Q2", 156.1694473m },
            { "2019 Q3", 156.1179552m },
            { "2019 Q4", 201.3004608m },
            { "2020 Q1", 219.8208998m },
            { "2020 Q2", 242.1871881m },
            { "2020 Q3", 222.4635232m },
            { "2020 Q4", 218.7290689m },
            { "2021 Q1", 220.0636971m },
            { "2021 Q2", 227.180801m },
            { "2021 Q3", 236.7526726m },
            { "2021 Q4", 247.0089996m },
            { "2022 Q1", 250.5847575m },
            { "2022 Q2", 275.9990857m },
            { "2022 Q3", 278.0534276m },
            { "2022 Q4", 300.5569975m },
            { "2023 Q1", 307.708749m },
            { "2023 Q2", 323.2182234m },
            { "2023 Q3", 337.1829132m },
            { "2023 Q4", 332.3452703m },
            { "2024 Q1", 339.2540358m },
            { "2024 Q2", 320.6700704m },
            { "2024 Q3", 317.0124438m },
            { "2024 Q4", 310.3445904m },
            { "2025 Q1", 313.2363962m },
            { "2025 Q2", 319.9377476m }
        };

        /// <summary>
        /// Given a date, returns the quarter key in "YYYY Q#" format.
        /// </summary>
        public static string GetQuarterKey(DateTime date)
        {
            int quarter = (date.Month - 1) / 3 + 1;  
            return $"{date.Year} Q{quarter}";
        }

        /// <summary>
        /// Gets the latest available Florida Fisher Index value.
        /// </summary>
        public static decimal GetLatestIndex()
        {
            return IndexByQuarter["2025 Q2"]; 
        }

        /// <summary>
        /// Calculates the inflation-adjusted price using Florida Fisher Index.
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
                    return originalPrice;
                }

                decimal lettingDateIndex = IndexByQuarter[quarterKey];
                decimal latestIndex = GetLatestIndex();
                
                decimal inflationFactor = latestIndex / lettingDateIndex;
                
                return originalPrice * inflationFactor;
            }
            catch
            {
                return originalPrice;
            }
        }
    }
}

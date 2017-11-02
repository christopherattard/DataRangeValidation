using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateRangeValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            DateRange night1 =  new DateRange("night1", new DateTime(2017, 1, 1, 0, 0, 0), new DateTime(2017, 6, 30, 7, 59, 59)); 
            DateRange day =     new DateRange("day", new DateTime(2017, 1, 1, 7, 59, 59), new DateTime(2017, 6, 30, 19, 59, 59));             
            DateRange night2 =  new DateRange("night2", new DateTime(2017, 1, 1, 20, 0, 0), new DateTime(2017, 6, 30, 23, 59, 59)); 
            List<DateRange> lst = new List<DateRange> { day, night1, night2 };
            CoverAllRates(lst);

            Console.ReadLine();
        }
                
        public static void CoverAllRates(List<DateRange> dateRanges)
        {
            int matchCount;
            DateTime currentDate;

            ResetSeconds(dateRanges);

            Console.WriteLine("");

            List<DateRate> rates = ConvertToRates(dateRanges);

            DateTime earliest, latest;
            GetEarliestLatestRates(rates, out earliest, out latest);
            Console.WriteLine("Earliest: " + earliest + "     Latest: " + latest + "\r\n");

            //Get boundaries
            List<DateTime> boundaries = new List<DateTime>();
            foreach (DateRate r in rates)
            {
                boundaries.Add(r.From1);                                
                currentDate = r.From1.AddSeconds(-1);
                if (currentDate >= earliest) boundaries.Add(currentDate);                
                currentDate = r.From1.AddSeconds(1);
                if (currentDate <= latest) boundaries.Add(currentDate);

                boundaries.Add(r.From2);
                currentDate = r.From2.AddSeconds(-1);
                if (currentDate >= earliest) boundaries.Add(currentDate);
                currentDate = r.From2.AddSeconds(1);
                if (currentDate <= latest) boundaries.Add(currentDate);

                boundaries.Add(r.To1);
                currentDate = r.To1.AddSeconds(-1);
                if (currentDate >= earliest) boundaries.Add(currentDate);
                currentDate = r.To1.AddSeconds(1);
                if (currentDate <= latest) boundaries.Add(currentDate);

                boundaries.Add(r.To2);
                currentDate = r.To2.AddSeconds(-1);
                if (currentDate >= earliest) boundaries.Add(currentDate);
                currentDate = r.To2.AddSeconds(1);
                if (currentDate <= latest) boundaries.Add(currentDate);
            }

            Console.WriteLine("Boundaries:");
            foreach (DateTime b in boundaries)
            {
                Console.WriteLine(b);
            }

            Console.WriteLine("");

            Console.WriteLine("Rates:");
            foreach (DateRate r in rates)
            {
                Console.WriteLine(String.Format("{0} [{1}-{2}] [{3}-{4}]", r.name, r.From1, r.From2, r.To1, r.To2));
            }

            Console.WriteLine("");

            //Compare boundaries with rates
            List<DateTime> noMatches = new List<DateTime>();
            List<DateTime> gaps = new List<DateTime>();
            List<DateTime> overlaps = new List<DateTime>();
            foreach (DateTime b in boundaries)
            {
                Console.Write(b + " [");
                matchCount = 0;
                foreach (DateRate r in rates)
                {
                    if (b.Year == r.From1.Year && b.Month == r.From1.Month && b.Day == r.From1.Day && b >= r.From1 && b <= r.From2)
                    {
                        Console.Write(" " + r.name + " ");
                        matchCount++;
                    }

                    if (b.Year == r.To1.Year && b.Month == r.To1.Month && b.Day == r.To1.Day && b >= r.To1 && b <= r.To2)
                    {
                        Console.Write(" " + r.name + " ");
                        matchCount++;
                    }                    
                }

                Console.WriteLine("]");

                if (matchCount != 1)
                {
                    if (matchCount == 0) noMatches.Add(b);
                    else overlaps.Add(b);
                }
            }

            Console.WriteLine("");

            if (noMatches.Count > 0 && overlaps.Count == 0)
            {
                //Re-check the no matches due to different year, month, day
                foreach (DateTime g in noMatches)
                {
                    Console.Write(g + " [");
                    matchCount = 0;
                    foreach (DateRange dr in dateRanges)
                    {
                        if (g >= dr.RangeFrom && g <= dr.RangeTo)
                        {
                            DateTime dtFrom = new DateTime(g.Year, g.Month, g.Day, dr.RangeFrom.Hour, dr.RangeFrom.Minute, dr.RangeFrom.Second);
                            DateTime dtTo = new DateTime(g.Year, g.Month, g.Day, dr.RangeTo.Hour, dr.RangeTo.Minute, dr.RangeTo.Second);

                            if (g >= dtFrom && g <= dtTo)
                            {
                                Console.Write(dr.name + " ");
                                matchCount++;
                            }
                        }
                    }

                    Console.WriteLine("]");

                    if (matchCount != 1)
                    {
                        if (matchCount == 0) gaps.Add(g);
                        else overlaps.Add(g);
                    }
                }

            }

            Console.WriteLine("");

            if (gaps.Count == 0 && overlaps.Count == 0)
            {
                Console.WriteLine("*** Result: GOOD ***");
            }

            if (gaps.Count > 0)
            {
                Console.WriteLine("*** Result: GAPS ***");
                foreach (DateTime dt in gaps)
                {
                    Console.WriteLine(dt);
                }
            }            

            if (overlaps.Count > 0)
            {
                Console.WriteLine("*** Result: OVERLAPS ***");
                foreach (DateTime dt in overlaps)
                {
                    Console.WriteLine(dt);
                }
            }
        }

        public static void CoverAllMinutesWithBoundaries(List<DateRange> dateRanges)
        {
            int matchCount; bool match;
            DateTime currentDate;

            ResetSeconds(dateRanges);

            Console.WriteLine("");            

            DateTime earliest, latest;
            GetEarliestLatestDateRange(dateRanges, out earliest, out latest);
            Console.WriteLine("Earliest: " +earliest +"     Latest: "+latest +"\r\n");
            

            //Get boundaries
            List<DateTime> boundaries = new List<DateTime>();            
            foreach (DateRange r in dateRanges)
            {
                boundaries.Add(r.RangeFrom);

                //Subtract second and check if covered once by a rate
                currentDate = r.RangeFrom.AddSeconds(-1);
                if (currentDate >= earliest) boundaries.Add(currentDate);
                
                //Add second and check if covered once by a rate
                currentDate = r.RangeFrom.AddSeconds(1);
                if (currentDate <= latest) boundaries.Add(currentDate);


                boundaries.Add(r.RangeTo);

                //Subtract second and check if covered once by a rate
                currentDate = r.RangeTo.AddSeconds(-1);
                if (currentDate >= earliest) boundaries.Add(currentDate);

                //Add second and check if covered once by a rate
                currentDate = r.RangeTo.AddSeconds(1);
                if (currentDate <= latest) boundaries.Add(currentDate);
            }

            Console.WriteLine("Boundaries:");
            foreach (DateTime b in boundaries)
            {
                Console.WriteLine(b);
            }

            Console.WriteLine("");            

            //Compare boundaries with date ranges
            List<DateTime> gaps = new List<DateTime>();
            List<DateTime> overlaps = new List<DateTime>();            
            foreach (DateTime b in boundaries)
            {
                Console.Write(b +" [");
                matchCount = 0;
                foreach (DateRange r in dateRanges)
                {
                    if (b.Year == r.RangeFrom.Year && b.Month == r.RangeFrom.Month && b.Day == r.RangeFrom.Day && b >= r.RangeFrom)
                    {
                        Console.Write(" " + r.name + " ");
                        matchCount++;
                    }

                    if (b.Year == r.RangeTo.Year && b.Month == r.RangeTo.Month && b.Day == r.RangeTo.Day && b <= r.RangeTo)
                    {
                        Console.Write(" " + r.name + " ");
                        matchCount++;
                    }

                    /*if (b >= r.RangeFrom && b <= r.RangeTo)
                    {

                        Console.Write(" " +r.name +" ");
                        matchCount++;                        
                    }*/

                    //add day and check if covered once by a rate
                    //subtract day and check if covered once by a rate
                }

                Console.WriteLine("]");

                if (matchCount != 1)
                {
                    if (matchCount == 0) gaps.Add(b);
                    else overlaps.Add(b);
                }
            }

            if (gaps.Count == 0 && overlaps.Count == 0)
            {
                Console.WriteLine("Good");
            }

            if (gaps.Count > 0)
            {
                Console.WriteLine("Gaps");
                foreach (DateTime dt in gaps)
                {
                    Console.WriteLine(dt);
                }
            }

            if (overlaps.Count > 0)
            {
                Console.WriteLine("Overlaps");
                foreach (DateTime dt in overlaps)
                {
                    Console.WriteLine(dt);
                }
            }
        }

        public static void ParallelCoverAllMinutesWithSplit(List<DateRange> dateRanges)
        {
            Stopwatch s1 = Stopwatch.StartNew();

            ResetSeconds(dateRanges);

            List<DateRate> rates = ConvertToRates(dateRanges);

            DateTime earliest, latest;
            GetEarliestLatestDateRange(dateRanges, out earliest, out latest);

            #region Split date range into batches
            byte batchSize = 6;

            List<DateRange> batches = new List<DateRange>();
            DateTime batchStart = new DateTime(earliest.Year, earliest.Month, earliest.Day, earliest.Hour, earliest.Minute, 0);
            DateTime batchEnd = new DateTime(earliest.Year, earliest.Month, earliest.Day, earliest.Hour, earliest.Minute, 59);
            batchEnd = batchEnd.AddMonths(batchSize);

            int bn = 0;
            while (batchEnd < latest)
            {
                batches.Add(new DateRange("batch" + bn, batchStart, batchEnd));
                batchStart = batchEnd.AddSeconds(1); //new DateTime(batchEnd.Year, batchEnd.Month, batchEnd.Day, batchEnd.Hour, batchEnd.Minute+1, 0);
                batchEnd = batchEnd.AddMonths(batchSize); //new DateTime(batchStart.Year, batchStart.Month+3, batchStart.Day, batchStart.Hour, batchStart.Minute, 59);
                bn++;
            }

            //Insert last batch
            batches.Add(new DateRange("batch"+bn, batchStart, latest));

            for (int i = 0; i < batches.Count; i++)
            {
                Console.WriteLine("Batch[{0}] {1} - {2}", batches[i].name, batches[i].RangeFrom, batches[i].RangeTo);
            }
            Console.WriteLine("");
            #endregion

            //Process batches
            string[] checkResults = new string[batches.Count];
            ParallelLoopResult result = Parallel.For(0, batches.Count, i =>
            {
                //Console.WriteLine("Starting batch[{0}]", batches[i].name);

                int match = 0;
                DateTime currentDate = batches[i].RangeFrom;                
                while (currentDate <= batches[i].RangeTo)
                {
                    match = 0;
                    foreach (DateRange d in dateRanges)
                    {                        
                        DateTime rangeDay = d.RangeFrom;
                        DateTime dateFrom = new DateTime(rangeDay.Year, rangeDay.Month, rangeDay.Day, d.RangeFrom.Hour, d.RangeFrom.Minute, d.RangeFrom.Second);
                        DateTime dateTo = new DateTime(rangeDay.Year, rangeDay.Month, rangeDay.Day, d.RangeTo.Hour, d.RangeTo.Minute, d.RangeFrom.Second);
                        while (rangeDay <= d.RangeTo)
                        {
                            if (currentDate >= dateFrom && currentDate <= dateTo)
                            {
                                match++;
                                //Console.WriteLine("Batch{0}: {1} matches [{2} - {3}]", i, currentDate, dateFrom, dateTo); 
                            }
                            rangeDay = rangeDay.AddDays(1);
                            dateFrom = dateFrom.AddDays(1);
                            dateTo = dateTo.AddDays(1);
                        }
                    }

                    if (match != 1)
                    {
                        checkResults[i] = match < 1 ? "gap" : "overlap";
                        if (match == 0)
                            Console.WriteLine("{0} - Cannot match currentDate {1}", batches[i].name, currentDate);
                        else
                            Console.WriteLine("{0} - Overlap currentDate {1}", batches[i].name, currentDate);

                        currentDate = batches[i].RangeTo.AddDays(1); //stop loop
                    }
                    else
                    {
                        currentDate = currentDate.AddMinutes(1);
                    }
                }

                if (match == 1) checkResults[i] = "good";

                Console.WriteLine("Finished batch[{0}]", batches[i].name);
            });

            s1.Stop();

            Console.WriteLine("Result: {0}", result.IsCompleted ? String.Format("Completed Normally in {0}", s1.Elapsed.Seconds) : String.Format("Completed to {0}", result.LowestBreakIteration));

            for (int i = 0; i < batches.Count; i++)
            {
                Console.WriteLine("Batch[{0}] {1} - {2}  Result: {3}", batches[i].name, batches[i].RangeFrom, batches[i].RangeTo, checkResults[i]);
            }
        }

        /// <summary> 
        /// Check if between RangeFrom.Date and RangeTo.Date and check if between RangeFrom.Time and RangeTo.Time 
        /// This will support rates for e.g. day rates between 8am - 8pm and night rates between 8pm - 8am 
        /// WARNING: VERY SLOW!! 
        /// </summary> 
        /// <param name="dateRanges"></param> 
        /// <returns></returns> 
        public static string CoverAllMinutesWithSplit(List<DateRange> dateRanges)
        {
            foreach (DateRange c in dateRanges)
            {
                c.RangeFrom = new DateTime(c.RangeFrom.Year, c.RangeFrom.Month, c.RangeFrom.Day, c.RangeFrom.Hour, c.RangeFrom.Minute, 0);
                c.RangeTo = new DateTime(c.RangeTo.Year, c.RangeTo.Month, c.RangeTo.Day, c.RangeTo.Hour, c.RangeTo.Minute, 0);
            }

            DateTime earliest, latest;
            GetEarliestLatestDateRange(dateRanges, out earliest, out latest);

            int i = 0;
            DateTime currentDate = earliest; // new DateTime(2017,1,1,7,59,00); 
            while (currentDate <= latest)
            {
                i = 0;
                foreach (DateRange c in dateRanges)
                {
                    DateTime rangeDay = c.RangeFrom;
                    DateTime dateFrom = new DateTime(rangeDay.Year, rangeDay.Month, rangeDay.Day, c.RangeFrom.Hour, c.RangeFrom.Minute, 00);
                    DateTime dateTo = new DateTime(rangeDay.Year, rangeDay.Month, rangeDay.Day, c.RangeTo.Hour, c.RangeTo.Minute, 59);
                    while (rangeDay <= c.RangeTo)
                    {
                        if (currentDate >= dateFrom && currentDate <= dateTo)
                        {
                            i++;
                            //Console.WriteLine(String.Format("{0} matches [{1} - {2}]", currentDate, dateFrom, dateTo)); 
                        }
                        rangeDay = rangeDay.AddDays(1);
                        dateFrom = dateFrom.AddDays(1);
                        dateTo = dateTo.AddDays(1);
                    }
                }

                if (i != 1) return (i < 1 ? "gap" : "overlap");
                currentDate = currentDate.AddMinutes(1);
            }

            return "good";
        }


        private static void GetEarliestLatestDateRange(List<DateRange> dateRanges, out DateTime earliest, out DateTime latest)
        {
            earliest = dateRanges[0].RangeFrom;
            latest = dateRanges[0].RangeTo;
            foreach (DateRange c in dateRanges)
            {
                if (c.RangeFrom < earliest) earliest = c.RangeFrom;
                if (c.RangeTo < earliest) earliest = c.RangeTo;

                if (c.RangeFrom > latest) latest = c.RangeFrom;
                if (c.RangeTo > latest) latest = c.RangeTo;
            }
        }

        private static void GetEarliestLatestRates(List<DateRate> rates, out DateTime earliest, out DateTime latest)
        {
            earliest = rates[0].From1;
            latest = rates[0].To1;
            foreach (DateRate c in rates)
            {
                if (c.From1 < earliest) earliest = c.From1;
                if (c.From2 < earliest) earliest = c.From2;
                if (c.To1 < earliest) earliest = c.To1;
                if (c.To2 < earliest) earliest = c.To2;
                
                if (c.From1 > latest) latest = c.From1;
                if (c.From2 > latest) latest = c.From2;
                if (c.To1 > latest) latest = c.To1;
                if (c.To2 > latest) latest = c.To2;
            }
        }

        private static void ResetSeconds(List<DateRange> dateRanges)
        {
            foreach (DateRange d in dateRanges)
            {
                d.RangeFrom = new DateTime(d.RangeFrom.Year, d.RangeFrom.Month, d.RangeFrom.Day, d.RangeFrom.Hour, d.RangeFrom.Minute, 0);
                d.RangeTo = new DateTime(d.RangeTo.Year, d.RangeTo.Month, d.RangeTo.Day, d.RangeTo.Hour, d.RangeTo.Minute, 59);

                Console.WriteLine("[{0}] from {1} to {2}", d.name, d.RangeFrom, d.RangeTo);
            }            
        }

        private static List<DateRate> ConvertToRates(List<DateRange> dateRanges)
        {
            List<DateRate> lst = new List<DateRate>();
            foreach (DateRange dr in dateRanges)
            {
                lst.Add(new DateRate(dr));
            }

            return lst;
        }
    }

    public class DateRange
    {
        public DateTime RangeFrom;
        public DateTime RangeTo;
        public string name;

        public DateRange(string name, DateTime from, DateTime to)
        {
            this.name = name;
            this.RangeFrom = from;
            this.RangeTo = to;
        }
    }

    public class DateRate
    {
        public DateTime From1;
        public DateTime From2;
        public DateTime To1;
        public DateTime To2;
        public string name;

        public DateRate(DateRange range)
        {
            this.name = range.name;
            this.From1 = range.RangeFrom;
            this.From2 = new DateTime(range.RangeFrom.Year, range.RangeFrom.Month, range.RangeFrom.Day, range.RangeTo.Hour, range.RangeTo.Minute, range.RangeTo.Second);
            this.To1 = new DateTime(range.RangeTo.Year, range.RangeTo.Month, range.RangeTo.Day, range.RangeFrom.Hour, range.RangeFrom.Minute, range.RangeFrom.Second);
            this.To2 = range.RangeTo;
        }
    }
}

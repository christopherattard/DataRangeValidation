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
            DateRange day =     new DateRange("day", new DateTime(2017, 1, 1, 8, 0, 0), new DateTime(2017, 6, 30, 19, 59, 59));             
            DateRange night2 =  new DateRange("night2", new DateTime(2017, 1, 1, 20, 0, 0), new DateTime(2017, 6, 30, 23, 59, 59)); 
            List<DateRange> lst = new List<DateRange> { day, night1, night2 }; 
            //Console.WriteLine(CoverAllMinutesWithSplit(lst));
            ParallelCoverAllMinutesWithSplit(lst);

            Console.ReadLine();
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

        public static void ParallelCoverAllMinutesWithSplit(List<DateRange> dateRanges)
        {
            Stopwatch s1 = Stopwatch.StartNew();
            foreach (DateRange d in dateRanges)
            {
                d.RangeFrom = new DateTime(d.RangeFrom.Year, d.RangeFrom.Month, d.RangeFrom.Day, d.RangeFrom.Hour, d.RangeFrom.Minute, 0);
                d.RangeTo = new DateTime(d.RangeTo.Year, d.RangeTo.Month, d.RangeTo.Day, d.RangeTo.Hour, d.RangeTo.Minute, 59);

                Console.WriteLine("[{0}] from {1} to {2}", d.name, d.RangeFrom, d.RangeTo);
            }

            Console.WriteLine("");

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
}

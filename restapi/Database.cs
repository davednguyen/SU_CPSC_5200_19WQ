using System;
using System.Collections.Generic;
using System.Linq;
using restapi.Models;

namespace restapi
{
    public static class Database
    {
        private static readonly IDictionary<string, Timecard> Timecards = 
            new Dictionary<string, Timecard>();
        
        public static IEnumerable<Timecard> All
        {
            get => Timecards.Values.ToList();
        }

        public static Timecard Find(string id)
        {
            Timecard timecard = null;

            if (Timecards.TryGetValue(id, out timecard) == true) 
            {
                return timecard;
            }
            else
            {
                return null;
            }
        }

        public static void Add(Timecard timecard)
        {
            Timecards.Add(timecard.Identity.Value, timecard);
        }

        /// <summary>
        /// To remove time card from Database
        /// </summary>
        /// <param name="timecard"></param>
        public static void Remove(Timecard timecard)
        {
            Timecards.Remove(timecard.Identity.Number);
        }
    }
}
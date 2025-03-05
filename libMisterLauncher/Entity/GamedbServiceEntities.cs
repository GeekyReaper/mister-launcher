using Amazon.Runtime.Internal;
using libMisterLauncher.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JobState { RUNNING, DONE, CANCEL}

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum JobType { SCANROM, MATCHINGROM, SCANSYSTEM, UNDEFINNED }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LogResult { INFO, WARNING, SUCCEED, FAILED }

    public class JobMister
    {
        const int maxHistoryLogItem = 10;
        public string jobName { get; set; } = "";
        public List<JobLog> logs { get; set; } = new List<JobLog>();

        public JobType jobType { get; set; } = JobType.UNDEFINNED;
        public DateTime start { get; set; } = DateTime.Now;
        public long delay { get; set; } = 0;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobState state { get; set; } = JobState.RUNNING;

        public GbdRomUpdateResult result { get; set; } = new GbdRomUpdateResult();

        public void UpdateDelay()
        {
            delay = (int)(DateTime.Now - start).TotalMilliseconds;
        }
      
        public void AddLog(string title, string body, LogResult logresult = LogResult.INFO)
        {
            AddLog(new JobLog(title, body, logresult));           
        }

        public void AddLog(JobLog log)
        {
            logs.Add(log);
            if (logs.Count > maxHistoryLogItem)
            {
                logs.RemoveRange(0, logs.Count - maxHistoryLogItem);
            }
        }



    }

    public class JobLog
    {
        public DateTime start { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogResult result { get; set; }

        public JobLog (string title, string body, LogResult result = LogResult.INFO)
        {
            this.title = title;
            this.body = body;
            this.result = result;
            start = DateTime.Now;

        }
    }
    public class GbdRomUpdateResult
    {
        private int _insert = 0;
        public int Insert { get { return _insert; } }
        private int _update = 0;
        public int Update { get { return _update; } }
        private int _income = 0;
        private int _match = 0;
        public int Match { get { return _match; } }

        private int _videogameUpdate = 0;
        public int VideogameUpdate { get { return _videogameUpdate; } }

        private int _videogameCreate = 0;
        public int VideogameCreate { get { return _videogameCreate; } }

        private int _iteration = 0;
        public int Income { get { return _income; } }

        public int Progress { get { return _iteration == 0 || _income == 0 ? 0 : (int)(_iteration * 100 / _income); } }

        public void Initialize (int income) { _income =  income; _insert = 0; _match = 0; _iteration = 0; }
        public void IncInsert ()
        {
            _insert++;
        }
        public void IncUpdate()
        {
            _update++;
        }
        public void IncVideogameUpdate()
        {
            _videogameUpdate++;
        }
        public void IncVideogameCreate()
        {
            _videogameCreate++;
        }
        public void IncMatch()
        {
            _match++;
        }

        public void Newiteration()
        {
            _iteration++;
        }

        public static GbdRomUpdateResult operator +(GbdRomUpdateResult a, GbdRomUpdateResult b)
        {
            return new GbdRomUpdateResult {
                _iteration = a._iteration + b._iteration,
                _income = a.Income + b.Income,
                _insert = a.Insert + b.Insert,
                _update = a._update + b._update,
                _match = a._match + b._match,
                _videogameCreate = a._videogameCreate + b._videogameCreate,
                _videogameUpdate = a._videogameUpdate + b._videogameUpdate};
        }

    }

    public class ItemCount
    {
        public string value { get; set; } = "";
        public int count { get; set; } = 0;
        public string label { get; set; } = "";
    }

    public class SearchVideoGameFilter
    {
        public List<ItemCount> systemCategory { get; set; } = new List<ItemCount>();
        public List<ItemCount> systems { get; set; } = new List<ItemCount>();
        public List<ItemCount> playlist { get; set; } = new List<ItemCount>();
        public List<ItemCount> gametype { get; set; } = new List<ItemCount>();
        public List<ItemCount> players { get; set; } = new List<ItemCount>();
    }
}

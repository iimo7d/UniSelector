using System.Collections.Concurrent;
using Uni_Selector.ViewModels.SystemHealth;

namespace Uni_Selector.Helpers
{
    public class ResponseTimeTracker
    {
        private static readonly ConcurrentQueue<ResponseTimeRecord> _recentRecords = new();
        private static readonly object _lockObject = new object();
        private static readonly int _maxRecords = 1000; // Keep last 1000 requests

        private static double _totalResponseTime = 0;
        private static long _totalRequests = 0;
        private static double _minResponseTime = double.MaxValue;
        private static double _maxResponseTime = double.MinValue;


        public static void RecordResponseTime(string path, string method, int statusCode, double responseTimeMs)
        {
            lock (_lockObject)
            {
                var record = new ResponseTimeRecord
                {
                    Timestamp = DateTime.UtcNow,
                    Path = path,
                    Method = method,
                    StatusCode = statusCode,
                    ResponseTimeMs = responseTimeMs
                };

                // Add to queue
                _recentRecords.Enqueue(record);

                // Update statistics
                _totalResponseTime += responseTimeMs;
                _totalRequests++;
                _minResponseTime = Math.Min(_minResponseTime, responseTimeMs);
                _maxResponseTime = Math.Max(_maxResponseTime, responseTimeMs);

                // Maintain max size
                while (_recentRecords.Count > _maxRecords)
                {
                    if (_recentRecords.TryDequeue(out var oldRecord))
                    {
                        _totalResponseTime -= oldRecord.ResponseTimeMs;
                        _totalRequests--;
                    }
                }
            }
        }

        /// <summary>
        /// Get average response time for all recorded requests
        /// </summary>
        public static double GetAverageResponseTime()
        {
            lock (_lockObject)
            {
                return _totalRequests > 0 ? _totalResponseTime / _totalRequests : 0.0;
            }
        }

        /// <summary>
        /// Get average response time for last N requests
        /// </summary>
        public static double GetAverageResponseTime(int lastNRequests)
        {
            lock (_lockObject)
            {
                var records = _recentRecords.TakeLast(lastNRequests).ToList();
                return records.Any() ? records.Average(r => r.ResponseTimeMs) : 0.0;
            }
        }

        /// <summary>
        /// Get average response time for a specific time period
        /// </summary>
        public static double GetAverageResponseTime(TimeSpan period)
        {
            lock (_lockObject)
            {
                var cutoffTime = DateTime.UtcNow - period;
                var recentRecords = _recentRecords
                    .Where(r => r.Timestamp >= cutoffTime)
                    .ToList();

                return recentRecords.Any() ? recentRecords.Average(r => r.ResponseTimeMs) : 0.0;
            }
        }

        /// <summary>
        /// Get detailed statistics
        /// </summary>
        public static ResponseTimeStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                var records = _recentRecords.ToList();
                var now = DateTime.UtcNow;

                return new ResponseTimeStatistics
                {
                    TotalRequests = _totalRequests,
                    AverageResponseTime = GetAverageResponseTime(),
                    MinResponseTime = _minResponseTime == double.MaxValue ? 0 : _minResponseTime,
                    MaxResponseTime = _maxResponseTime == double.MinValue ? 0 : _maxResponseTime,
                    MedianResponseTime = CalculateMedian(records.Select(r => r.ResponseTimeMs).ToList()),

                    // Last 100 requests
                    Last100Average = GetAverageResponseTime(100),

                    // Time-based averages
                    Last1MinuteAverage = GetAverageResponseTime(TimeSpan.FromMinutes(1)),
                    Last5MinutesAverage = GetAverageResponseTime(TimeSpan.FromMinutes(5)),
                    Last15MinutesAverage = GetAverageResponseTime(TimeSpan.FromMinutes(15)),
                    Last1HourAverage = GetAverageResponseTime(TimeSpan.FromHours(1)),

                    // Status code breakdown
                    SuccessfulRequests = records.Count(r => r.StatusCode >= 200 && r.StatusCode < 300),
                    ClientErrors = records.Count(r => r.StatusCode >= 400 && r.StatusCode < 500),
                    ServerErrors = records.Count(r => r.StatusCode >= 500),

                    // Performance thresholds
                    FastRequests = records.Count(r => r.ResponseTimeMs < 100),      // < 100ms
                    NormalRequests = records.Count(r => r.ResponseTimeMs >= 100 && r.ResponseTimeMs < 500),
                    SlowRequests = records.Count(r => r.ResponseTimeMs >= 500 && r.ResponseTimeMs < 1000),
                    VerySlow = records.Count(r => r.ResponseTimeMs >= 1000),       // > 1s

                    // Requests per minute
                    RequestsLastMinute = records.Count(r => r.Timestamp >= now.AddMinutes(-1)),
                    RequestsLastHour = records.Count(r => r.Timestamp >= now.AddHours(-1)),

                    // Slowest endpoints
                    SlowestEndpoints = GetSlowestEndpoints(5)
                };
            }
        }

        /// <summary>
        /// Get slowest endpoints
        /// </summary>
        public static List<EndpointPerformance> GetSlowestEndpoints(int count = 10)
        {
            lock (_lockObject)
            {
                return _recentRecords
                    .GroupBy(r => new { r.Path, r.Method })
                    .Select(g => new EndpointPerformance
                    {
                        Path = g.Key.Path,
                        Method = g.Key.Method,
                        AverageResponseTime = g.Average(r => r.ResponseTimeMs),
                        MinResponseTime = g.Min(r => r.ResponseTimeMs),
                        MaxResponseTime = g.Max(r => r.ResponseTimeMs),
                        RequestCount = g.Count()
                    })
                    .OrderByDescending(e => e.AverageResponseTime)
                    .Take(count)
                    .ToList();
            }
        }

        /// <summary>
        /// Get fastest endpoints
        /// </summary>
        public static List<EndpointPerformance> GetFastestEndpoints(int count = 10)
        {
            lock (_lockObject)
            {
                return _recentRecords
                    .GroupBy(r => new { r.Path, r.Method })
                    .Select(g => new EndpointPerformance
                    {
                        Path = g.Key.Path,
                        Method = g.Key.Method,
                        AverageResponseTime = g.Average(r => r.ResponseTimeMs),
                        MinResponseTime = g.Min(r => r.ResponseTimeMs),
                        MaxResponseTime = g.Max(r => r.ResponseTimeMs),
                        RequestCount = g.Count()
                    })
                    .OrderBy(e => e.AverageResponseTime)
                    .Take(count)
                    .ToList();
            }
        }

        /// <summary>
        /// Get response time percentiles
        /// </summary>
        public static ResponseTimePercentiles GetPercentiles()
        {
            lock (_lockObject)
            {
                var times = _recentRecords.Select(r => r.ResponseTimeMs).OrderBy(t => t).ToList();

                if (!times.Any())
                {
                    return new ResponseTimePercentiles();
                }

                return new ResponseTimePercentiles
                {
                    P50 = CalculatePercentile(times, 50),
                    P75 = CalculatePercentile(times, 75),
                    P90 = CalculatePercentile(times, 90),
                    P95 = CalculatePercentile(times, 95),
                    P99 = CalculatePercentile(times, 99)
                };
            }
        }

        /// <summary>
        /// Reset all statistics
        /// </summary>
        public static void Reset()
        {
            lock (_lockObject)
            {
                _recentRecords.Clear();
                _totalResponseTime = 0;
                _totalRequests = 0;
                _minResponseTime = double.MaxValue;
                _maxResponseTime = double.MinValue;
            }
        }

        /// <summary>
        /// Get recent records
        /// </summary>
        public static List<ResponseTimeRecord> GetRecentRecords(int count = 100)
        {
            lock (_lockObject)
            {
                return _recentRecords.TakeLast(count).ToList();
            }
        }

        #region Helper Methods

        private static double CalculateMedian(List<double> values)
        {
            if (!values.Any()) return 0;

            var sorted = values.OrderBy(v => v).ToList();
            int count = sorted.Count;

            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        private static double CalculatePercentile(List<double> sortedValues, int percentile)
        {
            if (!sortedValues.Any()) return 0;

            int count = sortedValues.Count;
            double n = (percentile / 100.0) * (count - 1);
            int k = (int)Math.Floor(n);
            double d = n - k;

            if (k + 1 < count)
            {
                return sortedValues[k] + d * (sortedValues[k + 1] - sortedValues[k]);
            }
            else
            {
                return sortedValues[k];
            }
        }

        #endregion

    }
}

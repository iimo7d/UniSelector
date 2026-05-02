using System.Collections.Concurrent;

namespace Uni_Selector.Service.Interface
{
    public class RecommendationUpdateQueue : IRecommendationUpdateQueue
    {
        private readonly ConcurrentQueue<RegenerationRequest> _queue = new();
        private readonly object _lock = new();

        public void QueueRegenerationForAllStudents(string reason)
        {
            _queue.Enqueue(new RegenerationRequest
            {
                ForAllStudents = true,
                Reason = reason,
                QueuedAt = DateTime.UtcNow
            });
        }

        public void QueueRegenerationForStudent(int studentId, string reason)
        {
            _queue.Enqueue(new RegenerationRequest
            {
                ForAllStudents = false,
                StudentIds = new List<int> { studentId },
                Reason = reason,
                QueuedAt = DateTime.UtcNow
            });
        }

        public bool HasPendingRegenerations()
        {
            return !_queue.IsEmpty;
        }

        public (bool forAllStudents, List<int> studentIds, string reason) DequeuePendingRegenerations()
        {
            lock (_lock)
            {
                var studentIds = new HashSet<int>();
                var reasons = new List<string>();
                var forAllStudents = false;

                // Process all pending requests
                while (_queue.TryDequeue(out var request))
                {
                    if (request.ForAllStudents)
                    {
                        forAllStudents = true;
                        reasons.Add(request.Reason);
                        // If regenerating for all, no need to track individual students
                        studentIds.Clear();
                        break;
                    }
                    else
                    {
                        foreach (var id in request.StudentIds)
                        {
                            studentIds.Add(id);
                        }
                        reasons.Add(request.Reason);
                    }
                }

                var combinedReason = reasons.Any() ? string.Join("; ", reasons.Distinct()) : "Unknown";
                return (forAllStudents, studentIds.ToList(), combinedReason);
            }
        }

        private class RegenerationRequest
        {
            public bool ForAllStudents { get; set; }
            public List<int> StudentIds { get; set; } = new();
            public string Reason { get; set; }
            public DateTime QueuedAt { get; set; }
        }
    }
}

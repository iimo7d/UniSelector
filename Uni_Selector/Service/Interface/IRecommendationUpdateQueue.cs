namespace Uni_Selector.Service.Interface
{
    public interface IRecommendationUpdateQueue
    {
        void QueueRegenerationForAllStudents(string reason);
        void QueueRegenerationForStudent(int studentId, string reason);
        bool HasPendingRegenerations();
        (bool forAllStudents, List<int> studentIds, string reason) DequeuePendingRegenerations();
    }
}

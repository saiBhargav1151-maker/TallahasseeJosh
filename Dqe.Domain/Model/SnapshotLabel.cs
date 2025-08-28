namespace Dqe.Domain.Model
{
    public enum SnapshotLabel
    {
        Estimator = 'E',
        Review = 'R',
        Coder = 'C',
        Initial = 'I',
        Scope = 'S',
        Phase1 = '1',
        Phase2 = '2',
        Phase3 = '3',
        Phase4 = '4',
        Authorization = 'A',
        Official = 'O'
    }

    public enum SnapshotLabelOrder
    {
        Estimator = 1,
        Review = 2,
        Initial = 3,
        Scope = 4,
        Phase1 = 5,
        Phase2 = 6,
        Phase3 = 7,
        Phase4 = 8,
        Authorization = 9,
        Official = 10
    }
}
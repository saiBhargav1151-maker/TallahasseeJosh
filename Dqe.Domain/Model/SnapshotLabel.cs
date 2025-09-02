namespace Dqe.Domain.Model
{
    public enum SnapshotLabel
    {
        /// <summary>
        /// Snapshot made by Estimator user
        /// </summary>
        Estimator = 'E',

        /// <summary>
        /// Snapshot made by Review user
        /// </summary>
        Review = 'R',

        /// <summary>
        /// Snapshot made by Coder user
        /// </summary>
        Coder = 'C',

        /// <summary>
        /// Initial milestone (Order:1)
        /// </summary>
        Initial = 'I',

        /// <summary>
        /// Scope milestone (Order:2)
        /// </summary>
        Scope = 'S',

        /// <summary>
        /// Phase1 milestone (Order:3)
        /// </summary>
        Phase1 = '1',

        /// <summary>
        /// Phase2 milestone (Order:4)
        /// </summary>
        Phase2 = '2',

        /// <summary>
        /// Phase3 milestone (Order:5)
        /// </summary>
        Phase3 = '3',

        /// <summary>
        /// Phase4 milestone (Order:6)
        /// </summary>
        Phase4 = '4',

        /// <summary>
        /// Authorization milestone (Order:7)
        /// </summary>
        Authorization = 'A',

        /// <summary>
        /// Official milestone (Order:8)
        /// </summary>
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
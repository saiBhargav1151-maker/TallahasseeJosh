namespace Dqe.Domain.Model.Lre
{
    public class Project
    {
        public virtual long Id { get; set; }

        public virtual string ProjectName { get; set; }

        public virtual string District { get; set; }

        /// <summary>
        /// Dictates to user if they want DQE as the primary program instead of LRE
        /// It is in the DB as a single char byte
        /// </summary>
        public virtual string QuantitiesComplete { get; set; }

    }
}
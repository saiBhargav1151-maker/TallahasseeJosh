namespace Dqe.Domain.Model.Reports
{
    public class ReportLettingSummary
    {
        public virtual long Id { get; set; }

        public virtual string ContractRange { get; set; }

        public virtual int NumberOfContracts { get; set; }

        public virtual decimal ValueInCategory { get; set; }

        public virtual decimal ValueOfEstimate { get; set; }

        public virtual decimal PercentageOfContracts { get; set; }

        public virtual decimal PercentageOfLettingTotal { get; set; }

        public virtual int NumberOfContractsBelowEstimate { get; set; }

        public virtual string PercentageRangeDifferenceBelow { get; set; }

        public virtual decimal AveragePercentageBelowEstimate { get; set; }

        public virtual int NumberOfContractsAboveEstimate { get; set; }

        public virtual string PercentageRangeDifferenceAbove { get; set; }

        public virtual decimal AveragePercentageAboveEstimate { get; set; }

        public virtual decimal AverageBidsPerContract { get; set; }

        public virtual ReportLetting MyReportLetting { get; protected internal set; }
    }
}

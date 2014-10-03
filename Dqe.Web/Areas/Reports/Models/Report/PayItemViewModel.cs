using System;

namespace Dqe.Web.Areas.Reports.Models.Report
{
    public class PayItemViewModel
    {
        public string PayItemId { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Tsp { get; set; }
        public DateTime ValidDate { get; set; }
        public DateTime ObsoleteDate { get; set; }
    }
}
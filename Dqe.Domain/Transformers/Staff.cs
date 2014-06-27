using System.Collections;

namespace Dqe.Domain.Transformers
{
    public class Staff
    {
        private string _district;

        public string UserId { get; set; }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string District
        {
            get { return _district; }
            set
            {
                switch (value)
                {
                    case "01" :
                        _district = "D1";
                        break;
                    case "02" :
                        _district = "D2";
                        break;
                    case "03":
                        _district = "D3";
                        break;
                    case "04":
                        _district = "D4";
                        break;
                    case "05":
                        _district = "D5";
                        break;
                    case "06":
                        _district = "D6";
                        break;
                    case "07":
                        _district = "D7";
                        break;
                    case "08":
                        _district = "TP";
                        break;
                    default: 
                        _district = "CO";
                        break;
                }
            }
        }

        public string FullName
        {
            get { return string.Format("{0} {1} ({2})", FirstName, LastName, UserId); }
        }
    }
}
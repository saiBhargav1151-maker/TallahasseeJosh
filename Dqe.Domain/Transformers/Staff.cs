namespace Dqe.Domain.Transformers
{
    public class Staff
    {
        private string _district;
        private string _phoneNumber;

        public string UserId { get; set; }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneExt { get; set; }

        public string PhoneNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_phoneNumber)) return _phoneNumber;
                if (_phoneNumber.Length == 10)
                {
                    return string.Format("({0}) {1}-{2}", _phoneNumber.Substring(0, 3), _phoneNumber.Substring(3, 3),
                        _phoneNumber.Substring(6, 4));
                }
                return _phoneNumber;
            }
            set { _phoneNumber = value; }
        }

        public string PhoneAndExtension
        {
            get
            {
                return string.Format("{0}{1}", string.IsNullOrWhiteSpace(PhoneNumber) ? string.Empty : PhoneNumber,
                    string.IsNullOrWhiteSpace(PhoneExt) ? string.Empty : string.Format(" Ext: {0}", PhoneExt));
            }
        }

        public string Email { get; set; }

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
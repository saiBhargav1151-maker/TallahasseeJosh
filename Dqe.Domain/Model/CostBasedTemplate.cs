using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace Dqe.Domain.Model
{
    public class CostBasedTemplate : Entity<Transformers.CostBasedTemplate>
    {
        private readonly ICollection<CostBasedTemplateDocumentVersion> _documentVersions = new Collection<CostBasedTemplateDocumentVersion>();
        private readonly ICollection<PayItemMaster> _payItemMasters = new Collection<PayItemMaster>();

        public virtual IEnumerable<CostBasedTemplateDocumentVersion> DocumentVersions
        {
            get { return _documentVersions.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<PayItemMaster> PayItemMasters
        {
            get { return _payItemMasters.ToList().AsReadOnly(); }
        }

        public virtual CostBasedTemplateDocumentVersion CurrentDocumentVersion
        {
            get
            {
                if(DocumentVersions == null || !DocumentVersions.Any())
                    return null;

                var timestamp=DocumentVersions.Max(d=>d.Timestamp);
                var docVersion=DocumentVersions.Single(d => d.Timestamp == timestamp);

                return docVersion;
            }
        }

        public virtual void AddDocumentVersion(CostBasedTemplateDocumentVersion documentVersion, DqeUser account)
        {
            if (documentVersion == null) throw new ArgumentNullException("documentVersion");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.Administrator && account.Role != DqeRole.CostBasedTemplateAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.",
                    account.Role));
            }

            documentVersion.MyCostBasedTemplate = this;
            _documentVersions.Add(documentVersion);
        }

        [Required]
        [StringLength(8)]
        public virtual string ResultCell { get; protected set; }
        [Required]
        [StringLength(255)]
        public virtual string Name { get; protected set; }


        //private Document _document;
        public virtual string Column
        {
            get
            {
                var numAlpha = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9]*)");
                var match = numAlpha.Match(ResultCell);


                var alpha = match.Groups["Alpha"].Value;
                return alpha;
            }
        }

        public virtual int Row
        {
            get
            {
                var numAlpha = new Regex("(?<Alpha>[a-zA-Z]*)(?<Numeric>[0-9]*)");
                var match = numAlpha.Match(ResultCell);
                var num = match.Groups["Numeric"].Value;
                return Convert.ToInt32(num);
            }
        }
        public override Transformers.CostBasedTemplate GetTransformer()
        {
            var transformer = new Transformers.CostBasedTemplate
            {
                Id = Id,
                Name = Name,
                ResultCell = ResultCell,
            };

            return transformer;
        }

        public override void Transform(Transformers.CostBasedTemplate transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.Administrator && account.Role != DqeRole.CostBasedTemplateAdministrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Name = transformer.Name;
            ResultCell = transformer.ResultCell;

        }

        public virtual IEnumerable<ValidationResult> ValidateExcelFile(Document document)
        {
            //_document = document;
            var results = new List<ValidationResult>();

            using (var ms = new MemoryStream(document.FileData))
            {
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(ms))
                    {
                        if (workbook.Worksheets.Count >= 1)
                        {
                            var worksheet = workbook.Worksheets.First();

                            var cell = worksheet.Cell(ResultCell);

                            if (!cell.HasFormula)
                            {
                                results.Add(new ValidationResult("The results cell must contain a formula in it."));
                            }
                        }
                        else
                        {
                            results.Add(new ValidationResult("The excel file must have at least one workbook in it."));
                        }    
                    }
                }
                catch
                {
                    //Catch an exception for invalid excel file
                    results.Add(new ValidationResult("The excel file upload is either corrupted or invalid."));
                }

            }

            return results;
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            //if (_document == null)
            //{
            //    throw new Exception("The excel file must be validated before the save can occur");
            //}

            //results.AddRange(ValidateExcelFile(_document));

            results.AddRange(base.Validate(validationContext));

            return results;
        }
    }
}

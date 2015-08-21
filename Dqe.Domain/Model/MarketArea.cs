using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.Domain.Model
{
    public class MarketArea : Entity<Transformers.MarketArea>
    {
        private readonly ICollection<County> _counties;
        private readonly ICollection<MarketAreaAveragePrice> _marketAreaAveragePrices;
        private readonly IMarketAreaRepository _marketAreaRepository;

        public MarketArea(IMarketAreaRepository marketAreaRepository)
        {
            _counties = new Collection<County>();
            _marketAreaAveragePrices = new Collection<MarketAreaAveragePrice>();
            _marketAreaRepository = marketAreaRepository;
        }

        public virtual IEnumerable<MarketAreaAveragePrice> MarketAreaAveragePrices
        {
            get { return _marketAreaAveragePrices.ToList().AsReadOnly(); }
        }

        [StringLength(256)]
        public virtual string Name { get; protected internal set; }

        public virtual IEnumerable<County> Counties
        {
            get { return _counties.ToList().AsReadOnly(); }
        }

        public virtual void AddCounty(County county, DqeUser account)
        {
            if (county == null) throw new ArgumentNullException("county");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            if (_counties.Contains(county)) return;
            _counties.Add(county);
            county.MyMarketArea = this;
        }

        public virtual void AddItemAveragePrice(PayItemMaster payItemMaster, MarketAreaAveragePrice averagePrice)
        {
            var match = _marketAreaAveragePrices.FirstOrDefault(i => i.MyPayItemMaster.RefItemName == payItemMaster.RefItemName);
            if (match != null) _marketAreaAveragePrices.Remove(match);
            if (averagePrice.Price <= 0) return;
            averagePrice.MyMarketArea = this;
            averagePrice.MyPayItemMaster = payItemMaster;
            _marketAreaAveragePrices.Add(averagePrice);
        }

        public virtual void RemoveCounties(DqeUser account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            foreach (var county in _counties)
            {
                county.MyMarketArea = null;
            }
        }

        public override Transformers.MarketArea GetTransformer()
        {
            return new Transformers.MarketArea
            {
                Id = Id,
                Name = Name
            };
        }

        public override void Transform(Transformers.MarketArea transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Role != DqeRole.System && account.Role != DqeRole.Administrator)
            {
                throw new SecurityException(string.Format("Account role {0} is not authorized for this transaction.", account.Role));
            }
            Name = transformer.Name.Trim();
        }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Market Area Name is required.");
            }
            var ma = _marketAreaRepository.GetByName(Name);
            if (ma != null && ma.Id != Id)
            {
                yield return new ValidationResult(string.Format("There is already a Market Area with the name {0}.", Name));
            }
        }
    }
}
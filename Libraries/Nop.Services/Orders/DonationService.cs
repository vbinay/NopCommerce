using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using System.Collections;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Stores;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Donation Services
    /// </summary>
    public partial class DonationService : IDonationService	/// NU-33
    {
        #region Fields

        private readonly IRepository<Donation> _donationRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreMappingService _storeMappingService; //NU-90

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="donationsRepository">Donation context</param>
        /// <param name="eventPublisher"></param>
        public DonationService(IRepository<Donation> donationRepository, IEventPublisher eventPublisher, IDateTimeHelper dateTimeHelper, IStoreMappingService storeMappingService)
        {
            this._donationRepository = donationRepository;
            _eventPublisher = eventPublisher;
            this._dateTimeHelper = dateTimeHelper;
            this._storeMappingService = storeMappingService; //NU-90
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a donation
        /// </summary>
        /// <param name="Donation">Donation</param>
        public virtual void DeleteDonation(Donation Donation)
        {
            if (Donation == null)
                throw new ArgumentNullException("Donation");

            this._donationRepository.Delete(Donation);

            //event notification
            _eventPublisher.EntityDeleted(Donation);
        }

        /// <summary>
        /// Gets a donation
        /// </summary>
        /// /// <param name="storeId"></param>
        /// <param name="DonationId">Donation identifier</param>
        /// <returns>Donation entry</returns>
        public virtual Donation GetDonationById(int DonationId)
        {
            if (DonationId == 0)
                return null;

            var Donation = this._donationRepository.Table.Where(gc =>
                gc.Id == DonationId).FirstOrDefault();

            return Donation;
        }

        /// <summary>
        /// Gets all Donations
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="PurchasedWithOrderItemId">Associated order ID; null to load all records</param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <returns>Donations</returns>
        public virtual List<Donation> GetDonations(int storeId, int? PurchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null, Boolean? forExport = false)
        {

            var query = this._donationRepository.Table;

            if(startTime != null)
                startTime = new DateTime(startTime.Value.Year, startTime.Value.Month, startTime.Value.Day, 0, 0, 0);

            if(endTime != null)
                endTime = new DateTime(endTime.Value.Year, endTime.Value.Month, endTime.Value.Day, 23, 59, 59);

            if (PurchasedWithOrderItemId.HasValue)
                query = query.Where(mp =>
                    mp.PurchasedWithOrderItem != null &&
                    mp.PurchasedWithOrderItem.Id == PurchasedWithOrderItemId.Value);

            if (!includeProcessed)
                query = query.Where(mp => mp.IsProcessed == false);

            if (startTime.HasValue)
                query = query.Where(mp => startTime.Value <= mp.CreatedOnUtc);

            if (endTime.HasValue)
                query = query.Where(mp => endTime.Value >= mp.CreatedOnUtc);




            List<Donation> donationList = new List<Donation>();

            //NU-90
            foreach (var donation in query.ToList())
            {
                if (_storeMappingService.Authorize(donation.PurchasedWithOrderItem.Product, storeId))
                {
                    donationList.Add(donation);
                }
            }
            //NU-90


            return donationList;         
        }


        /// <summary>
        /// Gets all Donations
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="PurchasedWithOrderItemId">Associated order ID; null to load all records</param>
        /// <param name="includeProcessed"></param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <returns>Donations</returns>
        public virtual List<Donation> GetDonationsForExport(int storeId, int? PurchasedWithOrderItemId, bool includeProcessed, DateTime? startTime = null, DateTime? endTime = null)
        {

            var query = this._donationRepository.Table;//.Where(mp => mp.PurchasedWithOrderItem.SiteProduct.storeId == storeId);

            /* if (PurchasedWithOrderItemId.HasValue)
                 query = query.Where(mp =>
                     mp.PurchasedWithOrderItem != null &&
                     mp.PurchasedWithOrderItem.Id == PurchasedWithOrderItemId.Value);*/

            if (!includeProcessed)
                query = query.Where(mp => mp.IsProcessed == false);

            if (startTime.HasValue)
                query = query.Where(mp => startTime.Value <= mp.CreatedOnUtc);

            if (endTime.HasValue)
                query = query.Where(mp => endTime.Value >= mp.CreatedOnUtc);

            return query.ToList();
        }



        /// <summary>
        /// Inserts a Donation
        /// </summary>
        /// <param name="Donation">Donation</param>
        public virtual void InsertDonation(Donation Donation)
        {
            if (Donation == null)
                throw new ArgumentNullException("Donation");

            this._donationRepository.Insert(Donation);

            //event notification
            _eventPublisher.EntityInserted(Donation);
        }

        /// <summary>
        /// Updates the Donation
        /// </summary>
        /// <param name="Donation">Donation</param>
        public virtual void UpdateDonation(Donation Donation)
        {
            if (Donation == null)
                throw new ArgumentNullException("Donation");

            this._donationRepository.Update(Donation);

            //event notification
            _eventPublisher.EntityUpdated(Donation);
        }


        public virtual IList<Donation> GetDonationsByIds(int[] donationIds)
        {
            if (donationIds == null || donationIds.Length == 0)
                return new List<Donation>();

            var query = from pr in _donationRepository.Table
                        where donationIds.Contains(pr.Id)
                        select pr;
            var donations = query.ToList();
            //sort by passed identifiers
            var sortedDonations = new List<Donation>();
            foreach (int id in donationIds)
            {
                var donation = donations.Find(x => x.Id == id);
                if (donation != null)
                    sortedDonations.Add(donation);
            }
            return sortedDonations;
        }

        #endregion
    }
}

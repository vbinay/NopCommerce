using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Kendoui;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Services.Localization;
using Nop.Services.Stores;
using Nop.Core.Domain.Customers;
using Nop.Services.Logging;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Controllers;

namespace Nop.Admin.Controllers
{
    public class DonationController : BaseAdminController	// NU-33
    {
        #region Fields

        private readonly IDonationService _donationService;
        private readonly IExportManager _exportManager;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;

        #endregion

        #region Constructors

        public DonationController(
            IDonationService donationService,
            IDateTimeHelper dateTimeHelper,
            IExportManager exportManager,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IStoreService storeService)
        {
            _donationService = donationService;
            _exportManager = exportManager;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _permissionService = permissionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _storeService = storeService;
        }

        #endregion

        #region Methods

        // List
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDonations))
                return AccessDeniedView();

            var model = new DonationListModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult DonationsList(DataSourceRequest command, DonationListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDonations))
                return AccessDeniedView();

            var Donations = this.LoadDonations(model, false);

            var gridModel = new DataSourceResult
            {
                Data = Donations.PagedForCommand(command).Select(x =>
                    {
                        var m = x.ToModel();
                        m.Id = x.Id;
                        m.PurchasedWithOrderItemId = x.PurchasedWithOrderItemId;                       
                        m.DonorFirstName = x.DonorFirstName;
                        m.DonorLastName = x.DonorLastName;
                        m.DonorCompany = x.DonorCompany;
                        m.DonorAddress = x.DonorAddress;
                        m.DonorAddress2 = x.DonorAddress2;
                        m.DonorCity = x.DonorCity;
                        m.DonorState = x.DonorState;
                        m.DonorZip = x.DonorZip;
                        m.DonorPhone = x.DonorPhone;
                        m.DonorCountry = x.DonorCountry;
                        m.Comments = x.Comments;
                        m.NotificationEmail = x.NotificationEmail;
                        m.OnBehalfOfFullName = x.OnBehalfOfFullName;
                        m.CreatedOnLocal = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc).ToString("MM/dd/yy hh:mm tt");
                        m.Amount = _priceFormatter.FormatPrice(x.PurchasedWithOrderItem.UnitPriceInclTax, true, false);
                        m.ProductName = x.PurchasedWithOrderItem.Product.Name;
                        return m;
                    }).OrderBy(d => d.ProductName),
                Total = Donations.Count()
            };


            var result = Json(gridModel);
            return result;
        }

        [HttpPost]
        public ActionResult ProcessSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                var donations = _donationService.GetDonationsByIds(selectedIds.ToArray());
                foreach (var donation in donations)
                {
                    donation.IsProcessed = true;
                    _donationService.UpdateDonation(donation);
                }
            }

            return Json(new { Result = true });
        }

        private List<Donation> LoadDonations(DonationListModel model, bool forExport)
        {
            var startDate = (model.StartDate == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            var endDate = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone);
            var includeProcessed = model.IncludeProcessed;

            return _donationService.GetDonations(_storeContext.CurrentStore.Id, null, includeProcessed, startDate, endDate, forExport);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public ActionResult ExportExcelAll(DonationListModel model)
        {
            var donations = this.LoadDonations(model, false);
            try
            {
                var bytes = _exportManager.ExportDonationsToXlsx(donations, false);

                return File(bytes, MimeTypes.TextXlsx, "donations.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        #endregion
    }
}

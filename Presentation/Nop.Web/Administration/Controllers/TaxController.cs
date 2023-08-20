using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Tax;
using Nop.Core.Domain.Tax;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Admin.Controllers
{
    public partial class TaxController : BaseAdminController
    {
        #region Fields

        private readonly ITaxService _taxService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly TaxSettings _taxSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IGlCodeService _glcodeService;

        #endregion

        #region Constructors

        public TaxController(ITaxService taxService,
            ITaxCategoryService taxCategoryService, TaxSettings taxSettings,
            ISettingService settingService, IPermissionService permissionService, IGlCodeService glcodeService)
        {
            this._taxService = taxService;
            this._taxCategoryService = taxCategoryService;
            this._taxSettings = taxSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._glcodeService = glcodeService;
        }

        #endregion

        #region Tax Providers

        public ActionResult Providers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult Providers(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var taxProvidersModel = _taxService.LoadAllTaxProviders()
                .Select(x => x.ToModel())
                .ToList();
            foreach (var tpm in taxProvidersModel)
                tpm.IsPrimaryTaxProvider = tpm.SystemName.Equals(_taxSettings.ActiveTaxProviderSystemName, StringComparison.InvariantCultureIgnoreCase);
            var gridModel = new DataSourceResult
            {
                Data = taxProvidersModel.
                    OrderBy(t => t.FriendlyName), /// NU-24
                Total = taxProvidersModel.Count()
            };

            return Json(gridModel);
        }

        public ActionResult ConfigureProvider(string systemName)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var taxProvider = _taxService.LoadTaxProviderBySystemName(systemName);
            if (taxProvider == null)
                //No tax provider found with the specified id
                return RedirectToAction("Providers");

            var model = taxProvider.ToModel();
            string actionName, controllerName;
            RouteValueDictionary routeValues;
            taxProvider.GetConfigurationRoute(out actionName, out controllerName, out routeValues);
            model.ConfigurationActionName = actionName;
            model.ConfigurationControllerName = controllerName;
            model.ConfigurationRouteValues = routeValues;
            return View(model);
        }

        public ActionResult MarkAsPrimaryProvider(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
            {
                return RedirectToAction("Providers");
            }

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var taxProvider = _taxService.LoadTaxProviderBySystemName(systemName);
            if (taxProvider != null)
            {
                _taxSettings.ActiveTaxProviderSystemName = systemName;
                _settingService.SaveSetting(_taxSettings);
            }

            return RedirectToAction("Providers");
        }

        #endregion

        #region Tax Categories

        public ActionResult Categories()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public ActionResult Categories(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var categoriesModel = _taxCategoryService.GetAllTaxCategories()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = categoriesModel.
                    OrderBy(t => t.Name), /// NU-24
                Total = categoriesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public ActionResult CategoryUpdate(TaxCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var taxCategory = _taxCategoryService.GetTaxCategoryById(model.Id);
            taxCategory = model.ToEntity(taxCategory);
            _taxCategoryService.UpdateTaxCategory(taxCategory);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult CategoryAdd([Bind(Exclude = "Id")] TaxCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var taxCategory = new TaxCategory();
            taxCategory = model.ToEntity(taxCategory);
            _taxCategoryService.InsertTaxCategory(taxCategory);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult CategoryDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var taxCategory = _taxCategoryService.GetTaxCategoryById(id);
            if (taxCategory == null)
                throw new ArgumentException("No tax category found with the specified id");
            _taxCategoryService.DeleteTaxCategory(taxCategory);

            return new NullJsonResult();
        }

        #endregion



        [HttpPost]
        public ActionResult ListGlCodes(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var glCodesModel = _glcodeService.GetAllGlCodes()
                .Select(x => x.ToModel())
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = glCodesModel,
                //.                   OrderBy(t => t.Name), /// NU-24
                Total = glCodesModel.Count
            };

            return Json(gridModel);
        }

        public ActionResult GlCodes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            return View();
        }


        [HttpPost]
        public JsonResult ListTaxResponseStorage(DataSourceRequest request)
        {
           
            var glCodesModels = _taxService.GetTaxResponseStorage(TaxRequestType.RequestQuotationTax);
            var newModel = new List<TaxResponseStorageModel>();
            foreach (var response in glCodesModels)
            {
                var stuff = new TaxResponseStorageModel
                {
                    Id = response.Id,
                    AddedDate = response.AddedDate,
                    CustomerID = response.CustomerID,
                    ProductID = response.ProductID,
                    TypeOfCall = response.TypeOfCall,
                    OrderID = 0,
                    XMlResponse = response.XMlResponse
                };

                newModel.Add(stuff);

                break;
            }


            var gridModel = new DataSourceResult
            {
                Data = newModel,
                Total = newModel.Count
            };

            var json = Json(gridModel, JsonRequestBehavior.AllowGet);
            return json;


        }


        //private IEnumerable<TaxResponseStorageModel> GetProducts()
        //{

        //  //  List<ProductViewModel> collection = new List<ProductViewModel>();

        //    var glCodesModel = _taxService.GetTaxResponseStorage(TaxRequestType.RequestQuotationTax)
        //              .Select(x => x.ToModel()).OrderByDescending(t => t.AddedDate).Take(1)
        //              .ToList();
        //    return glCodesModel.AsEnumerable();
        //}

        public ActionResult TaxResponseStorage()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();



            return View();
        }

    }
}

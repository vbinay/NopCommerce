using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.SAP;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.SAP
{
    public partial class SAPService : ISAPService
    {
        #region Constants
        private const string SAP_ALL_KEY = "Nop.sap.all-{0}-{1}";
        private const string SAP_PATTERN_KEY = "Nop.sap.";
        #endregion

        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<SalesJournal> _salesJournal;
        private readonly IOrderService _orderService;
        private readonly ICacheManager _cacheManager;
        private readonly IDbContext _dbContext;
        private readonly IDataProvider _dataProvider;

        #endregion

        #region Ctor

        public SAPService(IEventPublisher eventPublisher,
            IRepository<SalesJournal> salesJournal,
            IOrderService orderService,
            ICacheManager cacheManager,
            IDbContext dbContext,
            IDataProvider dataProvider)
        {
            this._eventPublisher = eventPublisher;
            this._salesJournal = salesJournal;
            this._orderService = orderService;
            this._cacheManager = cacheManager;
            this._dbContext = dbContext;
            this._dataProvider = dataProvider;
        }

        #endregion

        #region Utilties

        #endregion

        #region Methods

        public void CreateEntry(OrderItem _orderItem, int _status)
        {
            var pOrderItemId = _dataProvider.GetParameter();
            pOrderItemId.ParameterName = "OrderItemId";
            pOrderItemId.Value = _orderItem.Id;
            pOrderItemId.DbType = DbType.Int32;

            var pStatus = _dataProvider.GetParameter();
            pStatus.ParameterName = "Status";
            pStatus.Value = _status;
            pStatus.DbType = DbType.Int32;

            _dbContext.ExecuteSqlCommand("EXEC [dbo].[SAP_SubmitOrderItem] @OrderItemId, @Status", false, 600, pOrderItemId, pStatus);
        }

        public void ExportSalesJournal()
        {
            throw new NotImplementedException();
        }

        public void UploadFiles()
        {
            throw new NotImplementedException();
        }

        public string Test()
        {
            var pOrderItemId = _dataProvider.GetParameter();
            pOrderItemId.ParameterName = "OrderItemId";
            pOrderItemId.Value = 796;
            pOrderItemId.DbType = DbType.Int32;

            var pStatus = _dataProvider.GetParameter();
            pStatus.ParameterName = "Status";
            pStatus.Value = 1;
            pStatus.DbType = DbType.Int32;

            _dbContext.ExecuteSqlCommand("EXEC [dbo].[SAP_SubmitOrderItem] @OrderItemId, @Status", false, 600, pOrderItemId, pStatus);

            return "Records: " + _salesJournal.Table.Count();
        }

        public string Count()
        {
            return "Records: " + _salesJournal.Table.Count();
        }

        #endregion

    }
}

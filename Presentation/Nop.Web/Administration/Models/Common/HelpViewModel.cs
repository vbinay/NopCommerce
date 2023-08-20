using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;

namespace Nop.Admin.Models.Common
{
    public partial class HelpViewModel : BaseNopModel
    {
        private readonly string tourId;

        public HelpViewModel()
        {
            tourId = Guid.NewGuid().ToString();
            this.Steps = new List<HelpStep>();
        }
        public string TourId
        {
            get { return tourId; }
        }

        public List<HelpStep> Steps { get; set; }
        public JRaw OnStart { get; set; }
        public JRaw OnClose { get; set; }
        public JRaw OnEnd { get; set; }
        public string StepsJson
        {
            get
            {
                return JsonConvert.SerializeObject(this.Steps
                                                , Formatting.Indented
                                                , new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            }
        }
    }

  
}
using Autofac;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Routes;
using Nop.Web.Framework.Themes;
using FluentValidation.Mvc;
using System.Web.Mvc;
using Nop.Core.Configuration;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Fakes;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Data;

using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
//using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
//using System.Windows.Forms;
using System.Xml.Linq;
using Nop.Services.Topics;
using Nop.Services.News;
using Nop.Services.Blogs;
using Nop.Services.SAP;
using Smw.Menu.Services;
using System.Globalization;


namespace ConsoleApplication1
{
    class Program
    {

        static void Main(string[] args)
        {
            //string date = "2017-11-09 01:13 PM";
            //DateTime dt = DateTime.ParseExact(date, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
            //if (dt.Date == DateTime.Today)
            //{
            //    //need to get nearest time.
            //    DateTime nearestTime = RoundUp(DateTime.Now, TimeSpan.FromMinutes(15));

            //   string openTime = nearestTime.ToString("hh:mm tt");

            //   string availableTimes = FormatTimes(openTime, "06:00 PM");
            //}


            //   string test = service.GetHeader("https://seminoledining.sodexomyway.com/").Result;

            EngineContext.Initialize(false);
            var _SAPService = EngineContext.Current.Resolve<IStagingService>();
            var _OrderService = EngineContext.Current.Resolve<IOrderService>();

            OrderItem oi = _OrderService.GetOrderItemById(125);







            //_SAPService.CreateEntry(oi, 1);

            //  Console.WriteLine(test);
            Console.Read();

        }

        public static string FormatTimes(string openTime, string closeTime)
        {
            var startTime = DateTime.Parse(openTime);
            startTime = RoundUp(startTime, TimeSpan.FromMinutes(15));
            var endTime = DateTime.Parse(closeTime);
            endTime = RoundUp(endTime, TimeSpan.FromMinutes(15));

            double l = Math.Round(endTime.Subtract(startTime).TotalMinutes / 15.0);
            int maxHours = Convert.ToInt16(Math.Round(l, MidpointRounding.AwayFromZero));

            var clockQuery = from offset in Enumerable.Range(0, maxHours + 1) select TimeSpan.FromMinutes(15 * offset);

            var sb = new StringBuilder();
            foreach (var time in clockQuery)
            {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.AppendFormat("{0}", (startTime + time).ToString("H:mm"));
            }
            return sb.ToString();
        }

        public static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }



    }
}

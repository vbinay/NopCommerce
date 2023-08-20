using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Nop.Web.Framework.Tridion;
//using Nop.Web.Framework.Tridion.HostNameMapping.Services;

namespace Nop.Web.Controllers
{
    public class TridionResourceController : Controller
    {
        private ActionResult GetResource(string fullPath, string mimeType)
        {
            FileInfo file = new FileInfo(fullPath);
            if (file.Exists)
            {
                if (mimeType == "application/pdf" || mimeType == "text/html")
                {
                    AddLastModifiedHeader(file);
                    return new FilePathResult(file.FullName, mimeType);
                }

                if (NotModified(file))
                {
                    return new HttpStatusCodeResult(304); //304 = Not modified
                }
                else
                {
                    AddLastModifiedHeader(file);
                    return new FilePathResult(file.FullName, mimeType);
                }
            }
            else
            {
                return new HttpNotFoundResult(String.Format("File {0} not found", file.Name));
            }
        }

        [HttpGet]
        public ActionResult Css(string fileName)
        {
            string filePath = HostNameContext.ConfigurationManager.CssPath + fileName;
            return GetResource(filePath, "text/css");
        }
        [HttpGet]
        public ActionResult Js(string fileName)
        {
            string filePath = HostNameContext.ConfigurationManager.JsPath + fileName;
            return GetResource(filePath, "text/javascript");
        }
        [HttpGet]
        public ActionResult Images(string fileName)
        {
            string filePath = HostNameContext.ConfigurationManager.MultimediaPath + fileName;
            return GetResource(filePath, MimeTypeMap.GetMimeType(Path.GetExtension(filePath)));
        }

        private void AddLastModifiedHeader(FileInfo file)
        {
            Response.AddHeader("Last-Modified", file.LastWriteTimeUtc.ToString("R")); // DateTime.Now.ToUniversalTime().ToString("R")); //DateTime.Now also technically works, less semantic though 
        }
        private bool NotModified(FileInfo file)
        {
            string ifModifiedSinceStr = HttpContext.Request.Headers["If-Modified-Since"];
            DateTime ifModifiedSince;
            if (!String.IsNullOrEmpty(ifModifiedSinceStr) && DateTime.TryParse(ifModifiedSinceStr, out ifModifiedSince))
            {
                ifModifiedSince = ifModifiedSince.ToUniversalTime();

                DateTime fileLastModified = file.LastWriteTimeUtc;

                return fileLastModified > ifModifiedSince;
            }

            return false; //Browser is requesting a new image, not conditional on modified date
        }
    }
}
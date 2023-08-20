using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;
using HtmlAgilityPack;
using System.Web.Http;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Web;

namespace Smw.Menu.Services
{

    public class HtmlAgilityService
    {

        public string hostname { get; set; }
        public string refPage { get; set; }
        public string pageTitle { get; set; }

        private readonly HttpContextBase _httpContext;

        public HtmlAgilityService()
        {

        }

        public HtmlAgilityService(HttpContextBase httpContext)
        {
            this._httpContext = httpContext;
        }

        public async Task<List<string>> GetHead(string refer)
        {

            List<string> innerHTML = new List<string>();
            pageTitle = "Menus";
            refPage = refer;
            Uri uri = new Uri(refPage);
            hostname = uri.Scheme + "://" + uri.Host;

            var html = @refPage;

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);


            try
            {

                var node = htmlDoc.DocumentNode.SelectSingleNode("//head");
                // var title = htmlDoc.DocumentNode.SelectSingleNode("//title");
                //title.InnerHtml = pageTitle;

                foreach (HtmlNode css in htmlDoc.DocumentNode.SelectNodes("/*/head/link/@href"))
                {
                    string srcValue = css.GetAttributeValue("href", "");

                    bool cssRemoved = false;
                    //srcValue = srcValue.Replace("/dist/", HOSTNAME + "dist/");
                    if (srcValue.StartsWith("/dist/"))
                    {
                        cssRemoved = true;
                        css.Remove();
                    }

                    if (!cssRemoved)
                    {
                        innerHTML.Add(css.OuterHtml);
                    }

                    //css.SetAttributeValue("href", srcValue);
                    // Console.WriteLine(css.InnerHtml);               
                }


                HtmlNode meta = htmlDoc.CreateElement("meta");
                node.AppendChild(meta);
                meta.SetAttributeValue("property", "refer");
                meta.SetAttributeValue("content", refPage);


                //HtmlNode mainJS = htmlDoc.CreateElement("<script src=\"//content-service.sodexomyway.com/system/assets/js/main.min.js?url=" + refer.Replace("https", "http") + "\"></script>");
                //innerHTML.Add(mainJS.OuterHtml);
                //node.AppendChild(mainJS);

                // HtmlNode link = htmlDoc.CreateElement("link");
                // node.AppendChild(link);
                // link.SetAttributeValue("rel", "stylesheet");
                // link.SetAttributeValue("href", "https://fillupmyfridge.com/bite/bite.css");


                // HtmlNode link1 = htmlDoc.CreateElement("link");
                // node.AppendChild(link);
                // link1.SetAttributeValue("rel", "stylesheet");
                // link1.SetAttributeValue("href", "https://fillupmyfridge.com/bite/nf.css");


                foreach (HtmlNode scripts in htmlDoc.DocumentNode.SelectNodes("/*/head/script/@src"))
                {
                    innerHTML.Add(scripts.OuterHtml);
                    //Console.WriteLine(scripts.InnerHtml);
                }


                // Console.WriteLine("Node Name: " + node.Name + "\n" + node.InnerHtml);


                // string response = node.InnerHtml;


            }
            catch (Exception ex)
            {
                innerHTML = new List<string>();
            }

            return innerHTML;
        }


        public async Task<string> GetHeader(string url, string shopUrl)
        {


            refPage = url;
            //refPage = "https://cpsfacilities.sodexomyway.com/no-auth";
            Uri uri = new Uri(refPage);
            hostname = uri.Scheme + "://" + uri.Host;

            var html = @refPage;

            HtmlWeb web = new HtmlWeb();
            string response = "";
            try
            {
                web.UserAgent = "Sodexoagent";
                var htmlDoc = web.Load(html);
                if (hostname.Contains("staging-nmsu"))
                {
                    var header = htmlDoc.DocumentNode.SelectSingleNode("//html/body");
                    //header.SetAttributeValue("style", "opacity:0 !important");
                    header.SelectSingleNode("//html/body/div[2]").Remove();
                    header.SelectSingleNode("//html/body/div[1]/div/div[2]").Remove();
                    header.SelectSingleNode("//div[@class='header_group']").SetAttributeValue("style", "float:none;padding-left:69%");
                    // header.SelectSingleNode("//span[@class='main_nav_toggle_icon']/svg/use").SetAttributeValue("href",hostname);

                    var headerMenuIcon = header.SelectSingleNode("//svg");
                    foreach (HtmlNode href1 in headerMenuIcon.Descendants("use"))
                    {
                        HtmlAttribute att = href1.Attributes["xlink:href"];
                        if (att.Name.Contains("xlink:href"))
                        {
                            att.Value = hostname + att.Value;
                        }
                    }

                    var div1 = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='main_nav_list']");
                    if (div1 != null)
                    {
                        foreach (HtmlNode href in div1.Descendants("a"))
                        {

                            HtmlAttribute att = href.Attributes["href"];

                            if (att.Value.Contains("https") || att.Value.Contains("http"))
                            {

                                if (att.Value.Contains("com/shop"))
                                    att.Value = shopUrl;
                                else
                                    att.Value = att.Value;
                            }
                            else
                            {
                                if (att.Value.Contains("shop"))
                                {
                                    att.Value = shopUrl;
                                    if (att.Value.Contains("com/shop"))
                                        att.Value = att.Value.TrimEnd('p').TrimEnd('o').TrimEnd('h').TrimEnd('s').TrimEnd('/');
                                }
                                else
                                {
                                    att.Value = hostname + att.Value;
                                }
                            }
                        }

                    }
                    header.SelectSingleNode("//html/body/div");
                    return header.OuterHtml;
                }
                else
                {
                    var header = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div/div/header");

                    header.SelectSingleNode("//html/body/div/div/header/div[1]").SetAttributeValue("style", "text-align: left;-webkit-box-sizing: content-box;"); //main Panel
                    header.SelectSingleNode("//html/body/div/div/header/nav").SetAttributeValue("style", "text-align: left;-webkit-box-sizing: content-box;"); //main Panel

                    if (header.SelectSingleNode("//a[@class='signin']") != null)
                    {
                        header.SelectSingleNode("//a[@class='signin']").Remove(); //Remove Signin
                    }

                    if (!hostname.Contains("cps") && !hostname.Contains("loganlodge") && !hostname.Contains("umwcatering") && !hostname.Contains("armourhousecatering.sodexomyway.com") && !hostname.Contains("conferences") && !hostname.Contains("cafe1883"))
                        header.SelectSingleNode("//span[@class='fa fa-shopping-bag']").Remove();//.Attributes["class"].Value = "fa fa-search-plus"; //Add Searchbox

                    var div1 = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='inline-dropdown-nav']");
                    if (div1 != null)
                    {

                        foreach (HtmlNode href in div1.Descendants("a"))
                        {

                            HtmlAttribute att = href.Attributes["href"];
                            if (att.Value.Contains("https") || att.Value.Contains("http"))
                            {
                                if (att.Value.Contains("com/shop"))
                                    att.Value = shopUrl;
                                else
                                    att.Value = att.Value;
                            }
                            else
                            {
                                if (att.Value.Contains("shop"))
                                {
                                    att.Value = shopUrl;
                                    if (att.Value.Contains("com/shop"))
                                        att.Value = att.Value.TrimEnd('p').TrimEnd('o').TrimEnd('h').TrimEnd('s').TrimEnd('/');
                                }
                                else
                                {
                                    att.Value = hostname + att.Value;
                                }
                            }
                        }

                    }
                    //Mostly for Conference Sites
                    var div2 = htmlDoc.DocumentNode.SelectSingleNode("//ul[@class='inline-dropdown-nav no-cart']");
                    if (div2 != null)
                    {

                        foreach (HtmlNode href in div2.Descendants("a"))
                        {

                            HtmlAttribute att = href.Attributes["href"];
                            if (att.Value.Contains("https") || att.Value.Contains("http"))
                            {
                                if (att.Value.Contains("com/shop"))
                                    att.Value = shopUrl;
                                else
                                    att.Value = att.Value;
                            }
                            else
                            {
                                if (att.Value.Contains("shop"))
                                {
                                    att.Value = shopUrl;
                                    if (att.Value.Contains("com/shop"))
                                        att.Value = att.Value.TrimEnd('p').TrimEnd('o').TrimEnd('h').TrimEnd('s').TrimEnd('/');
                                }
                                else
                                {
                                    att.Value = hostname + att.Value;
                                }
                            }
                        }

                    }
                    var div = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='nav-links']");
                    if (div != null)
                    {
                        foreach (HtmlNode href in div.Descendants("a"))
                        {
                            HtmlAttribute att = href.Attributes["href"];
                            if (att.Value.Contains("https") || att.Value.Contains("http"))
                            {
                                if (att.Value.Contains("com/shop"))
                                    att.Value = shopUrl;
                                else
                                    att.Value = att.Value;
                            }
                            else
                            {
                                if (att.Value.Contains("shop"))
                                {
                                    att.Value = shopUrl;
                                    if (att.Value.Contains("com/shop"))
                                        att.Value = att.Value.TrimEnd('p').TrimEnd('o').TrimEnd('h').TrimEnd('s').TrimEnd('/');
                                }
                                else
                                {
                                    att.Value = hostname + att.Value;
                                }
                            }
                        }
                    }
                    var logoLink = htmlDoc.DocumentNode.SelectSingleNode("//header/div[1]/div/a");
                    HtmlAttribute attribute = logoLink.Attributes["href"];


                    if (hostname.Contains("findlay"))
                    {
                        attribute.Value = "https://shop-findlay.sodexomyway.com";
                    }
                    else
                    {
                        attribute.Value = hostname;
                        if (hostname.Contains("cps") || hostname.Contains("loganlodge") || hostname.Contains("umwcatering") || hostname.Contains("armourhousecatering.sodexomyway.com") || hostname.Contains("conferences") || hostname.Contains("cafe1883"))
                        {
                            var logoLink1 = htmlDoc.DocumentNode.SelectSingleNode("//header/div[1]/h1/a");
                            HtmlAttribute attribute1 = logoLink1.Attributes["href"];
                            attribute1.Value = hostname;
                        }
                    }
                    if (hostname.Contains("muhlenberg") || hostname.Contains("mberg"))
                    {
                        attribute.Value = "https://dining.muhlenberg.edu/";
                    }
                    if (hostname.Contains("stophunger"))
                    {
                        attribute.Value = "http://us.stop-hunger.org";
                    }
                    if (hostname.Contains("ebrg"))
                    {
                        attribute.Value = "https://shop-ebrg.sodexomyway.com";
                    }
                    else
                    {
                        attribute.Value = hostname;
                        if (hostname.Contains("cps") || hostname.Contains("loganlodge") || hostname.Contains("umwcatering") || hostname.Contains("armourhousecatering.sodexomyway.com") || hostname.Contains("conferences") || hostname.Contains("cafe1883"))
                        {
                            var logoLink1 = htmlDoc.DocumentNode.SelectSingleNode("//header/div[1]/h1/a");
                            HtmlAttribute attribute1 = logoLink1.Attributes["href"];
                            attribute1.Value = hostname;
                        }
                    }

                    var startNode = htmlDoc.DocumentNode.SelectSingleNode("//comment()[contains(., 'Google Tag Manager')]");
                    var endNode = htmlDoc.DocumentNode.SelectSingleNode("//comment()[contains(., 'End Google Tag Manager')]");
                    int startNodeIndex = startNode.ParentNode.ChildNodes.IndexOf(startNode);
                    int endNodeIndex = endNode.ParentNode.ChildNodes.IndexOf(endNode);

                    var headforGTM = startNode.ParentNode.ChildNodes.Where((n, index) => index >= startNodeIndex && index <= endNodeIndex).Select(n => n);

                    //var headforGTM = htmlDoc.DocumentNode.SelectSingleNode("//html/head");
                    //headforGTM.SelectSingleNode("//comment()[contains(., 'Google Tag Manager')]");

                    //header.SelectSingleNode("//span[@class='ecomm-total-num margin-0']").Remove(); //Remove Ecomm Count

                    //var newNode = HtmlNode.CreateNode("<div class=\"shopping-bag-list hide\" style=\"width:400px\"><div class=\"search-box store-search-box\">" + godHelpMe + " </div>        </div>");

                    //var shoppinBag = header.SelectSingleNode("//div[@class='shopping-bag-list hide']");

                    //shoppinBag.ParentNode.ReplaceChild(newNode, shoppinBag);              


                    //response = "<script>" + headforGTM.Where(x => x.Name == "script").FirstOrDefault().InnerHtml + "</script>" + header.OuterHtml;

                    response = header.OuterHtml;
                }

            }
            catch (Exception ex)
            {
                response = "";
            }

            return response;
        }




        public async Task<string[]> GetBodyStart(string contentTitle)
        {

            var html = @refPage;

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(html);

            var body = htmlDoc.DocumentNode.SelectSingleNode("//html/body");

            foreach (HtmlNode node in body.SelectNodes("//img | //a"))
            {

                if (node.Name == "img")
                {
                    string srcVal = node.GetAttributeValue("src", "");

                    if (srcVal.StartsWith("/dist/"))
                    {
                        node.Attributes["src"].Value = modifyAttrPath(srcVal, hostname);
                    }
                }
                else
                {
                    string hrefVal = node.GetAttributeValue("href", "");

                    if (hrefVal != "" && !hrefVal.StartsWith("http") && !hrefVal.StartsWith("//") && !hrefVal.StartsWith("tel:") && !hrefVal.StartsWith("#"))
                    {
                        node.Attributes["href"].Value = modifyAttrPath(hrefVal, hostname);
                    }
                    //replace menu link with bite link
                    if (hrefVal.Contains("smw-bite.azurewebsites.net"))
                    {
                        node.Attributes["href"].Value = "http://txpl.us/gtt";
                        HtmlNode buttonTxt = node.SelectSingleNode("./p");
                        buttonTxt.InnerHtml = "Open Menu in Bite app";
                    }
                }

                //css.SetAttributeValue("href", srcValue);
                // Console.WriteLine(css.InnerHtml);
            }

            var startNode = htmlDoc.DocumentNode.SelectSingleNode("//comment()[contains(., 'Google Tag Manager (noscript)')]");
            var endNode = htmlDoc.DocumentNode.SelectSingleNode("//comment()[contains(., 'End Google Tag Manager (noscript)')]");
            int startNodeIndex = startNode.ParentNode.ChildNodes.IndexOf(startNode);
            int endNodeIndex = endNode.ParentNode.ChildNodes.IndexOf(endNode);

            var bodyforGTM = startNode.ParentNode.ChildNodes.Where((n, index) => index >= startNodeIndex && index <= endNodeIndex).Select(n => n);

            //var h1 = body.SelectSingleNode("/html/body/div/div/div[2]/div/div/div");
            //if (html.Contains("meal-plan"))
            //{
            var h1 = body.SelectSingleNode("//div[@class='hero-content-left']/h1");
            h1.InnerHtml = pageTitle;
            //}

            //var shoppingCart = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div[1]/div/header/div[1]/div[1]/a[1]").InnerHtml = "";
            var cart = body.SelectSingleNode("//a[@class='ecomm']");//.Attributes.Add("class","hide");
            cart.Attributes["class"].Value = "hide";
            //NOTE: CREATE EMPTY PAGE IN SITE TYPE AS DEFAULT SCRAPE
            //var content = body.SelectSingleNode("./div/div/div[3]/div[1]");
            string className = "main-content";
            var content = body.SelectSingleNode("//div[@class='" + className + "']");

            string nodeStr = "<div class=\"" + className + "\">"; //</div>
            var newNode = HtmlNode.CreateNode(nodeStr);
            content.ParentNode.ReplaceChild(newNode, content);


            //string[] split = body.InnerHtml.Split(new[] { nodeStr }, StringSplitOptions.None);            
            string[] split = Regex.Split("<noscript>" + bodyforGTM.Where(x => x.Name == "script").FirstOrDefault().InnerHtml + "</noscript>" + body.InnerHtml, @"(?<=" + nodeStr + ")");

            //content.InnerHtml = "TEST";
            // Console.WriteLine(body);
            //IEnumerable<HtmlNode> nodes = body.Descendants().TakeWhile(n => n != bodyEnd).Where(n => n.NodeType == HtmlNodeType.Element);
            //FIND THE BOTTOM-HALF
            //REPLACE MAIN-CONTENT with 

            //string response = body.InnerHtml;
            //return response;
            return split;
        }


        public async Task<string> GetBodyEnd()
        {

            var html = @"http://fsu.sodexomyway.com/meal-plans/index";

            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(html);

            var body = htmlDoc.DocumentNode.SelectSingleNode("//html/body");


            var content = body.SelectSingleNode("//html/body/div/div/div[3]/div[1]");


            content.InnerHtml = "TEST";
            // Console.WriteLine(body);
            //IEnumerable<HtmlNode> nodes = body.Descendants().TakeWhile(n => n != bodyEnd).Where(n => n.NodeType == HtmlNodeType.Element);
            //FIND THE BOTTOM-HALF
            //REPLACE MAIN-CONTENT with 




            string response = body.InnerHtml;
            return response;
        }
        static IEnumerable<HtmlNode> FilteredTakeWhile(
            HtmlNode root,
            Func<HtmlNode, bool> predicate,
            Func<HtmlNode, bool> takePredicate)
        {
            for (var currentNode = root.NextSibling;
                 currentNode != null && takePredicate(currentNode);
                 currentNode = currentNode.NextSibling)
            {
                if (predicate(currentNode))
                    yield return currentNode;
            }
        }

        public string modifyAttrPath(string src, string insert, bool append = false)
        {
            string path = (append == false) ? insert + src : src + '?' + insert;

            return path;
        }

        public async Task<string> GetFooter(string url)
        {
            refPage = url;

            Uri uri = new Uri(refPage);
            hostname = uri.Scheme + "://" + uri.Host;


            var html = @refPage;

            HtmlWeb web = new HtmlWeb();
            string response = "";
            try
            {
                var htmlDoc = web.Load(html);

                var header = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div/div/div/div/div/div/div/div/div");

                header.SelectSingleNode("//html/body/div/div/div/footer[1]").SetAttributeValue("style", "text-align: left;-webkit-box-sizing: content-box;"); //main Panel
                header.SelectSingleNode("//html/body/div/div/div/footer[2]").SetAttributeValue("style", "text-align: right;-webkit-box-sizing: content-box;"); //main Panel

                //if (header.SelectSingleNode("//a[@class='signin']") != null)
                //{
                //    header.SelectSingleNode("//a[@class='signin']").Remove(); //Remove Signin
                //}

                //  header.SelectSingleNode("//span[@class='fa fa-shopping-bag']").Remove();//.Attributes["class"].Value = "fa fa-search-plus"; //Add Searchbox

                // var div = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='nav-links']");
                var div = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div/div/div/footer");
                if (div != null)
                {
                    foreach (HtmlNode href in div.Descendants("a"))
                    {
                        //if (href.InnerText != "Shop")
                        //{
                        HtmlAttribute att = href.Attributes["href"];
                        if (att.Value.Contains("https") || att.Value.Contains("http"))
                            att.Value = att.Value;
                        else
                        {
                            att.Value = hostname + att.Value;
                        }
                        //}
                    }
                }
                var logoLink = htmlDoc.DocumentNode.SelectSingleNode("//footer");
                HtmlAttribute attribute = logoLink.Attributes["href"];
                attribute.Value = hostname;

                //var headforGTM = htmlDoc.DocumentNode.SelectSingleNode("//html/head");
                //headforGTM.SelectSingleNode("//comment()[contains(., 'Google Tag Manager')]");

                //header.SelectSingleNode("//span[@class='ecomm-total-num margin-0']").Remove(); //Remove Ecomm Count

                //var newNode = HtmlNode.CreateNode("<div class=\"shopping-bag-list hide\" style=\"width:400px\"><div class=\"search-box store-search-box\">" + godHelpMe + " </div>        </div>");

                //var shoppinBag = header.SelectSingleNode("//div[@class='shopping-bag-list hide']");

                //shoppinBag.ParentNode.ReplaceChild(newNode, shoppinBag);

                response = header.OuterHtml;
            }
            catch (Exception ex)
            {
                response = "";
            }

            return response;
        }


        public async Task<string> GetCompanyLogo(string url)
        {
            string response = "";

            refPage = url;

            Uri uri = new Uri(refPage);
            hostname = uri.Scheme + "://" + uri.Host;

            var html = @refPage;

            HtmlWeb web = new HtmlWeb();
            try
            {
                var htmlDoc = web.Load(html);

                var header = htmlDoc.DocumentNode.SelectSingleNode("//html/body/div/div/header/div/div/a/img");

                HtmlAttribute attribute = header.Attributes["src"];
                attribute.Value = hostname;

                response = attribute.Value;
            }
            catch (Exception ex)
            {

            }


            return response;
        }
    }


}

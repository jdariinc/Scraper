using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scraper.Models;
using HtmlAgilityPack;
using System.Text;
using System.Net;
using System.Net.Mime;
using System.IO;

namespace Scraper.Controllers
{
    public class ImageScraperController : Controller
    {
        // GET: ImageScraper
        public ActionResult Index()
        {
            string view = "ImageScraper";
            return View(view);
        }

        public ActionResult ImageScraper()
        {
            string view = "ImageScraper";
            return View(view);
        }

        [HttpPost]
        public JsonResult SubmitInformation(string url)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                var ResponseStream = response.GetResponseStream();

                HtmlDocument document = new HtmlDocument();
                //document.Load(ResponseStream);

                var reader = new StreamReader(response.GetResponseStream());
                document.LoadHtml(reader.ReadToEnd());

                string imgSrc = string.Empty;
                var ogMeta = document.DocumentNode.SelectNodes("//meta[@property]");

                //Check if contain Open graph element
                if (ogMeta != null)
                {
                    var ogImage = document.DocumentNode.SelectNodes("//meta[@property]").Where(x => x.Attributes["property"].Value == "og:image");
                    if (ogImage.Count() > 0) //check og:image found
                    {
                        Image image = new Image
                        {
                            ImageData = string.Concat("<li><img src=", ogImage.FirstOrDefault().Attributes["content"].Value, " /></li>")
                        };
                        return Json(image);
                    }                        
                    else  //return some images
                    {
                        Image image = new Image
                        {
                            ImageData = GetImages(document.DocumentNode.SelectNodes("//img"), url)
                        };
                        return Json(image);
                    }
                        
                }
                else
                {
                    Image image = new Image
                    {
                        ImageData = GetImages(document.DocumentNode.SelectNodes("//img"), url)
                    };
                    return Json(image);
                }

            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message, MediaTypeNames.Text.Plain);
            }
        }

        private string GetImages(HtmlNodeCollection DOM, string url)
        {
            StringBuilder Images = new StringBuilder();
            if (DOM != null)
            {
                foreach (var img in DOM)
                {
                    string outterHtml = img.OuterHtml;

                    if (img.Attributes["src"] == null)
                        continue;

                    if(img.Attributes["src"].Value.Contains(url) == false && img.Attributes["src"].Value.Contains(".com") == false && img.Attributes["src"].Value.Contains(".net") == false)
                    {
                        if(url.EndsWith("/") == false && img.Attributes["src"].Value.StartsWith("/") == false)
                        {
                            url = url += "//";
                        }

                        img.SetAttributeValue("src", url + img.Attributes["src"].Value);
                        outterHtml = img.OuterHtml;
                    }

                    Images.Append("<li>");
                    try
                    {
                        Images.Append(outterHtml);
                    }
                    catch(Exception ex)
                    {
                        int x = 1;
                        x = 2;
                    }
                    Images.Append("</li>");
                }
            }
            return Images.ToString();
        }
    }
}
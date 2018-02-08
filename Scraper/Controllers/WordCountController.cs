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
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace Scraper.Controllers
{
    public class WordCountController : Controller
    {
        // GET: WordCount
        // GET: ImageScraper
        public ActionResult Index()
        {
            string view = "WordCount";
            return View(view);
        }

        public ActionResult WordCount()
        {
            string view = "WordCount";
            return View(view);
        }

        [HttpPost]
        public JsonResult SubmitInformation(string url)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                WebResponse webResponse = webRequest.GetResponse();
                Stream data = webResponse.GetResponseStream();
                string html = String.Empty;
                using (StreamReader sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                    html = getText(html);

                }

                var wordCount = GetWordOccurences(html);

                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                var ResponseStream = response.GetResponseStream();

                HtmlDocument document = new HtmlDocument();
                document.Load(ResponseStream);
                string imgSrc = string.Empty;

                Image image = new Image
                {
                    ImageData = GetImages(document.DocumentNode.SelectNodes("//img"))
                };
                return Json(wordCount);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(ex.Message, MediaTypeNames.Text.Plain);
            }
        }

        private string getText (string htmlText)
        {
            // Where m_whitespaceRegex is a Regex with [\s].
            // Where sampleHtmlText is a raw HTML string.

            Regex m_whitespaceRegex = new Regex(@"\s");

            var extractedSampleText = new StringBuilder();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlText);

            if (doc != null && doc.DocumentNode != null)
            {
                foreach (var script in doc.DocumentNode.Descendants("script").ToArray())
                {
                    script.Remove();
                }

                foreach (var style in doc.DocumentNode.Descendants("style").ToArray())
                {
                    style.Remove();
                }

                var allTextNodes = doc.DocumentNode.SelectNodes("//text()");
                if (allTextNodes != null && allTextNodes.Count > 0)
                {
                    foreach (HtmlNode node in allTextNodes)
                    {
                        extractedSampleText.Append(" " + node.InnerText);
                    }
                }

                var finalText = m_whitespaceRegex.Replace(extractedSampleText.ToString(), " ");
                return finalText;
            }

            return string.Empty;
        }

        private string GetImages(HtmlNodeCollection DOM)
        {
            StringBuilder Images = new StringBuilder();
            if (DOM != null)
            {
                foreach (var img in DOM)
                {
                    Images.AppendFormat("<li>");
                    Images.AppendFormat(img.OuterHtml);
                    Images.AppendFormat("</li>");
                }
            }
            return Images.ToString();
        }

        private Dictionary<string, int> GetWordOccurences(string inputString)
        {

            // Convert our input to lowercase
            inputString = inputString.ToLower();

            // Define characters to strip from the input and do it
            string[] stripChars = { ";", ",", ".", "-", "_", "^", "(", ")", "[", "]",
                        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "\n", "\t", "\r" };
            foreach (string character in stripChars)
            {
                inputString = inputString.Replace(character, "");
            }

            // Split on spaces into a List of strings
            List<string> wordList = inputString.Split(' ').ToList();

            // Define and remove stopwords
            string[] stopwords = new string[] { "and", "the", "she", "for", "this", "you", "but" };
            foreach (string word in stopwords)
            {
                // While there's still an instance of a stopword in the wordList, remove it.
                // If we don't use a while loop on this each call to Remove simply removes a single
                // instance of the stopword from our wordList, and we can't call Replace on the
                // entire string (as opposed to the individual words in the string) as it's
                // too indiscriminate (i.e. removing 'and' will turn words like 'bandage' into 'bdage'!)
                while (wordList.Contains(word))
                {
                    wordList.Remove(word);
                }
            }

            // Create a new Dictionary object
            Dictionary<string, int> dictionary = new Dictionary<string, int>();

            // Loop over all over the words in our wordList...
            foreach (string word in wordList)
            {
                // If the length of the word is at least three letters...
                if (word.Length >= 3)
                {
                    // ...check if the dictionary already has the word.
                    if (dictionary.ContainsKey(word))
                    {
                        // If we already have the word in the dictionary, increment the count of how many times it appears
                        dictionary[word]++;
                    }
                    else
                    {
                        // Otherwise, if it's a new word then add it to the dictionary with an initial count of 1
                        dictionary[word] = 1;
                    }

                } // End of word length check

            } // End of loop over each word in our input

            // Create a dictionary sorted by value (i.e. how many times a word occurs)
            var sortedDict = (from entry in dictionary orderby entry.Value descending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

            return ConvertDictionary(sortedDict);
        }

        private Dictionary<string, TValue> ConvertDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            Dictionary<string, TValue> newDict = new Dictionary<string, TValue>();
            int count = 1;

            foreach (TKey key in dict.Keys)
            {
                newDict.Add(key.ToString(), dict[key]);
                count++;
                if (count > 10)
                {
                    break;
                }
            }
            return newDict;
        }
    }
}
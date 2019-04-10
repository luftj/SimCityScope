using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SimCityScope
{
    static class XmlHandler
    {
        public static Dictionary<string, int> getCosts()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("../../../../magicnumbers.xml");   // todo: NONONONONO!!

            Dictionary<string, int> costs = new Dictionary<string, int>();

            XmlNode node = doc.DocumentElement.SelectSingleNode("/numbers/costs");
            foreach (XmlNode n in node.ChildNodes)
            {
                string text = n.InnerText; //or loop through its children as well
                Console.WriteLine("xml "+ n.Name + ":"+ text);
                costs[n.Name] = int.Parse(text);
            }

            return costs;
            //string text = node.InnerText;
            //string attr = node.Attributes["theattributename"]?.InnerText
        }
    }
}

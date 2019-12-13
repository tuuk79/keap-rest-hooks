using keap_rest_hooks.Models;
using System;
using System.Configuration;
using System.IO;

namespace keap_rest_hooks.Managers
{
    public class LogFileManager
    {
        public void writeToLogFile(string eventKey, Order order)
        {
            string path = ConfigurationManager.AppSettings["LogDirectory"];

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, $"dailylog_{DateTime.Now.ToString("yyyyMMdd")}.csv");

            if (System.IO.File.Exists(path))
            {
                using (var sw = File.AppendText(path))
                {
                    writeDetails(sw, eventKey, order);
                }
            }
            else
            {
                using (var sw = File.CreateText(path))
                {
                    writeDetails(sw, eventKey, order);
                }
            }
        }

        public void writeDetails(StreamWriter sw, string eventKey, Order order)
        {
            sw.WriteLine(DateTime.Now);
            sw.WriteLine(eventKey);
            sw.WriteLine(order.contact.email);

            foreach (OrderItem orderItem in order.order_items)
            {
                sw.WriteLine(orderItem.product.sku);
            }

            sw.WriteLine();
        }







    }
}
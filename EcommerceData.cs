using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCommerce.EntitiesV2;

namespace EcommerceDataModel
{
    public class TransactionProduct
    {
        public int id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }


        public string toFaceBookJSON() {

            return ("{ id: '" + this.id + "', quantity: "+ this.quantity + "}");
        }

        public TransactionProduct()
        {
        }
        public TransactionProduct(OrderLine item)
        {
            this.id = item.Id;
            this.sku = item.Sku;
            this.name = item.ProductName;
            this.category = "product";
            this.price = (double)item.Price;
            this.quantity = item.Quantity;
        }

    }

    public class EcommerceData
    {
        public string transactionId { get; set; }
        public string transactionAffiliation { get; set; }
        public double transactionTotal { get; set; }
        public double transactionTax { get; set; }
        public int transactionShipping { get; set; }
        public List<TransactionProduct> transactionProducts { get; set; }

        public EcommerceData()
        {
        }

        public EcommerceData(PurchaseOrder po)
        {
            this.transactionId = po.OrderNumber;
            this.transactionTotal = 0;
            this.transactionShipping = 0;
            this.transactionAffiliation = "";

            foreach (Shipment sm in po.Shipments)
            {
                this.transactionShipping += (int)sm.ShipmentPrice;
            }

            this.transactionProducts = new List<TransactionProduct>();
            foreach (OrderLine item in po.OrderLines)
            {
                TransactionProduct orderLine = new TransactionProduct(item);
                this.transactionProducts.Add(orderLine);
                this.transactionTotal += (orderLine.price * orderLine.quantity);
            }

            // NB! VAT HARDCODED 
            this.transactionTax = this.transactionTotal * 0.25;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public string ToMerchantScript()
        {
            return "<script>window.dataLayer = window.dataLayer || []; dataLayer.push(" + this.ToString() + ")</script>";
        }
        public string ToFaceBookScript()
        {
            double priceTotal = this.transactionProducts.Sum(i => (i.price * i.quantity));

            string script = @"
                <script>
                fbq('track', 'Purchase',
                  {{
                    value: {0},
                    currency: 'DKK',
                    contents:
                      [
                        {1}
                      ],
                    content_type: 'product',
                  }}
                );
               </script>";

            string faceBookJSON = string.Join(",", this.transactionProducts.Select(i => i.toFaceBookJSON()).ToArray());

            return string.Format(script, priceTotal.ToString("0.00"), faceBookJSON);
        }

    }

}

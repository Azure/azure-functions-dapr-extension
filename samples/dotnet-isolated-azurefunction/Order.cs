namespace dotnet_isolated_azurefunction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Order
    {
        public Data? Data { get; set; }
    }

    public class Data
    {
        public int? OrderId { get; set; }
    }
}

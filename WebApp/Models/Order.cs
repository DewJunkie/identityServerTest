using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Order
    {
        public Order()
        {
            Widgets = new HashSet<Widget>();
        }
        public int Id { get;set; }
        public DateTime OrderTime { get;set; }

        public IEnumerable<Widget> Widgets { get; set; }
    }
}

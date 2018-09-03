using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class InfoDomain
    {
        public ICollection<Info> Tasks { get; set; }
        public string Message { get; set; }
    }
}

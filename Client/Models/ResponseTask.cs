using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ResponseTask
    {
        public ICollection<Result> Tasks { get; set; }
    }
}

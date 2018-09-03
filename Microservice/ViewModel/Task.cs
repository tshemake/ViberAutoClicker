using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microservice.ViewModel
{
    public class AddTask
    {
        public ICollection<AddDomain> Dimains { get; set; }
    }
    public class InfoTask
    {
        public ICollection<InfoDomain> Dimains { get; set; }
    }
    public class ResponeTask
    {
        public ICollection<Result> Tasks { get; set; }
    }
}
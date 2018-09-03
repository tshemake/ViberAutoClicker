using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microservice.Models;

namespace Microservice.ViewModel
{
    public class AddDomain
    {
        public ICollection<string> Phones { get; set; }
        public string Message { get; set; }
    }

    public class Info
    {
        public Guid Id { get; set; }
        public string Phone { get; set; }

        public static explicit operator Info(Message msg)
        {
            return new Info { Id = msg.Id, Phone = msg.PhoneNumber };
        }
    }

    public class Result
    {
        public Guid Id { get; set; }
        public Guid StatusId { get; set; }

        public static explicit operator Result(Message msg)
        {
            return new Result { Id = msg.Id, StatusId = msg.StatusId };
        }
    }

    public class InfoDomain
    {
        public ICollection<Info> Tasks { get; set; }
        public string Message { get; set; }
    }
}
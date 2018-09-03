using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Microservice.Models
{
    public class History
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public Guid TaskId { get; set; }
        public Message Task { get; set; }
        [Required]
        public Guid StatusId { get; set; }
        public Status Status { get; set; }
        private DateTime _time = DateTime.Now;
        public DateTime CreatedAt
        {
            get { return _time; }
            set { _time = value; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database.Models
{
    [Table("Accounts")]
    public class Account
    {
        [StringLength(100)]
        [Column("ID")]
        public string Id { get; set; }
        [Required]
        [StringLength(100)]
        public string DeviceKey { get; set; }
        [StringLength(100)]
        public string Token { get; set; }
        [StringLength(1000)]
        public string Email { get; set; }
        [StringLength(100)]
        public string NickName { get; set; }
        [StringLength(100)]
        public string DownloadID { get; set; }
        [StringLength(2000)]
        public string PhotoPath { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsAutoSignIn { get; set; } = false;
        public bool IsValid { get; set; } = false;
        public int TimeStamp { get; set; } = 0;
    }
}
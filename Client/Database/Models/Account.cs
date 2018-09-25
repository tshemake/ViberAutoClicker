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
        [MaxLength(100)]
        [Column("ID")]
        public string Id { get; set; }
        [MaxLength(100)]
        public string DeviceKey { get; set; }
        [MaxLength(100)]
        public string Token { get; set; }
        [MaxLength(1000)]
        public string Email { get; set; }
        [MaxLength(100)]
        public string NickName { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsAutoSignIn { get; set; } = false;
        public bool IsValid { get; set; } = false;
        public int TimeStamp { get; set; } = 0;
    }
}
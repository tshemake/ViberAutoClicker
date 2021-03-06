﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Database;
using Client.Database.Models;

namespace Client
{
    public class ViberDb
    {
        private readonly string _dataSource;

        public ViberDb(string configDbPath)
        {
            _dataSource = configDbPath;
        }

        public async Task OffAccountsAsync()
        {
            try
            {
                using (var context = new ViberConfigDbContext(_dataSource))
                {
                    foreach (var account in context.Accounts)
                    {
                        account.IsValid = false;
                    }
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                // ignored
            }
        }

        public async Task<List<Account>> WaitNewAccountAsync()
        {
            try
            {
                using (var context = new ViberConfigDbContext(_dataSource))
                {
                    var count = context.Accounts.Count();
                    while (Viber.IsOpen())
                    {
                        await Task.Delay(100);
                        if (count < context.Accounts.Count())
                            break;
                    }

                    foreach (var account in context.Accounts)
                    {
                        account.IsValid = true;
                        context.Entry(account).State = EntityState.Modified;
                    }
                    await context.SaveChangesAsync();

                    return await context.Accounts.ToListAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task SaveAccountAsync(Account entity)
        {
            using (var context = new ViberConfigDbContext(_dataSource))
            {
                var account = context.Accounts.Find(entity.Id);
                if (account != null)
                {
                    account.IsDefault = entity.IsDefault;
                    account.IsAutoSignIn = entity.IsAutoSignIn;
                    account.DeviceKey = entity.DeviceKey;
                    account.Email = entity.Email;
                    account.IsValid = entity.IsValid;
                    account.NickName = entity.NickName;
                    account.TimeStamp = entity.TimeStamp;
                    account.Token = entity.Token;
                    await context.SaveChangesAsync();
                }
            }
        }

        public void SaveChanges()
        {
            using (var context = new ViberConfigDbContext(_dataSource))
            {
                context.SaveChangesAsync();
            }
        }

        public async Task<int> GetNextActiveAccountAsync()
        {
            var resultIndex = 0;
            try
            {
                using (var context = new ViberConfigDbContext(_dataSource))
                {
                    var accounts = context.Accounts.Where(a => a.IsAutoSignIn).ToList();
                    var index = accounts.FindIndex(a => a.IsDefault);
                    if (index == accounts.Count - 1)
                    {
                        resultIndex = 0;
                    }
                    else
                    {
                        resultIndex = index + 1;
                    }

                    if (index >= 0) accounts[index].IsDefault = false;
                    accounts[resultIndex].IsDefault = true;
                    await context.SaveChangesAsync();
                }
            }
            catch
            {
                // ignored
            }

            return resultIndex;
        }

        public async Task<string> GetCurrentAccountsAsync()
        {
            var result = string.Empty;
            try
            {
                using (var context = new ViberConfigDbContext(_dataSource))
                {
                    var account = await context.Accounts.FirstOrDefaultAsync(a => a.IsDefault);
                    if (account != null) return account.Id;
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }

        public BindingList<Account> LoadAccounts()
        {
            try
            {
                using (var context = new ViberConfigDbContext(_dataSource))
                {
                    context.Accounts.Load();
                    return context.Accounts.Local.ToBindingList();
                }
            }
            catch
            {
                throw;
            }
        }

        public int CountActiveAccount()
        {
            try
            {
                using (var context = new ViberConfigDbContext(_dataSource))
                {
                    return context.Accounts.Count(a => a.IsValid);
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

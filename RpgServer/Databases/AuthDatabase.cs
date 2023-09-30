using Microsoft.EntityFrameworkCore;
using RpgServer.Models;

namespace RpgServer.Databases
{
    public class AuthDatabase : DbContext
    {
        public AuthDatabase(DbContextOptions<AuthDatabase> opts) : base(opts)
        {
        }

        public AccountModel? FindAccount(ulong inAccountId)
        {
            return AccountSet.Where(each => each.Id == inAccountId).FirstOrDefault();
        }

        public AccountModel TouchAccount(ulong inAccountId)
        {
            var oldAccount = FindAccount(inAccountId);
            if (oldAccount != null) { return oldAccount; }

            var newAccount = new AccountModel();
            return newAccount;
        }

        public DeviceModel? FindDevice(string inIdfv)
        {
            return DeviceSet.Where(each => each.Idfv == inIdfv).FirstOrDefault();
        }

        public DeviceModel TouchDevice(string inIdfv)
        {
            var oldDevice = FindDevice(inIdfv);
            if (oldDevice != null) { return oldDevice; }

            var newDevice = new DeviceModel()
            {
                Idfv = inIdfv
            };
            return newDevice;
        }

        public SessionModel? FindSessionById(string inSessionId)
        {
            return SessionSet.Where(each => each.Id == inSessionId).FirstOrDefault();
        }
        public SessionModel? FindSessionByAccountId(ulong inAccountId)
        {
            return SessionSet.Where(each => each.AccountId == inAccountId).FirstOrDefault();
        }

        public DbSet<AccountModel> AccountSet { get; set; }
        public DbSet<DeviceModel> DeviceSet { get; set; }
        public DbSet<SessionModel> SessionSet { get; set; }
    }
}
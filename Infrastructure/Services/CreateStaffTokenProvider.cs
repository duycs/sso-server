using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthServer.Infrastructure.Services
{
    public class CreateStaffTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CreateStaffTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<CreateStaffTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger) : base(dataProtectionProvider, options, logger)
        {

        }
    }
}
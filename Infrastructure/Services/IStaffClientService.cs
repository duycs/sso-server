
using AuthServer.Models;
using Refit;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Services
{
    public interface IStaffClientService
    {
        /// <summary>
        /// https://localhost:5001/staffs
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Post("/staffs")]
        public Task Create(CreateStaffVM createStaffVM, string token);
    }
}

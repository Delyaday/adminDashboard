using Foundation.Users.Authorization;
using Microsoft.AspNetCore.Mvc;
using SailorMoon.Web.Records;
using SailorMoon.Web.Services;

namespace SailorMoon.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : Controller
    {
        private readonly IClientsService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public ClientsController(IClientsService service)
        {
            _service = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ClientRecord>> Get() => await _service.Get();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}")]
        public async Task<ClientRecord> Get(int id) => await _service.Get(id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [Route("phone/{phone}")]
        public async Task<ClientRecord> Get(string phone) => await _service.Get(phone);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ClientRecord> Create(ClientRecord record) => await _service.Create(record);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ClientRecord> Update(ClientRecord source) => await _service.Update(source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("{id:int}")]
        public async Task Delete(int id) => await _service.Delete(id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpDelete, Route("phone/{phone}")]
        public async Task Delete(string phone) => await _service.Delete(phone);

    }
}

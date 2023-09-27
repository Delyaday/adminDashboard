using Foundation.Users.Authorization;
using Microsoft.AspNetCore.Mvc;
using SailorMoon.Web.Records;
using SailorMoon.Web.Services;

namespace SailorMoon.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : Controller
    {
        private readonly IServicesService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public ServicesController(IServicesService service)
        {
            _service = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ServiceRecord>> Get() => await _service.Get();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}")]
        public async Task<ServiceRecord> Get(int id) => await _service.Get(id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ServiceRecord> Create(ServiceRecord record) => await _service.Create(record);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ServiceRecord> Update(ServiceRecord source) => await _service.Update(source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("{id:int}")]
        public async Task Delete(int id) => await _service.Delete(id);

    }
}

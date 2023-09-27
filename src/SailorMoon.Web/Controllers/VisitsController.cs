using Foundation.Users.Authorization;

using Microsoft.AspNetCore.Mvc;

using SailorMoon.Web.Records;
using SailorMoon.Web.Services;

namespace SailorMoon.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VisitsController : Controller
    {
        private readonly IVisitsService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public VisitsController(IVisitsService service)
        {
            _service = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<VisitRecord>> Get() => await _service.Get();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("{id:int}")]
        public async Task<VisitRecord> Get(int id) => await _service.Get(id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [Route("byClient/{clientId:int}")]
        public async Task<IEnumerable<VisitRecord>> GetByClient(int clientId) => await _service.GetByClient(clientId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("byService/{serviceId:int}")]
        public async Task<IEnumerable<VisitRecord>> GetByService(int serviceId) => await _service.GetByService(serviceId);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("byStaff/{staffId:int}")]
        public async Task<IEnumerable<VisitRecord>> GetByStaff(int staffId) => await _service.GetByStaff(staffId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<VisitRecord> Create(VisitRecord record) => await _service.Create(record);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<VisitRecord> Update(VisitRecord source) => await _service.Update(source);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("{id:int}")]
        public async Task Delete(int id) => await _service.Delete(id);
    }
}

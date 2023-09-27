using Foundation.Users.Authorization;
using Microsoft.AspNetCore.Mvc;
using SailorMoon.Web.Records;
using SailorMoon.Web.Services;

namespace SailorMoon.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserDescriptionController : Controller
    {
        private readonly IUserDescriptionService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public UserDescriptionController(IUserDescriptionService service)
        {
            _service = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserDescriptionRecord>> Get() => await _service.Get();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id:int}")]
        public async Task<UserDescriptionRecord> Get(int id) => await _service.Get(id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [Route("login/{login}")]
        public async Task<UserDescriptionRecord> Get(string login) => await _service.Get(login);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<UserDescriptionRecord> Create(UserDescriptionRecord record) => await _service.Create(record);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<UserDescriptionRecord> Update(UserDescriptionRecord source) => await _service.Update(source);

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
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpDelete, Route("login/{login}")]
        public async Task Delete(string login) => await _service.Delete(login);

    }
}

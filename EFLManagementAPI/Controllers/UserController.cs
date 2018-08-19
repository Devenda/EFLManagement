using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFLManagementAPI.Context;
using EFLManagementAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EFLManagementAPI.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly EFLContext _eflContext;

        public UserController(EFLContext eflContext)
        {
            _eflContext = eflContext;
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<IList<User>> GetAll()
        {
            return _eflContext.User.ToList();
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<User> GetById(int id)
        {
            return _eflContext.User.Where(u => u.UserId == id).FirstOrDefault();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<User> Create([FromBody] User user)
        {
            _eflContext.User.Add(user);
            _eflContext.SaveChanges();

            return Created("PostUser", user);
        }

        
    }
}
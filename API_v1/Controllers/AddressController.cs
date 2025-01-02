using API.ErrorHandling;
using AutoMapper;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Request;
using Respon;
using Respon.AddressRes;
using Service;
using System.Net;

namespace API.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableCors]
    public class AddressController : ControllerBase {
        private readonly IAddressService _addressService;
        private readonly IMapper _mapper;

        public AddressController(IAddressService addressService, IMapper mapper) {
            _addressService = addressService;
            _mapper = mapper;
        }

        private int GetUserIdFromToken() {
            var user = HttpContext.User;
            return int.Parse(user.Claims.FirstOrDefault(p => p.Type == "UserId").Value);
        }

        [HttpGet]
        public IActionResult GetByCustomerId() {
            var id = GetUserIdFromToken();
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Lấy địa chỉ khách hàng thành công", 
                Data = _addressService.GetByCustomerId(id).Select(p => _mapper.Map<AddressResponse>(p)) 
            });
        }

        [HttpGet("current")]
        public IActionResult GetDeliveryByCurrentUser()
        {
            var id = GetUserIdFromToken();
            Address address = _addressService.GetDeliveryByCurrentUser(id);
            if(address == null)
            {
                return NotFound(new ErrorDetails { StatusCode = 400, Message = "Người dùng này không có bất kì địa chỉ nào." });
            }
            return Ok(new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Lấy địa chỉ mặc định thành công",
                Data = _mapper.Map<AddressResponse>(address)
            });
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult Create([FromBody] AddressRequest request) {
            var id = GetUserIdFromToken();
            var address = _mapper.Map<Address>(request);
            address.CustomerId = id;
            _addressService.Create(address);
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Thêm mới địa chỉ thành công", 
                Data = null 
            });
        }
        
        [HttpPut]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult Update([FromBody] AddressRequest request) {
            var id = GetUserIdFromToken();
            _addressService.Update(_mapper.Map<Address>(request), id);
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Cập nhật địa chỉ thành công", 
                Data = null 
            });
        }
        
        [HttpDelete("{addressId:int}")]
        public IActionResult Delete(int addressId) {
            var id = GetUserIdFromToken();
            _addressService.Delete(addressId, id);
            return Ok(new BaseResponse { 
                Code = (int) HttpStatusCode.OK, 
                Message = "Xóa địa chỉ thành công", 
                Data = null 
            });
        }
    }
}

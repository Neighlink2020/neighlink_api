﻿using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NeighLink.Api.Models;
using NeighLink.DateLayer.Models;
using NeighLink.DateLayer.Service;
using NeighLink.DateLayer.Service.Impls;

namespace NeighLink.Api.Controllers
{
    [Route( "info" )]
    [ApiController]
    public class InfoController : BaseController
    {
        private readonly IBillService _billService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentCategoryService _paymentCategoryService;
        private readonly IAdministratorService _administratorService;
        private readonly IResidentService _residentService;
        private readonly IUserService _userService;

        public InfoController()
        {
            _billService = new BillServiceImpl( context );
            _paymentService = new PaymentServiceImpl( context );
            _paymentCategoryService = new PaymentCategoryServiceImpl( context );
            _administratorService = new AdministratorServiceImpl( context );
            _residentService = new ResidentServiceImpl( context );
            _userService = new UserServiceImpl( context );
        }

        #region BILL
        [HttpGet( "condominiums/{condominiumId}/departments/{departmentId}/bills" )]
        public async Task<IActionResult> GetBillsByDepartment(int condominiumId, int departmentId, [FromHeader] string Authotization)
        {
            try
            {
                var result = await _billService.GetAllByDepartment( departmentId );
                OkResponse( result );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }

        [HttpGet( "condominiums/{condominiumId}/bills" )]
        public async Task<IActionResult> GetBillsByCondominium(int condominiumId, [FromHeader] string Authotization)
        {
            try
            {
                var result = await _billService.GetAllByCondominium( condominiumId );
                OkResponse( result );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }

        [HttpPost( "condominiums/{condominiumId}/departments/{departmentId}/bills" )]
        public async Task<IActionResult> PostBillByDepartment(int condominiumId, int departmentId, [FromHeader] string Authorization, [FromBody] RequestBill request)
        {
            try
            {
                var admin = await _administratorService.AuthToken( Authorization );
                if (admin == null)
                {
                    UnauthorizedResponse();
                    return new ObjectResult(response);
                }

                var bill = new Bill()
                {

                    AdministratorId = admin.AdministratorId,
                    Amount = request.Amount,
                    CondominiumId = condominiumId,
                    DepartmentId = departmentId,
                    Description = request.Description,
                    EndDate = request.EndDate,
                    StartDate = request.StartDate,
                    IsDelete = false,
                    Name = request.Name,
                    PaymentCategoryId = request.PaymentCategoryId
                };
                var billSaved = await _billService.Insert( bill );
                OkResponse( billSaved );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }
        [HttpPut( "condominiums/{condominiumId}/departments/{departmentId}/bills/{billId}" )]
        public async Task<IActionResult> updateCondominiumRule(int condominiumId, int departmentId, int billId, [FromHeader] string Authotization, [FromBody] RequestBill request)
        {
            try
            {
                var bill = await _billService.GetById( billId );
                if (bill == null)
                {
                    NotFoundResponse();
                    return new ObjectResult(response);
                }

                bill.Name = request.Name;
                bill.Description = request.Description;
                bill.Amount = request.Amount;
                bill.StartDate = request.StartDate;
                bill.EndDate = request.EndDate;
                bill.PaymentCategoryId = request.PaymentCategoryId;
                var billSaved = await _billService.Update( bill );
                OkResponse( billSaved );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }
        [HttpDelete( "condominiums/{condominiumId}/departments/{departmentId}/bills/{billId}" )]
        public async Task<IActionResult> DeleteBillsByCondominium(int condominiumId, int departmentId, int billId, [FromHeader] string Authotization)
        {
            try
            {
                var bill = await _billService.GetById( billId );
                if (bill == null)
                {
                    NotFoundResponse();
                    return new ObjectResult(response);
                }
                bill.IsDelete = true;
                await _billService.Update( bill );
                OkResponse( null );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }
        #endregion

        #region PAY
        [HttpGet( "departments/{departmentId}/bills/{billId}/pays" )]
        public async Task<IActionResult> GetResidentDepartment(int departmentId, int billId, [FromHeader] string Authotization)
        {
            try
            {
                var result = await _paymentService.GetByBill( billId );
                OkResponse( result );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }

        [HttpPost( "departments/{departmentId}/bills/{billId}/pays" )]
        public async Task<IActionResult> AddResidentDepartment(int departmentId, int billId, [FromHeader] string Authorization, [FromBody] RequestPayment request)
        {
            try
            {
                var resident = await _residentService.AuthToken( Authorization );
                if (resident == null)
                {
                    NotFoundResponse();
                    return new ObjectResult(response);
                }

                var payment = new Payment()
                {
                    Amount = request.Amount,
                    ConfirmPaid = false,
                    BillId = billId,
                    PaymentDate = DateTime.Now,
                    ResidentId = resident.ResidentId,
                    ResidentUserId = resident.UserId,
                    UrlImage = request.UrlImage
                };
                var paymentSaved = await _paymentService.Insert( payment );
                OkResponse( paymentSaved );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }

        [HttpPut( "departments/{departmentId}/bills/{billId}/pays/{payId}/accept" )]
        public async Task<IActionResult> AcceptPayment(int departmentId, int billId, int payId, [FromHeader] string Authotization)
        {
            try
            {
                var payment = await _paymentService.GetById( payId );
                payment.ConfirmPaid = true;
                var paymentSaved = await _paymentService.Update( payment );
                OkResponse( paymentSaved );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }

        [HttpPut( "departments/{departmentId}/bills/{billId}/pays/{payId}/denny" )]
        public async Task<IActionResult> DennytPayment(int departmentId, int billId, int payId, [FromHeader] string Authotization)
        {
            try
            {
                var payment = await _paymentService.GetById( payId );
                payment.ConfirmPaid = false;
                var paymentSaved = await _paymentService.Update( payment );
                OkResponse( paymentSaved );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }
        #endregion 

        #region PAYMENT CATEGORY
        [HttpGet( "condominiums/{condominiumId}/paymentCategories" )]
        public async Task<IActionResult> GetResidentDepartment(int condominiumId, [FromHeader] string Authotization)
        {
            try
            {
                var result = await _paymentCategoryService.GetAllByCondomininum( condominiumId );
                OkResponse( result );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }

        [HttpPost( "condominiums/{condominiumId}/paymentCategories" )]
        public async Task<IActionResult> AddResidentDepartment(int condominiumId, [FromHeader] string Authorization, [FromBody] RequestPaymentCategory request)
        {
            try
            {
                var paymentCategory = new Paymentcategory()
                {
                    CondominiumId = condominiumId,
                    Description = request.Description,
                    Name = request.Name,
                    IsDelete = false
                };
                var paymentCategorySaved = await _paymentCategoryService.Insert( paymentCategory );
                OkResponse( paymentCategorySaved );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }
        [HttpDelete( "condominiums/{condominiumId}/paymentCategories/{paymentCategoryId}" )]
        public async Task<IActionResult> DeleteResidentDepartment(int condominiumId, int paymentCategoryId, [FromHeader] string Authotization)
        {
            try
            {
                var paymentCategory = await _paymentCategoryService.GetById( paymentCategoryId );
                paymentCategory.IsDelete = true;
                await _paymentCategoryService.Update( paymentCategory );
                OkResponse( null );
                return new ObjectResult(response);
            }
            catch (Exception e)
            {
                InternalServerErrorResponse( e.Message );
                return new ObjectResult(response);
            }
        }
        #endregion
    }


}

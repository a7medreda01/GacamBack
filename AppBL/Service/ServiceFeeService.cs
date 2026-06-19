using AppBL.DTOs;
using AppBL.IService;
using AppDAL.Entities;
using AppDAL.IRepos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AppBL.Service
{
    public class ServiceFeeService : IServiceFeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceFeeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceFeeDto>> GetAllFeesAsync()
        {
            var fees = await _unitOfWork.ServiceFees.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceFeeDto>>(fees);
        }

        public async Task<ServiceFeeDto> UpdateFeeAsync(
    OrderType orderType,
    ServiceFeeUpdateRequest request)
        {
            var fee = await _unitOfWork.ServiceFees
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.OrderType == orderType);

            if (fee == null)
                throw new KeyNotFoundException(
                    $"Pricing for '{orderType}' was not found.");

            fee.UnitPrice = request.UnitPrice;
            fee.ShippingFee = request.ShippingFee;
            fee.IsActive = request.IsActive;
            fee.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ServiceFees.Update(fee);

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ServiceFeeDto>(fee);
        }
    }
}

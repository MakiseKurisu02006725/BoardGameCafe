using BoardGameCafe.Core.Entities;
using BoardGameCafe.Core.Interfaces;

namespace BoardGameCafe.API.HangfireJobs;

/// <summary>
/// 生日优惠券发放定时任务
/// </summary>
public class BirthdayCouponJob
{
    /// <summary>
    /// 工作单元
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// 初始化生日优惠券发放定时任务的新实例
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    public BirthdayCouponJob(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 执行任务：检查今天过生日的会员，发放生日优惠券
    /// </summary>
    /// <returns>任务</returns>
    public Task ExecuteAsync()
    {
        try
        {
            var today = DateTime.Today;

            var birthdayMembers = _unitOfWork.Members
                .GetAll()
                .Where(m => m.Birthday.HasValue
                    && m.Birthday.Value.Month == today.Month
                    && m.Birthday.Value.Day == today.Day)
                .ToList();

            if (!birthdayMembers.Any())
            {
                return Task.CompletedTask;
            }

            foreach (var member in birthdayMembers)
            {
                var existingCoupon = _unitOfWork.Coupons
                    .GetAll()
                    .Any(c => c.MemberId == member.Id
                        && c.Name == "生日专属优惠券"
                        && c.CreatedAt >= today);

                if (existingCoupon)
                {
                    continue;
                }

                var coupon = new Coupon
                {
                    Id = Guid.NewGuid(),
                    MemberId = member.Id,
                    Name = "生日专属优惠券",
                    DiscountAmount = 20,
                    MinAmount = 100,
                    ExpireDate = today.AddDays(7),
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Coupons.Add(coupon);
            }

            _unitOfWork.SaveChanges();

            return Task.CompletedTask;
        }
        catch
        {
            throw;
        }
    }
}

using DeliveryApp.Application.Abstractions.Services;
using DeliveryApp.Application.DTOs.User;
using DeliveryApp.Application.Repositories;
using DeliveryApp.Application.ViewModels;
using DeliveryApp.Domain.Entities;
using DeliveryApp.Infrastructure.Enums;
using DeliveryApp.Persistence.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DeliveryApp.Persistence.Services
{
    public class CompanyService : ICompanyService
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly ICompanyRepository _companyRepository;
		private readonly IPhotoService _photoService;
		private readonly AppDbContext _context;
        public CompanyService(UserManager<AppUser> userManager, ICompanyRepository companyRepository,
            IPhotoService photoService, AppDbContext context)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _photoService = photoService;
            _context = context;
        }

        public async Task<CreateUserResponse> CreateAsync(RegisterCompanyVM model)
		{
			//var imageResult = await _photoService.AddPhotoAsync(model.Photo);
			AppUser user = new()
			{
				Email = model.Email,
				UserName = model.Email
			};

			IdentityResult result = await _userManager.CreateAsync(user,model.Password);

			bool companyResult = await _companyRepository.AddAsync(new()
			{
				Name = model.Name,
				PhoneNumber = model.PhoneNumber,
				AppUserId = user.Id,
				StartJob =model.StartJob,
				EndJob = model.EndJob,
				//ImageUrl = imageResult.SecureUrl.AbsoluteUri,
				//ImagePublicId = imageResult.PublicId profile
			});

			
			//f (result.Error != null) return BadRequest(result.Error.Message);

			

			await _userManager.AddToRoleAsync(user, AppRole.Company.ToString());
			bool companySaveResult = await _companyRepository.SaveAsync();

			CreateUserResponse response = new() { Succeeded = result.Succeeded };

			if (result.Succeeded&& companyResult&&companySaveResult)
				response.Message = "The user has been successfully created.";
			else
				foreach (var error in result.Errors)
					response.Message += $"{error.Code} - {error.Description}\n";


			return response;
			
		}

        public Task UpdateRefreshToken(string refreshToken, AppUser user, DateTime accessTokenDate, int addOnAccessTokenDate)
		{
			throw new NotImplementedException();
		}


		public Company GetCompany(string userId)
		{
			var query = _companyRepository.GetWhere(x => x.AppUserId == userId);
			Company company = query
							.Include(x => x.Orders)
							.Include(x => x.Comments)
							.ThenInclude(x => x.Customer)
							.Include(x => x.Products)
							.Include(c => c.Categories).First();

			return company;
		}

        public async Task<bool> UpdateAsync(UpdateCompanyDto companyDto)
        {
			var user = await _userManager.FindByIdAsync(companyDto.Id);
			var company =  GetCompany(user.Id);

			company.Name = companyDto.Name;
			company.EndJob = companyDto.EndJob;
			company.StartJob= companyDto.StartJob;
			company.PhoneNumber= companyDto.PhoneNumber;
			company.Description= companyDto.Description;

			

			 _companyRepository.Update(company);

			return await _companyRepository.SaveAsync();

		}

        public async Task<bool> UpdatePhotoAsync(IFormFile Photo, string userId)
        {
            if (Photo != null)
            {
				var user = await _userManager.FindByIdAsync(userId);
				var company = GetCompany(user.Id);

				if(company.ImagePublicId!=null) await _photoService.DeletePhotoAsync(company.ImagePublicId);

				var imageResult = await _photoService.AddPhotoAsync(Photo);

				company.ImageUrl = imageResult.SecureUrl.AbsoluteUri;
				company.ImagePublicId = imageResult.PublicId;

				return await _companyRepository.SaveAsync();
			}
			return false;
		}


		public IQueryable<Company> GetAllCompany()
        {
			var query = _companyRepository.GetAll(false);
			query = query.
					 Include(x => x.Products)
					.Include(x => x.Orders)
					.Include(x => x.Comments)
					.ThenInclude(x=>x.Customer)
					.Include(x=>x.Categories).ThenInclude(x=>x.Products)
					.OrderByDescending(x=>x.CreatedDate);

			return query;
        }

		public async Task<Company> GetCompanyByIdAsync(int? id)
        {
			var company = await _context.Companies
				.Include(x => x.Orders)
				.Include(x => x.Comments)
				.ThenInclude(x => x.Customer)
				.Include(x => x.Categories)
				.ThenInclude(x => x.Products)
				.FirstOrDefaultAsync(x=>x.Id==id);
			

			return company;
        }

        public async Task<bool> SetAddress(AddressVM address, int id)
        {
			var company =await GetCompanyByIdAsync(id);
			company.Address = address.Address;
			company.LatCoord = address.LatCoord;
			company.LngCoord = address.LngCoord;
			return await _companyRepository.SaveAsync();
        }
    }
}

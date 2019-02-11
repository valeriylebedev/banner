using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using BannerTest.Api.Models;
using BannerTest.Persistence.Context;
using BannerTest.Persistence.Context.Models;
using BannerTest.Services.Validation;
using Microsoft.AspNetCore.Mvc;

namespace BannerTest.Api.Controllers
{
    [Route("api/v1/banners")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly BannerContext _context;
        private readonly IMapper _mapper;
        private readonly IHtmlValidator _htmlValidator;

        public BannersController(BannerContext context, IMapper mapper, IHtmlValidator htmlValidator)
        {
            _context = context;
            _mapper = mapper;
            _htmlValidator = htmlValidator;
        }

        /// <summary>
        /// Returns an array of banners by query. At least one query parameter must be filled in.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BannerModel>), (int)HttpStatusCode.OK)]
        public IActionResult GetByQuery(string title = null)
        {
            var queryParams = new[] { title };

            if (queryParams.All(p => string.IsNullOrWhiteSpace(p)))
                return BadRequest("At least one query parameter must be specified");

            var banners = _context.Banners.Where(b => b.Title.Contains(title)).Select(b => _mapper.Map<BannerModel>(b));

            return Ok(banners);
        }

        /// <summary>
        /// Gets a banner by id
        /// </summary>
        /// <param name="bannerId"></param>
        /// <returns></returns>
        [HttpGet("{bannerId:int}")]
        [ProducesResponseType(typeof(BannerModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult GetById(int bannerId)
        {
            var bannerById = _context.Banners.FirstOrDefault(b => b.Id == bannerId);

            if (bannerById == null)
                return NotFound();

            return Ok(_mapper.Map<BannerModel>(bannerById));
        }

        [HttpGet("{bannerId:int}/html")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult GetHtmlById(int bannerId)
        {
            var bannerById = _context.Banners.FirstOrDefault(b => b.Id == bannerId);

            if (bannerById == null)
                return NotFound();

            return new ContentResult
            {
                ContentType = "text/html",
                Content = bannerById.Html
            };
        }

        /// <summary>
        /// Creates a banner. Title must be unique.
        /// </summary>
        /// <param name="createModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BannerModel), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> Create([FromBody]CreateBannerModel createModel)
        {
            if (createModel == null || !ModelState.IsValid)
                return BadRequest();

            if (IsTitleAlreadyRegistered(createModel.Title))
                return Conflict("Title is already registered");

            var validationResult = await _htmlValidator.Validate(createModel.Html);
            if (!validationResult.IsValidHtml)
                return BadRequest(validationResult);

            var banner = _mapper.Map<Banner>(createModel);

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            var createdBanner = _mapper.Map<BannerModel>(banner);

            return Created(string.Empty, createdBanner);
        }

        /// <summary>
        /// Updates a banner. Title, again, must be unique in case it has changed. 
        /// </summary>
        /// <param name="bannerId"></param>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        [HttpPut("{bannerId:int}")]
        [ProducesResponseType(typeof(BannerModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Update(int bannerId, [FromBody]UpdateBannerModel updateModel)
        {
            if (updateModel == null || !ModelState.IsValid)
                return BadRequest();

            if (IsTitleAlreadyRegistered(updateModel.Title, bannerId))
                return Conflict("Title is already registered");

            var bannerToUpdate = _context.Banners.FirstOrDefault(b => b.Id == bannerId);

            if (bannerToUpdate == null)
                return NotFound();

            var validationResult = await _htmlValidator.Validate(updateModel.Html);
            if (!validationResult.IsValidHtml)
                return BadRequest(validationResult);

            _mapper.Map(updateModel, bannerToUpdate);

            _context.Banners.Update(bannerToUpdate);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<BannerModel>(bannerToUpdate));
        }

        /// <summary>
        /// Deletes a banner
        /// </summary>
        /// <param name="bannerId"></param>
        /// <returns></returns>
        [HttpDelete("{bannerId:int}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(int bannerId)
        {
            var bannerToDelete = _context.Banners.FirstOrDefault(b => b.Id == bannerId);

            if (bannerToDelete == null)
                return NotFound();

            _context.Remove(bannerToDelete);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool IsTitleAlreadyRegistered(string title, int? bannerId = null)
        {
            return _context.Banners.Any(b => b.Title == title && (!bannerId.HasValue || b.Id != bannerId.Value));
        }
    }
}
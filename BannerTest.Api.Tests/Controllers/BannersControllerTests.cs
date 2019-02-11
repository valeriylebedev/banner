using BannerTest.Api.Components;
using BannerTest.Api.Controllers;
using BannerTest.Api.Models;
using BannerTest.Persistence.Context;
using BannerTest.Persistence.Context.Models;
using BannerTest.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerTest.Api.Tests.Controllers
{
    [TestClass]
    public class BannersControllerTests
    {
        private BannerContext Context { get; set; }
        private BannersController Controller { get; set; }
        private Mock<IHtmlValidator> HtmlValidatorMock { get; set; }

        [TestInitialize]
        public void RunBeforeEachTest()
        {
            var mapper = MapperFactory.GetMapper();
            var contextOptions = new DbContextOptionsBuilder<BannerContext>()
                .UseInMemoryDatabase("TestBannerContext").Options;
            HtmlValidatorMock = new Mock<IHtmlValidator>();

            Context = new BannerContext(contextOptions);
            Controller = new BannersController(Context, mapper, HtmlValidatorMock.Object);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_EmptyFilterQuery_When_GetByQuery_Then_BadRequest()
        {
            // Arrange

            // Act
            var result = Controller.GetByQuery(string.Empty);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("At least one query parameter must be specified", ((BadRequestObjectResult)result).Value);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_NoMatchingBanners_When_GetByQuery_Then_EmptyArrayOkResult()
        {
            // Arrange

            // Act
            var result = Controller.GetByQuery(Guid.NewGuid().ToString());

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var banners = ((OkObjectResult)result).Value as IEnumerable<BannerModel>;
            Assert.IsNotNull(banners);
            Assert.IsFalse(banners.Any());
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_MatchingBanner_When_GetByQuery_Then_OkResult()
        {
            // Arrange
            var banner = GenerateRandomBanner();
            var searchValue = banner.Title.Substring(1, 8);

            // Act
            var result = Controller.GetByQuery(searchValue);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var retrievedBanner = (((OkObjectResult)result).Value as IEnumerable<BannerModel>)?.SingleOrDefault();
            Assert.IsNotNull(retrievedBanner);
            Assert.AreEqual(banner.Id, retrievedBanner.Id);
            Assert.AreEqual(banner.Title, retrievedBanner.Title);
            Assert.AreEqual(banner.Html, retrievedBanner.Html);
            Assert.AreEqual(banner.Created, retrievedBanner.Created);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_NonexistentBanner_When_GetById_Then_NotFound()
        {
            // Arrange

            // Act
            var result = Controller.GetById(new Random().Next());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_ExistingBanner_When_GetById_Then_OkResult()
        {
            // Arrange
            var banner = GenerateRandomBanner();

            // Act
            var result = Controller.GetById(banner.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var retrievedBanner = ((OkObjectResult)result).Value as BannerModel;
            Assert.IsNotNull(retrievedBanner);
            Assert.AreEqual(banner.Id, retrievedBanner.Id);
            Assert.AreEqual(banner.Title, retrievedBanner.Title);
            Assert.AreEqual(banner.Html, retrievedBanner.Html);
            Assert.AreEqual(banner.Created, retrievedBanner.Created);
        }
        
        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_EmptyModel_When_Create_Then_BadRequest()
        {
            // Arrange

            // Act
            var result = await Controller.Create(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_AlreadyExistingTitle_When_Create_Then_Conflict()
        {
            // Arrange
            var banner = GenerateRandomBanner();

            var createModel = new CreateBannerModel
            {
                Title = banner.Title,
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            // Act
            var result = await Controller.Create(createModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
            Assert.AreEqual("Title is already registered", ((ConflictObjectResult)result).Value);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_ValidModel_When_Create_Then_OkResult()
        {
            // Arrange
            var createModel = new CreateBannerModel
            {
                Title = Guid.NewGuid().ToString(),
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            HtmlValidatorMock.Setup(v => v.Validate(createModel.Html))
                .Returns(Task.FromResult(new ValidationResult()));

            // Act
            var result = await Controller.Create(createModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedResult));

            var createdBanner = ((CreatedResult)result).Value as BannerModel;
            Assert.IsNotNull(createdBanner);
            Assert.AreNotEqual(default(int), createdBanner.Id);
            Assert.AreNotEqual(default(DateTime), createdBanner.Created);
            Assert.AreEqual(createModel.Title, createdBanner.Title);
            Assert.AreEqual(createModel.Html, createdBanner.Html);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_EmptyModel_When_Update_Then_BadRequest()
        {
            // Arrange

            // Act
            var result = await Controller.Update(new Random().Next(), null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_AlreadyExistingTitle_When_Update_Then_Conflict()
        {
            // Arrange
            var firstBanner = GenerateRandomBanner();
            var secondBanner = GenerateRandomBanner();

            var updateModel = new UpdateBannerModel
            {
                Title = firstBanner.Title,
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            // Act
            var result = await Controller.Update(secondBanner.Id, updateModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
            Assert.AreEqual("Title is already registered", ((ConflictObjectResult)result).Value);
        }


        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_NonexistentBanner_When_Update_Then_NotFound()
        {
            // Arrange
            var updateModel = new UpdateBannerModel
            {
                Title = Guid.NewGuid().ToString(),
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            // Act
            var result = await Controller.Update(new Random().Next(), updateModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_ValidModel_When_Update_Then_OkResult()
        {
            // Arrange
            var banner = GenerateRandomBanner();

            var updateModel = new UpdateBannerModel
            {
                Title = Guid.NewGuid().ToString(),
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            HtmlValidatorMock.Setup(v => v.Validate(updateModel.Html))
               .Returns(Task.FromResult(new ValidationResult()));

            // Act
            var result = await Controller.Update(banner.Id, updateModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var updatedBanner = ((OkObjectResult)result).Value as BannerModel;
            Assert.IsNotNull(updatedBanner);
            Assert.AreEqual(banner.Id, updatedBanner.Id);
            Assert.AreEqual(banner.Created, updatedBanner.Created);
            Assert.AreEqual(updateModel.Title, updatedBanner.Title);
            Assert.AreEqual(updateModel.Html, updatedBanner.Html);
            Assert.IsNotNull(updatedBanner.Modified);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_NonexistentBanner_When_Delete_Then_NotFound()
        {
            // Arrange

            // Act
            var result = await Controller.Delete(new Random().Next());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_BannerToDelete_When_Delete_Then_BannerIsDeleted()
        {
            // Arrange
            var banner = GenerateRandomBanner();

            // Act
            var result = await Controller.Delete(banner.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));

            var existingBanner = Context.Banners.FirstOrDefault(b => b.Id == banner.Id);
            Assert.IsNull(existingBanner);
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_InvalidHtml_When_Create_Then_BadRequest()
        {
            // Arrange
            var createModel = new CreateBannerModel
            {
                Title = Guid.NewGuid().ToString(),
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            HtmlValidatorMock.Setup(v => v.Validate(createModel.Html))
                .Returns(Task.FromResult(new ValidationResult()
                {
                    Errors = new[] { "Invalid HTML" }
                }));

            // Act
            var result = await Controller.Create(createModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var validationResult = ((BadRequestObjectResult)result).Value as ValidationResult;
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(validationResult.IsValidHtml);
            Assert.AreEqual("Invalid HTML", validationResult.Errors.Single());
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public async Task Given_InvalidHtml_When_Update_Then_BadRequest()
        {
            // Arrange
            var banner = GenerateRandomBanner();

            var updateModel = new UpdateBannerModel
            {
                Title = Guid.NewGuid().ToString(),
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>"
            };

            HtmlValidatorMock.Setup(v => v.Validate(updateModel.Html))
                .Returns(Task.FromResult(new ValidationResult()
                {
                    Errors = new[] { "Invalid HTML" }
                }));

            // Act
            var result = await Controller.Update(banner.Id, updateModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var validationResult = ((BadRequestObjectResult)result).Value as ValidationResult;
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(validationResult.IsValidHtml);
            Assert.AreEqual("Invalid HTML", validationResult.Errors.Single());
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_NonexistentBanner_When_GetHtml_Then_NotFound()
        {
            // Arrange

            // Act
            var result = Controller.GetHtmlById(new Random().Next());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        [TestCategory("Api/Controllers/Banners")]
        public void Given_ExistingBanner_When_GetHtml_Then_ContentResult()
        {
            // Arrange
            var banner = GenerateRandomBanner();

            // Act
            var result = Controller.GetHtmlById(banner.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ContentResult));

            var content = (ContentResult)result;
            Assert.AreEqual("text/html", content.ContentType);
            Assert.AreEqual(banner.Html, content.Content);
        }

        private Banner GenerateRandomBanner()
        {
            var banner = new Banner
            {
                Title = Guid.NewGuid().ToString(),
                Html = $"<html><body><span>{Guid.NewGuid()}</span></body></html>",
                Created = DateTime.UtcNow
            };

            Context.Banners.Add(banner);
            Context.SaveChanges();

            return banner;
        }
    }
}

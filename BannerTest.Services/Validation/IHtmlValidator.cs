using System.Threading.Tasks;

namespace BannerTest.Services.Validation
{
    public interface IHtmlValidator
    {
        Task<ValidationResult> Validate(string html);
    }
}

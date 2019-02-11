using System.Collections.Generic;
using System.Linq;

namespace BannerTest.Services.Validation
{
    public class ValidationResult
    {
        public IEnumerable<string> Errors { get; set; }

        public bool IsValidHtml => Errors?.Any() != true;
    }
}

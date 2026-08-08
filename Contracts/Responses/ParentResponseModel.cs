using System.Text.Json;
using Contracts.enums;

namespace Contracts.Responses
{
    public class ParentResponseModel
    {
        public ErrorCatalog ErrorCode { get; set; }
        public bool IsDone { get; set; }
        public string? ReturnMessage { get; set; }
        public int? StatusCode { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class SingleObjectResponseModel : ParentResponseModel { }
}

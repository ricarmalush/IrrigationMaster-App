using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Core.Features.Models.Structure.Country
{
    public class PagedResponse<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; init; }

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; init; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; init; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; init; }
    }
}

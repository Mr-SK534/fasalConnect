namespace FarmerMarketplace.Api.DTOs
{
    public class AdminUserListResponseDto
    {
        public List<UserResponseDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}

namespace GameStudioSite.Models
{
    public class PaginationModel
    {
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }
        public Func<int, string> PageUrl { get; init; } = _ => "#";
    }
}
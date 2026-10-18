using GameStudioSite.Models;
using MongoDB.Driver;

namespace GameStudioSite.Data
{
    public class BlogPostRepository
    {
        private readonly IMongoCollection<BlogPost> _posts;
        private readonly CategoryRepository _categoryRepository;

        public BlogPostRepository(MongoDbContext context, CategoryRepository categoryRepository)
        {
            _posts = context.BlogPosts;
            _categoryRepository = categoryRepository;
        }

        public (List<BlogPost> Posts, int TotalCount) GetPage(int page, int pageSize, string? category = null)
        {
            var filterBuilder = Builders<BlogPost>.Filter;
            var privateCategories = _categoryRepository.GetPrivateCategoryNames();
            var filter = filterBuilder.Nin(p => p.Category, privateCategories);

            if (!string.IsNullOrEmpty(category))
            {
                filter &= filterBuilder.Eq(p => p.Category, category);
            }

            var totalCount = (int)_posts.CountDocuments(filter);

            var posts = _posts.Find(filter)
                .SortByDescending(p => p.PublishedDate)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToList();

            return (posts, totalCount);
        }

        public List<BlogPost> GetLatest(int count)
        {
            var privateCategories = _categoryRepository.GetPrivateCategoryNames();

            return _posts.Find(Builders<BlogPost>.Filter.Nin(p => p.Category, privateCategories))
                .SortByDescending(p => p.PublishedDate)
                .Limit(count)
                .ToList();
        }

        public BlogPost? GetById(string id) => _posts.Find(p => p.Id == id).FirstOrDefault();


        public List<BlogPost> GetAllForAdmin()
        {
            return _posts.Find(FilterDefinition<BlogPost>.Empty)
                .ToList()
                .OrderByDescending(p => p.PublishedDate)
                .ToList();
        }

        public List<string> GetAllCategoriesPublic()
        {
            var privateCategories = _categoryRepository.GetPrivateCategoryNames();
            return _posts.Find(Builders<BlogPost>.Filter.Nin(p => p.Category, privateCategories))
                .ToList()
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public BlogPost? GetBySlug(string slug)
        {
            return _posts.Find(p => p.Slug == slug).FirstOrDefault();
        }

        public void Create(BlogPost post) => _posts.InsertOne(post);

        public void Update(string id, BlogPost post) =>
            _posts.ReplaceOne(p => p.Id == id, post);

        public void Delete(string id) =>
            _posts.DeleteOne(p => p.Id == id);
    }
}
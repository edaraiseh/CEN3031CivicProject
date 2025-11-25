using Microsoft.EntityFrameworkCore;
using Xunit;
using Api.Services;
using Api.Models;
using Api.Controllers;
using Api.Data;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

[CollectionDefinition("PostTests")]
public class PostAndVariantTests { }

[Collection("PostTests")]
public class PostApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public PostApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });
        });
    }

    private void ClearDatabase(TVDbContext dbContext)
    {
        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Posts.RemoveRange(dbContext.Posts);
        dbContext.SaveChanges();
    }

    [Fact]
    public async Task CreateUpdateDeletePostWithValidJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
        // Arrange
        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Test CRUD endpoints
        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        makePostResponse.EnsureSuccessStatusCode();
        var makePostResult = await makePostResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var postId = makePostResult!.Id;

        var updatePostRequest = new UpdatePostDTO
        {
            NewContent = "Goodbye world!"
        };
        var updatePostResponse = await client.PostAsJsonAsync($"/api/posts/{postId}", updatePostRequest);
        updatePostResponse.EnsureSuccessStatusCode();

        var deletePostResponse = await client.DeleteAsync($"/api/posts/{postId}");
        deletePostResponse.EnsureSuccessStatusCode();

        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        Assert.Equal("Goodbye world!", post!.Content);
        Assert.NotNull(post!.UpdatedAt);
        Assert.True(post!.IsDeleted);
    }
    [Fact]
    public async Task CreateUpdateDeleteOfficialPostWithValidJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
        // Arrange
        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        var userResult = await createUserResponse.Content.ReadFromJsonAsync<UserDTO>();
        createUserResponse.EnsureSuccessStatusCode();
        // Set user to be official user
        var user = await dbContext.Users
            .Where(u => u.Id == userResult!.Id)
            .FirstOrDefaultAsync();
        user!.IsOfficial = true;
        await dbContext.SaveChangesAsync();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Test CRUD endpoints
        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts/official", postRequest);
        makePostResponse.EnsureSuccessStatusCode();
        var makePostResult = await makePostResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var postId = makePostResult!.Id;

        var updatePostRequest = new UpdatePostDTO
        {
            NewContent = "Goodbye world!"
        };
        var updatePostResponse = await client.PostAsJsonAsync($"/api/posts/{postId}", updatePostRequest);
        updatePostResponse.EnsureSuccessStatusCode();

        var deletePostResponse = await client.DeleteAsync($"/api/posts/{postId}");
        deletePostResponse.EnsureSuccessStatusCode();

        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        Assert.Equal("Goodbye world!", post!.Content);
        Assert.NotNull(post!.UpdatedAt);
        Assert.True(post!.IsDeleted);
    }
    [Fact]
    public async Task CreatePostWithoutJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);

        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, makePostResponse.StatusCode);
    }
    [Fact]
    public async Task CreateOfficialPostWithoutJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);

        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts/official", postRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, makePostResponse.StatusCode);
    }
    [Fact]
    public async Task CreateOfficialPostAsNormalUser_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
        // Arrange
        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Test CRUD endpoints
        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        makePostResponse.EnsureSuccessStatusCode();
        var makeOfficialPostResponse = await client.PutAsJsonAsync("/api/posts/official", postRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, makeOfficialPostResponse.StatusCode);
    }
    [Fact]
    public async Task GetOfficialPostsOnlyGetsOfficial()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
        // Arrange
        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        var userResult = await createUserResponse.Content.ReadFromJsonAsync<UserDTO>();
        createUserResponse.EnsureSuccessStatusCode();
        // Set user to be official user
        var user = await dbContext.Users
            .Where(u => u.Id == userResult!.Id)
            .FirstOrDefaultAsync();
        user!.IsOfficial = true;
        await dbContext.SaveChangesAsync();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Create official and unofficial post
        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var officialPostRequest = new CreatePostDTO
        {
            Content = "Hello official world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        makePostResponse.EnsureSuccessStatusCode();
        var makeOfficialPostResponse = await client.PutAsJsonAsync("/api/posts/official", officialPostRequest);
        makeOfficialPostResponse.EnsureSuccessStatusCode();

        var getPostsResponse = await client.GetAsync("/api/posts");
        getPostsResponse.EnsureSuccessStatusCode();
        var getPostsResult = await getPostsResponse.Content.ReadFromJsonAsync<PostQueryDTO>();

        var getOfficialPostsResponse = await client.GetAsync("/api/posts/official");
        getOfficialPostsResponse.EnsureSuccessStatusCode();
        var getOfficialPostsResult = await getOfficialPostsResponse.Content.ReadFromJsonAsync<PostQueryDTO>();
        
        var posts = getPostsResult!.Posts;
        var officialPosts = getOfficialPostsResult!.Posts;

        Assert.Single(posts);
        Assert.Single(officialPosts);
        Assert.Equal("Hello world!", posts.First().Content);
        Assert.Equal("Hello official world!", officialPosts.First().Content);
    }
    [Fact]
    public async Task DeleteOrEditAnotherUserPost_EndToEnd()
    {

        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        UserService userService = new UserService(dbContext);
        PostsService postsService = new PostsService(dbContext);
        ClearDatabase(dbContext);

        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Create a post from another user
        var newUser = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "tester2",
            Name = "tester2",
            Email = "tester@test.com",
            Password = "P@ssw0rd!",
        });
        var (_, newPost) = await postsService.CreatePostAsync(newUser!.Id, "Not Hello World!");
        int postId = newPost!.Id;

        // Attempt to edit and delete
        var updatePostRequest = new UpdatePostDTO
        {
            NewContent = "Goodbye world!"
        };
        var updatePostResponse = await client.PostAsJsonAsync($"/api/posts/{postId}", updatePostRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, updatePostResponse.StatusCode);

        var deletePostResponse = await client.DeleteAsync($"/api/posts/{postId}");
        Assert.Equal(HttpStatusCode.Unauthorized, deletePostResponse.StatusCode);

    }
    [Fact]
    public async Task CreateUpdateDeleteReplyWithValidJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
        // Arrange
        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Create post
        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        makePostResponse.EnsureSuccessStatusCode();
        var makePostResult = await makePostResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var postId = makePostResult!.Id;
        var replyRequest = new CreatePostDTO
        {
            Content = "I'm replying to a post!"
        };
        var makeReplyResponse = await client.PutAsJsonAsync($"/api/posts/{postId}/replies", replyRequest);
        var makeReplyResult = await makeReplyResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var replyId = makeReplyResult!.Id;

        var updateReplyRequest = new UpdatePostDTO
        {
            NewContent = "Goodbye world!"
        };
        var updateReplyResponse = await client.PostAsJsonAsync($"/api/posts/{replyId}", updateReplyRequest);
        updateReplyResponse.EnsureSuccessStatusCode();

        var deleteReplyResponse = await client.DeleteAsync($"/api/posts/{replyId}");
        deleteReplyResponse.EnsureSuccessStatusCode();

        var post = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == replyId);
        Assert.Equal("Goodbye world!", post!.Content);
        Assert.NotNull(post!.UpdatedAt);
        Assert.True(post!.IsDeleted);
        Assert.Equal(postId, post!.ParentId);
    }
    [Fact]
    public async Task CreateReplyWithoutJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        UserService userService = new UserService(dbContext);
        PostsService postsService = new PostsService(dbContext);
        ClearDatabase(dbContext);

        // Create a post
        var user = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "Test",
            Name = "Test",
            Password = "P@ssw0rd!",
            Email = "test@test.com"
        });
        var (_,post) = await postsService.CreatePostAsync(user!.Id, "Hello world!");

        // Reply without JWT
        var replyRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makeReplyResponse = await client.PutAsJsonAsync($"/api/posts/{post!.Id}/replies", replyRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, makeReplyResponse.StatusCode);
    }
    [Fact]
    public async Task DeletingTopLevelPostDeletesAllReplies_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        UserService userService = new UserService(dbContext);
        PostsService postsService = new PostsService(dbContext);
        ClearDatabase(dbContext);

        // Create a user
        var user = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "Test",
            Name = "Test",
            Password = "P@ssw0rd!",
            Email = "test@test.com"
        });
        // Sign in and store JWT
        var loginRequest = new UserLoginRequest { Username = "Test", Password = "P@ssw0rd!" };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Create a post and replies
        var postRequest = new CreatePostDTO
        {
            Content = "Post"
        };
        var postResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        postResponse.EnsureSuccessStatusCode();
        var postResult = await postResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var reply1Response = await client.PutAsJsonAsync($"api/posts/{postResult!.Id}/replies", postRequest);
        reply1Response.EnsureSuccessStatusCode();
        var reply1Result = await reply1Response.Content.ReadFromJsonAsync<CreatePostDTO>();
        var reply2Response = await client.PutAsJsonAsync($"api/posts/{reply1Result!.Id}/replies", postRequest);
        reply2Response.EnsureSuccessStatusCode();
        var reply2Result = await reply2Response.Content.ReadFromJsonAsync<CreatePostDTO>();

        // Delete the top level
        var deleteResult = await client.DeleteAsync($"/api/posts/{postResult.Id}");
        deleteResult.EnsureSuccessStatusCode();

        // Query the posts
        var post1 = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postResult!.Id);
        Assert.True(post1!.IsDeleted);
        var post2 = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == reply1Result!.Id);
        Assert.True(post2!.IsDeleted);
        var post3 = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == reply2Result!.Id);
        Assert.True(post3!.IsDeleted);
    }
    [Fact]
    public async Task DeletingMidLevelReplyOnlyDeletesItself_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        UserService userService = new UserService(dbContext);
        PostsService postsService = new PostsService(dbContext);
        ClearDatabase(dbContext);

        // Create a user
        var user = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "Test",
            Name = "Test",
            Password = "P@ssw0rd!",
            Email = "test@test.com"
        });
        // Sign in and store JWT
        var loginRequest = new UserLoginRequest { Username = "Test", Password = "P@ssw0rd!" };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Create a post and replies
        var postRequest = new CreatePostDTO
        {
            Content = "Post"
        };
        var postResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        postResponse.EnsureSuccessStatusCode();
        var postResult = await postResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var reply1Response = await client.PutAsJsonAsync($"api/posts/{postResult!.Id}/replies", postRequest);
        reply1Response.EnsureSuccessStatusCode();
        var reply1Result = await reply1Response.Content.ReadFromJsonAsync<CreatePostDTO>();
        var reply2Response = await client.PutAsJsonAsync($"api/posts/{reply1Result!.Id}/replies", postRequest);
        reply2Response.EnsureSuccessStatusCode();
        var reply2Result = await reply2Response.Content.ReadFromJsonAsync<CreatePostDTO>();

        // Delete the mid level
        var deleteResult = await client.DeleteAsync($"/api/posts/{reply1Result.Id}");
        deleteResult.EnsureSuccessStatusCode();

        // Query the posts
        var post1 = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == postResult!.Id);
        Assert.False(post1!.IsDeleted);
        var post2 = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == reply1Result!.Id);
        Assert.True(post2!.IsDeleted);
        var post3 = await dbContext.Posts.FirstOrDefaultAsync(p => p.Id == reply2Result!.Id);
        Assert.False(post3!.IsDeleted);
    }
    [Fact]
    public async Task CreateDeletePostReactions()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
        // Arrange
        // Create user
        string username = "john";
        string name = "John Dingle";
        string email = "test@tester.com";
        string password = "P@ssw0rd!";
        var createUserRequest = new CreateUserRequest { Username = username, Name = name, Email = email, Password = password };
        var createUserResponse = await client.PutAsJsonAsync("/api/users/register", createUserRequest);
        createUserResponse.EnsureSuccessStatusCode();

        // Login and store JWT
        var loginRequest = new UserLoginRequest { Username = username, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/users/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<UserLoginResponse>();
        string jwt = loginResult!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        // Create post
        var postRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makePostResponse = await client.PutAsJsonAsync("/api/posts", postRequest);
        makePostResponse.EnsureSuccessStatusCode();
        var makePostResult = await makePostResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var postId = makePostResult!.Id;

        // Create reactions
        var likeRequest = new CreateReactionDTO
        {
            PostId = postId,
            Type = ReactionType.Like
        };
        var dislikeRequest = new CreateReactionDTO
        {
            PostId = postId,
            Type = ReactionType.Dislike
        };
        var heartRequest = new CreateReactionDTO
        {
            PostId = postId,
            Type = ReactionType.Heart
        };
        var makeLikeResponse = await client.PutAsJsonAsync($"/api/posts/{postId}/reactions",likeRequest); 
        makeLikeResponse.EnsureSuccessStatusCode();
        var makeDislikeResponse = await client.PutAsJsonAsync($"/api/posts/{postId}/reactions",dislikeRequest); 
        makeDislikeResponse.EnsureSuccessStatusCode();
        var makeHeartResponse = await client.PutAsJsonAsync($"/api/posts/{postId}/reactions",heartRequest); 
        makeHeartResponse.EnsureSuccessStatusCode();
        
        var reactionsResponse = await client.GetAsync($"/api/posts/{postId}/reactions");
        reactionsResponse.EnsureSuccessStatusCode();
        var reactionsResult = await reactionsResponse.Content.ReadFromJsonAsync<ReactionAggregateDTO>();
        Assert.Equal(postId, reactionsResult!.PostId);
        Assert.Equal(1, reactionsResult!.Likes);
        Assert.Equal(1, reactionsResult!.Dislikes);
        Assert.Equal(1, reactionsResult!.Hearts);

        // no method for sending delete requests with JSON bodies
        var deleteLikeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{postId}/reactions")
        {
            Content = new StringContent(JsonSerializer.Serialize(likeRequest), Encoding.UTF8, "application/json")
        };
        var deleteLikeResponse = await client.SendAsync(deleteLikeRequest);
        deleteLikeResponse.EnsureSuccessStatusCode();
        var deleteDislikeRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{postId}/reactions")
        {
            Content = new StringContent(JsonSerializer.Serialize(dislikeRequest), Encoding.UTF8, "application/json")
        };
        var deleteDislikeResponse = await client.SendAsync(deleteDislikeRequest);
        deleteDislikeResponse.EnsureSuccessStatusCode();
        var deleteHeartRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/posts/{postId}/reactions")
        {
            Content = new StringContent(JsonSerializer.Serialize(heartRequest), Encoding.UTF8, "application/json")
        };
        var deleteHeartResponse = await client.SendAsync(deleteHeartRequest);
        deleteHeartResponse.EnsureSuccessStatusCode();

        reactionsResponse = await client.GetAsync($"/api/posts/{postId}/reactions");
        reactionsResponse.EnsureSuccessStatusCode();
        reactionsResult = await reactionsResponse.Content.ReadFromJsonAsync<ReactionAggregateDTO>();
        Assert.Equal(postId, reactionsResult!.PostId);
        Assert.Equal(0, reactionsResult!.Likes);
        Assert.Equal(0, reactionsResult!.Dislikes);
        Assert.Equal(0, reactionsResult!.Hearts);
    }
    [Fact]
    public async Task GetPostsDoesNotReturnPetitions()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        UserService userService = new UserService(dbContext);
        PostsService postsService = new PostsService(dbContext);
        PetitionsService petitionsService = new PetitionsService(dbContext);
        ClearDatabase(dbContext);

        // Create a post
        var user = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "Test",
            Name = "Test",
            Password = "P@ssw0rd!",
            Email = "test@test.com"
        });
        var (_,post) = await postsService.CreatePostAsync(user!.Id, "Hello world!");
        // Create a petition
        var (_,petition) = await petitionsService.CreatePetitionAsync(user!.Id, "Test Petition", "Hello world petitioners!");
        // Get posts
        var getPostsResponse = await client.GetAsync($"/api/posts"); 
        var posts = await getPostsResponse.Content.ReadFromJsonAsync<PostQueryDTO>();
        
        Assert.Single(posts!.Posts);
        Assert.Equal("Hello world!",posts!.Posts.First().Content);
    }
    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
    }
}

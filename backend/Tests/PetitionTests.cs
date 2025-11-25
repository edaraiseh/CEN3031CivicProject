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
using Microsoft.VisualBasic;

[Collection("PostTests")]
public class PetitionApiTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    public PetitionApiTests(WebApplicationFactory<Program> factory)
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
    public async Task CreateDeletePetitionWithValidJWT_EndToEnd()
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
        var petitionRequest = new CreatePetitionDTO
        {
            Title = "Test Petition",
            Content = "Hello world!"
        };
        var makePetitionResponse = await client.PutAsJsonAsync("/api/petitions", petitionRequest);
        makePetitionResponse.EnsureSuccessStatusCode();
        var makePostResult = await makePetitionResponse.Content.ReadFromJsonAsync<CreatePetitionDTO>();
        var postId = makePostResult!.Id;

        var deletePetitionResponse = await client.DeleteAsync($"/api/petitions/{postId}");
        deletePetitionResponse.EnsureSuccessStatusCode();

        var petition = await dbContext.Petitions.FirstOrDefaultAsync(p => p.Id == postId);
        Assert.Equal("Test Petition", petition!.Title);
        Assert.Equal("Hello world!", petition!.Content);
        Assert.NotNull(petition!.UpdatedAt);
        Assert.Equal(PetitionStatus.Failed,petition!.Status);
    }
    [Fact]
    public async Task CreatePetitionWithoutJWT_EndToEnd()
    {
        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);

        var petitionRequest = new CreatePetitionDTO
        {
            Title = "Test Petition",
            Content = "Hello world!"
        };
        var makePetitionResponse = await client.PutAsJsonAsync("/api/petitions", petitionRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, makePetitionResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteAnotherUserPetition_EndToEnd()
    {

        HttpClient client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        UserService userService = new UserService(dbContext);
        PetitionsService petitionsService = new PetitionsService(dbContext);
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

        // Create a petition from another user
        var newUser = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "tester2",
            Name = "tester2",
            Email = "tester@test.com",
            Password = "P@ssw0rd!",
        });
        var (_, newPetition) = await petitionsService.CreatePetitionAsync(newUser!.Id, "Test Petition", "Not Hello World!");
        int petitionId = newPetition!.Id;

        // Attempt to delete
        var deletePostResponse = await client.DeleteAsync($"/api/posts/{petitionId}");
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

        // Create petition
        var petitionRequest = new CreatePetitionDTO
        {
            Title = "Test Petition",
            Content = "Hello world!"
        };
        var makePetitionResponse = await client.PutAsJsonAsync("/api/petitions", petitionRequest);
        makePetitionResponse.EnsureSuccessStatusCode();
        var makePostResult = await makePetitionResponse.Content.ReadFromJsonAsync<CreatePetitionDTO>();
        var postId = makePostResult!.Id;
        var replyRequest = new CreatePostDTO
        {
            Content = "I'm replying to a post!"
        };
        var makeReplyResponse = await client.PutAsJsonAsync($"/api/petitions/{postId}/replies", replyRequest);
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
        PetitionsService petitionsService = new PetitionsService(dbContext);
        ClearDatabase(dbContext);

        // Create a petition
        var user = await userService.RegisterUserAsync(new CreateUserRequest
        {
            Username = "Test",
            Name = "Test",
            Password = "P@ssw0rd!",
            Email = "test@test.com"
        });
        var (_,petition) = await petitionsService.CreatePetitionAsync(user!.Id, "Test Petition", "Hello world!");

        // Reply without JWT
        var replyRequest = new CreatePostDTO
        {
            Content = "Hello world!"
        };
        var makeReplyResponse = await client.PutAsJsonAsync($"/api/petitions/{petition!.Id}/replies", replyRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, makeReplyResponse.StatusCode);
    }
    [Fact]
    public async Task GetPetitionsDoesNotReturnPosts()
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
        // Get petitions
        var getPetitions = await client.GetAsync($"/api/petitions"); 
        var petitions = await getPetitions.Content.ReadFromJsonAsync<PetitionQueryDTO>();
        
        Assert.Single(petitions!.Petitions);
        Assert.Equal("Test Petition",petitions!.Petitions.First().Title);
        Assert.Equal("Hello world petitioners!",petitions!.Petitions.First().Content);
    }
    [Fact]
    public async Task CreateDeletePetitionSignature()
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

        // Create petition
        var petitionRequest = new CreatePetitionDTO
        {
            Title = "Test Petition",
            Content = "Hello world!"
        };
        var makePetitionResponse = await client.PutAsJsonAsync("/api/petitions", petitionRequest);
        makePetitionResponse.EnsureSuccessStatusCode();
        var makePetitionResult = await makePetitionResponse.Content.ReadFromJsonAsync<CreatePostDTO>();
        var petitionId = makePetitionResult!.Id;

        // Create signature
        var signatureResponse = await client.PutAsJsonAsync($"/api/petitions/{petitionId}/sign",""); 
        signatureResponse.EnsureSuccessStatusCode();
        
        // Get petitions
        var getPetitionsResponse = await client.GetAsync($"/api/petitions");
        getPetitionsResponse.EnsureSuccessStatusCode();
        var getPetitionsResult = await getPetitionsResponse.Content.ReadFromJsonAsync<PetitionQueryDTO>(); 

        Assert.Single(getPetitionsResult!.Petitions);
        Assert.Equal(1,getPetitionsResult!.Petitions.First().SignatureCount);

        // Delete signature
        var deleteSignatureResponse = await client.DeleteAsync($"/api/petitions/{petitionId}/sign");
        deleteSignatureResponse.EnsureSuccessStatusCode();

        // Get petitions, again
        getPetitionsResponse = await client.GetAsync($"/api/petitions");
        getPetitionsResponse.EnsureSuccessStatusCode();
        getPetitionsResult = await getPetitionsResponse.Content.ReadFromJsonAsync<PetitionQueryDTO>(); 

        Assert.Single(getPetitionsResult!.Petitions);
        Assert.Equal(0,getPetitionsResult!.Petitions.First().SignatureCount);
    }
    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        TVDbContext dbContext = scope.ServiceProvider.GetRequiredService<TVDbContext>();
        ClearDatabase(dbContext);
    }
}

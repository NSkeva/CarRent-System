namespace CarRent.Web.IntegrationTests;

public static class ApiTestClientExtensions
{
    public static HttpClient AsRole(this HttpClient client, string role)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        return client;
    }
}

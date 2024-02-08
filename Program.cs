using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class User
{
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public string Location { get; set; }
    public int Reputation { get; set; }
    public List<string> Collectives { get; set; }
    public string profileLink { get; set; }
    public string avatarLink { get; set; }

}


public class Program
{

    private static readonly HttpClient _httpClient = new HttpClient();

    // private const string key = "*RnG73M5XZloWeZ*7*Fh7A((";
    private const string ApiUrl = "https://api.stackexchange.com/2.3";
    private const string UsersEndPoint = $"/users?order=desc&sort=reputation&site=stackoverflow&page=4&pagesize=99";


    public static async Task Main()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(ApiUrl + UsersEndPoint);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await DecompressResponse(response);
                var users = JsonConvert.DeserializeObject<dynamic>(responseBody).items;

                var filteredUsers = FilterUsers(users);
                await ProcessUsers(filteredUsers);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
         }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }   
            //string jsonString = File.ReadAllText("fakeApi/users.json");

            //dynamic users;
            // if (1 == 0)
            // {
            // }
            // else
            // {
            //     users = JsonConvert.DeserializeObject<dynamic>(jsonString).items;
            // }
}

    private static async Task<string> DecompressResponse(HttpResponseMessage response)
    {
        using (var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
        using (var reader = new StreamReader(decompressedStream))
        {
            return await reader.ReadToEndAsync();
        }
    }

    //filter users
    private static List<User> FilterUsers(dynamic users)
    {


        var filteredUsers = new List<User>();
        foreach (var user in users)
        {
            //check end filter user reputation and user location
            if (user.reputation > 223 && user.location == "Transylvania, Romania" || user.location == "Moldova")
            {
                var newUser = new User
                {
                    UserId = user.user_id,
                    DisplayName = user.display_name,
                    Location = user.location,
                    Reputation = user.reputation,
                    profileLink = user.link,
                    avatarLink = user.profile_image,
                    Collectives = new List<string>(),

                };

                // Check and filter collectives
                if (user.collectives != null && user.collectives.Count > 0)
                {
                    foreach (var col in user.collectives)
                    {
                        if (col.collective != null && col.collective.tags.Count > 0)
                        {
                            bool containsTags = false;

                            foreach (var tag in col.collective.tags)
                            {
                                if (tag == ".net" || tag == "java" || tag == "bigtable" || tag == "c#")
                                {
                                    containsTags = true;
                                    newUser.Collectives.Add((string)tag);
                                }
                            }
                            if (containsTags)
                            {
                                filteredUsers.Add(newUser);
                            }
                        }
                    }
                }
            }
        }
        return filteredUsers;
    }

    // function returns AnswerCount
    private static async Task<int> AnswerCount(int userId)
    {
        int count = 0;


        string answersEndPoint = $"https://api.stackexchange.com/2.3/users/{userId}/answers?order=desc&sort=activity&site=stackoverflow&filter=total";
        HttpResponseMessage answerResponse = await _httpClient.GetAsync(answersEndPoint);

        if (answerResponse.IsSuccessStatusCode)
        {
            string responseBody = await DecompressResponse(answerResponse);
            var answers = JsonConvert.DeserializeObject<dynamic>(responseBody).total;

            count = answers;
        }
        else
        {
            Console.WriteLine($"Error: {answerResponse.StatusCode} - {answerResponse.ReasonPhrase}");
        }
        return count;
    }

    // function returns QuestionCount
    private static async Task<int> QuestionCount(int userId)
    {
        int count = 0;


        string questionEndPoint = $"https://api.stackexchange.com/2.3/users/{userId}/questions?order=desc&sort=activity&site=stackoverflow&filter=total";
        HttpResponseMessage questionResponse = await _httpClient.GetAsync(questionEndPoint);

        if (questionResponse.IsSuccessStatusCode)
        {
            string responseBody = await DecompressResponse(questionResponse);
            var questions = JsonConvert.DeserializeObject<dynamic>(responseBody).total;

            count = questions;
        }
        else
        {
            Console.WriteLine($"Error: {questionResponse.StatusCode} - {questionResponse.ReasonPhrase}");
        }

        return count;
    }

    // printing fitered users list

    private static async Task ProcessUsers(List<User> users)
    {
        foreach (var user in users)
        {
            Console.WriteLine($"User name: {user.DisplayName}");
            Console.WriteLine($"Location: {user.Location}");
            Console.WriteLine($"Reputation: {user.Reputation}");
            Console.WriteLine($"Answer count: {await AnswerCount(user.UserId)}");
            Console.WriteLine($"Question count: {await QuestionCount(user.UserId)}");
            foreach (var tag in user.Collectives)
            {
                Console.WriteLine($"TAG: {tag}");
            }
            Console.WriteLine($"Profile: {user.profileLink}");
            Console.WriteLine($"Avatar: {user.avatarLink}");

            Console.WriteLine();
        }
    }
}


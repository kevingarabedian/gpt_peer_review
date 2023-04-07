using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class GitHubPullRequestReviewer
{
    private readonly HttpClient _client;
    private readonly string _openAiApiKey;

    private readonly HashSet<int> _commentedPullRequests;

    public GitHubPullRequestReviewer(string accessToken, string openAiApiKey)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        _client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("GitHubPullRequestReviewer", "1.0"));

        _openAiApiKey = openAiApiKey;

        _commentedPullRequests = new HashSet<int>();
    }

    public async Task<string> CreatePullRequest(string repositoryName, string baseBranchName, string headBranchName, string title, string body, string diff)
    {
        var url = $"https://api.github.com/repos/{repositoryName}/pulls";
        var requestData = new
        {
            title = title,
            body = body,
            head = headBranchName,
            base = baseBranchName,
            maintainer_can_modify = true
        };

        var json = JsonConvert.SerializeObject(requestData);

        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create pull request: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<PullRequest>(content);

        var pullRequestId = result.Number;

        var comment = await GenerateComment(pullRequestId, diff);

        await PostComment(repositoryName, pullRequestId, comment);

        return result.HtmlUrl;
    }

    public async Task CheckForNewPullRequestsAndPostComments(string repositoryName)
    {
        var url = $"https://api.github.com/repos/{repositoryName}/pulls?state=open";

        var response = await _client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to retrieve pull requests: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var pullRequests = JsonConvert.DeserializeObject<List<PullRequest>>(content);

        foreach (var pullRequest in pullRequests)
        {
            if (!_commentedPullRequests.Contains(pullRequest.Number))
            {
                var comment = await GenerateComment(pullRequest.Number, pullRequest.Diff);

                await PostComment(repositoryName, pullRequest.Number, comment);

                _commentedPullRequests.Add(pullRequest.Number);
            }
        }
    }

    public async Task PostComment(string repositoryName, int pullRequestId, string comment)
    {
        var url = $"https://api.github.com/repos/{repositoryName}/issues/{pullRequestId}/comments";
        var requestData = new
        {
            body = comment
        };

        var json = JsonConvert.SerializeObject(requestData);

        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(url, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to post comment: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }

    private async Task<string> GenerateComment(int pullRequestId, string diff)
    {
        var url = "https://api.openai.com/v1/engines/davinci-codex/completions";
        
    var requestData = new
    {
        prompt = $"Suggest changes for pull request #{pullRequestId}: {diff}",
        max_tokens = 128,
        n = 1,
        stop = "."
    };

    var json = JsonConvert.SerializeObject(requestData);

    var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _openAiApiKey);

    var response = await _client.PostAsync(url, requestContent);

    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"Failed to generate comment: {response.StatusCode} - {response.ReasonPhrase}");
    }

    var content = await response.Content.ReadAsStringAsync();

    var result = JsonConvert.DeserializeObject<OpenAiResponse>(content);

    return result.Completions[0].Text.Trim();
}

private class PullRequest
{
    [JsonProperty("number")]
    public int Number { get; set; }

    [JsonProperty("html_url")]
    public string HtmlUrl { get; set; }

    [JsonProperty("diff_url")]
    public string DiffUrl { get; set; }

    public string Diff
    {
        get
        {
            var response = _client.GetAsync(DiffUrl).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve diff: {response.StatusCode} - {response.ReasonPhrase}");
            }

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}

private class OpenAiResponse
{
    [JsonProperty("completions")]
    public List<Completion> Completions { get; set; }
}

private class Completion
{
    [JsonProperty("text")]
    public string Text { get; set; }
}
}

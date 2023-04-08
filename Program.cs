
Console.WriteLine("Gpt Peer Review Agent!");
Console.WriteLine(Environment.GetEnvironmentVariable("GITHUB"));
GitHubPullRequestReviewer agent = new GitHubPullRequestReviewer(Environment.GetEnvironmentVariable("GITHUB"), Environment.GetEnvironmentVariable("CHATGPT"));

await agent.CheckForNewPullRequestsAndPostComments("kevingarabedian/test_repo");

Console.WriteLine("Done... Press any key to exit...");
Console.ReadKey();
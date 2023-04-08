# GitHub GPT Pull Request Peer Reviewer

Demonstrates the use of GPT to generate comments for GitHub pull requests. 

## How to Use

To use this code, you'll need a personal access token from GitHub and an OpenAI API key. 

### Personal Access Token from GitHub

1. Go to [https://github.com/settings/tokens](https://github.com/settings/tokens).
2. Click on "Generate new token".
3. Give the token a name.
4. Select the "repo" scope.
5. Click on "Generate token".
6. Copy the token.

### OpenAI API Key

1. Go to [https://beta.openai.com/docs/developer-quickstart/your-api-keys](https://beta.openai.com/docs/developer-quickstart/your-api-keys).
2. Click on "Create a new API key".
3. Give the API key a name.
4. Click on "Create API key".
5. Copy the API key.

### Environment Variables

On Windows, you can set environment variables using the following command:

```
setx GITHUB_ACCESS_TOKEN "your personal access token from GitHub"
setx OPENAI_API_KEY "your OpenAI API key"
```

On Linux, you can set environment variables using the following command:

```
export GITHUB_ACCESS_TOKEN="your personal access token from GitHub"
export OPENAI_API_KEY="your OpenAI API key"
```

### Code

Once you have your personal access token and OpenAI API key, you can create a new instance of the `GitHubPullRequestReviewer` class and call its methods to create pull requests and generate comments for existing pull requests. 

Here's an example:

```
var reviewer = new GitHubPullRequestReviewer(
    Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN"),
    Environment.GetEnvironmentVariable("OPENAI_API_KEY")
);

// If you want to create a pull request
var pullRequestUrl = await reviewer.CreatePullRequest(
    repositoryName: "your-repository-name",
    baseBranchName: "main",
    headBranchName: "new-feature-branch",
    title: "Add new feature",
    body: "This pull request adds a new feature.",
    diff: "The changes made in the new feature."
);

// Scan for pull requests to add review, you can set this to poll periodically, or you can implment a trigger such as pubsub, or webhook if desired.
await reviewer.CheckForNewPullRequestsAndPostComments("your-repository-name");
```

## Disclaimer

Use at your own risk. This is a proof of concept project and is not intended for production use. It is for educational and entertainment purposes only.

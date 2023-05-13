using CommandDotNet;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace sherlock_cs;
internal class Program
{
    private static SemaphoreSlim _concurrencyThrottler = new SemaphoreSlim(1);
    private static TimeSpan _timeout = TimeSpan.FromSeconds(1);
#pragma warning disable CS8618
    private static SherlockArguments _providedArgs;
#pragma warning restore CS8618

    static async Task Main(string[] args)
        => await new AppRunner<Program>().RunAsync(args);

    [DefaultCommand]
    public async Task Sherlock(SherlockArguments args)
    {
        _providedArgs = args;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var resourceFetcher = new HttpClient();
        var sherlockData = args.Local ? await File.ReadAllTextAsync("data.json") : await resourceFetcher.GetStringAsync("https://raw.githubusercontent.com/sherlock-project/sherlock/master/sherlock/resources/data.json");
        var sherlockEntries = JsonSerializer.Deserialize<Dictionary<string, SherlockEntry>>(sherlockData)!;

        _concurrencyThrottler = new SemaphoreSlim(args.Concurrency);
        _timeout = TimeSpan.FromSeconds(args.Timeout);

        if (!string.IsNullOrEmpty(args.OutputFolder) && !Directory.Exists(args.OutputFolder))
            Directory.CreateDirectory(args.OutputFolder);

        Console.WriteLine($"Checking username{(args.Usernames.Length > 1 ? "s" : "")} {string.Join(", ", args.Usernames)}");

        await Parallel.ForEachAsync(args.Usernames, new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Math.Max(Environment.ProcessorCount - 1, 1), int.MaxValue) }, async (username, _) =>
        {
            var tasks = new List<Task>();
            foreach (var (module, entry) in sherlockEntries)
            {
                var lModule = module;
                var lEntry = entry;
                if (entry.Nsfw && !args.Nsfw) continue;
                tasks.Add(HandleQuery(lModule, username, lEntry));
            }
            await Task.WhenAll(tasks);
        });
    }

    static async Task HandleQuery(string module, string username, SherlockEntry network)
    {
        var result = await QueryWebsite(username, network);
        if (_providedArgs.PrintFound && result == QueryResult.Claimed)
            Console.WriteLine($"[+] {module}: {network.Url.Replace("{}", username)}");
        if (_providedArgs.PrintAll && result != QueryResult.Claimed)
            Console.WriteLine($"[{GetQueryChar(result)}] {module}: {network.Url.Replace("{}", username)}");

        if(result == QueryResult.Claimed)
        {
            //this could theoretically cause problems because of a race condition, however chances are low so idc
            if (!string.IsNullOrEmpty(_providedArgs.OutputFolder))
                await File.AppendAllTextAsync(Path.Combine(_providedArgs.OutputFolder, $"{username}.txt"), network.Url.Replace("{}", username) + Environment.NewLine);
            if (!string.IsNullOrEmpty(_providedArgs.OutputFile))
                await File.AppendAllTextAsync(_providedArgs.OutputFile, network.Url.Replace("{}", username) + Environment.NewLine);
        }
    }

    static char GetQueryChar(QueryResult result)
    {
        switch(result) {
            case QueryResult.Claimed:
                return '+';
            case QueryResult.Available:
                return '-';
            case QueryResult.Unknown:
                return '?';
            case QueryResult.Illegal:
                return '!';
        }
        return 'ඞ';
    }

    static async Task<QueryResult> QueryWebsite(string username, SherlockEntry networkDetail)
    {
        await _concurrencyThrottler.WaitAsync();
        var result = QueryResult.Unknown;

        try
        {

            using var httpHandler = new HttpClientHandler
            {
                AllowAutoRedirect = networkDetail.ErrorType != "response_url"
            };
            using var http = new HttpClient(httpHandler) { Timeout = _timeout };

            http.DefaultRequestHeaders.Add("User-Agent", _providedArgs.UserAgent);

            if (networkDetail.Headers is not null)
            {
                foreach (var header in networkDetail.Headers)
                    http.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }


            if (networkDetail.UsernamePattern is not null && Regex.Match(username, networkDetail.UsernamePattern) is null)
                return QueryResult.Illegal;

            HttpContent? content = null;


            if (networkDetail.Payload is not null)
            {
                /// lazy way of achieving this, a better way would be to recursively loop over the items in there but this will work for now.
                /// I'll fix it if it ever becomes an issue
                content = new StringContent(JsonSerializer.Serialize(networkDetail.Payload).Replace("{}", username), Encoding.UTF8, "application/json");
            }

            var res = await http.SendAsync(new HttpRequestMessage
            {
                Method = new HttpMethod(networkDetail.RequestMethod ?? "GET"),
                RequestUri = new Uri((networkDetail.ProbeUrl ?? networkDetail.Url).Replace("{}", username)),
                Content = content
            });

            string responseBody = await res.Content.ReadAsStringAsync();

            switch (networkDetail.ErrorType)
            {
                case "message":
                    bool errorFound = true;

                    if (networkDetail.ErrorMessage!.Value.ValueKind == JsonValueKind.String)
                    {
                        if (responseBody.Contains(networkDetail.ErrorMessage!.Value.GetString()!))
                            errorFound = false;
                    }

                    if (networkDetail.ErrorMessage!.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var error in networkDetail.ErrorMessage!.Value.EnumerateArray())
                        {
                            if (responseBody.Contains(error.GetString()!))
                                errorFound = false;
                        }
                    }

                    result = errorFound ? QueryResult.Claimed : QueryResult.Available;
                    break;
                case "status_code":
                    if (networkDetail.ErrorCode == (int)res.StatusCode)
                        result = QueryResult.Available;
                    else if (!((int)res.StatusCode >= 300 || (int)res.StatusCode < 200))
                        result = QueryResult.Claimed;
                    else
                        result = QueryResult.Available;
                    break;
                case "response_url":
                    result = ((int)res.StatusCode <= 200 && (int)res.StatusCode < 300) ? QueryResult.Claimed : QueryResult.Available;
                    break;
            }

        }
        catch (Exception e)
        {
            result = QueryResult.Illegal;
        }
        finally
        {
            _concurrencyThrottler.Release();
        }

        return result;
    }
}
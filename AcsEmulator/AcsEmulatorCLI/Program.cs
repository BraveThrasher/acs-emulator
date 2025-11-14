using System.CommandLine;
using System.Diagnostics;

class Program
{
	static async Task<int> Main(string[] args)
	{
		RootCommand rootCommand = new("Emulator for Azure Communication Services");

		Command run = new("run", description: "Run the emulator.");
		Command openApi = new("openApi", description: "Open the emulator's API in Swagger UI.");
		Command openDB = new("openDB", description: "Open the emulator's sqlite database.");
		Command openUI = new("openUI", description: "Open the emulator UI.");
		Command clean = new("clean", description: "Clean all data and reset the emulator state.");
		Command connectionString = new("connectionString", description: "Get the ACS connection string for the emulator.");
		Command repo = new("repo", description: "Open code repository.");

		run.SetAction(Run);
		openApi.SetAction(OpenSwaggerUI);
		openDB.SetAction(OpenDB);
		openUI.SetAction(OpenUI);
		clean.SetAction(CleanDB);
		connectionString.SetAction(GetConnectionString);
		repo.SetAction(OpenRepo);

		rootCommand.Subcommands.Add(run);
		rootCommand.Subcommands.Add(openApi);
		rootCommand.Subcommands.Add(openDB);
		rootCommand.Subcommands.Add(openUI);
		rootCommand.Subcommands.Add(clean);
		rootCommand.Subcommands.Add(connectionString);
		rootCommand.Subcommands.Add(repo);

        return await rootCommand.Parse(args).InvokeAsync();
	}

	private static async Task Run(ParseResult parseResult, CancellationToken cancellationToken)
	{
		_ = StartEmulator(cancellationToken);
		OpenUI(parseResult);
	}

	private static Task StartEmulator(CancellationToken cancellationToken)
	{
		using Process proc = new();
		proc.StartInfo.WorkingDirectory = AppContext.BaseDirectory;
		proc.StartInfo.FileName = "dotnet";
		proc.StartInfo.Arguments = "--additional-deps AcsEmulatorCLI.deps.json AcsEmulatorAPI.dll --urls=https://localhost/";
		proc.StartInfo.UseShellExecute = true;
		proc.Start();
		return proc.WaitForExitAsync(cancellationToken);
	}

	private static void OpenSwaggerUI(ParseResult parseResult) => Process.Start(new ProcessStartInfo("https://localhost/swagger") { UseShellExecute = true });

	private static void OpenDB(ParseResult parseResult)
	{
		try
		{
			Process.Start(new ProcessStartInfo("AcsEmulator.db") { UseShellExecute = true, WorkingDirectory = AppContext.BaseDirectory });
		}
		catch (Exception ex)
		{
            if (ex.Message.Contains("cannot find the file"))
				Console.WriteLine("Please first run 'acs-emulator run' to create the database.");
			else
				Console.WriteLine(ex.Message);
		}
	}

	private static void OpenUI(ParseResult parseResult) => Process.Start(new ProcessStartInfo("https://localhost") { UseShellExecute = true });

    private static void OpenRepo(ParseResult parseResult) => Process.Start(new ProcessStartInfo("https://github.com/DominikMe/acs-emulator") { UseShellExecute = true });

	private static void CleanDB(ParseResult parseResult)
	{
		var path = $"{AppContext.BaseDirectory}/AcsEmulator.db";
		if (File.Exists(path))
			File.Delete(path);
	}

	private static void GetConnectionString(ParseResult parseResult) => Console.WriteLine("endpoint=https://localhost/;accessKey=pw==");

}

namespace DatacampAICoordinator.Infrastructure.Services;

using System.IO;
using System.Linq;

public static class SolutionPathHelper
{
    public static string GetSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir != null && !dir.GetFiles("*.sln").Any())
            dir = dir.Parent;

        if (dir == null)
            throw new DirectoryNotFoundException("Could not find the solution root (no .sln file found).");

        return dir.FullName;
    }

    public static string GetDatabasePath(string dbFileName = "datacamp.db")
    {
        var solutionRoot = GetSolutionRoot();
        Console.WriteLine(solutionRoot);
        return Path.Combine(solutionRoot, dbFileName);
    }
}
#region pre
//using System.CommandLine;
//var bundleOption = new Option<FileInfo>("--output", "File path and name");
//var bundleCommand = new Command("bundle", "Bundle code files to a single file");
//bundleCommand.AddOption(bundleOption);
//bundleCommand.SetHandler((output) =>
//{
//    try
//    {
//        File.Create(output.FullName);
//        Console.WriteLine("File was created");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine("Error : The path isnot valid");
//    }

//}, bundleOption);
//var rootCommand = new RootCommand("Root command for file Bundler CLI");
//rootCommand.AddCommand(bundleCommand);
//rootCommand.InvokeAsync(args);
#endregion

using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;
// Function to prompt the user for input
string PromptForInput(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine();
}

// Function to prompt the user for yes/no input
bool PromptForYesNo(string prompt)
{
    while (true)
    {
        Console.Write($"{prompt}");
        string input = Console.ReadLine().ToLower();
        if (input == "y" || input == "n")
        {
            return input == "y";
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
        }
    }
}

//Function to GetExtensionOfLanguages
 string[] GetExtensionOfLanguages(string language, string[] extensions, string[] allLanguages)
{
    if (language.Equals("all"))
        return extensions;

    string[] selectedExtensions = language
        .Split(' ')
        .Join(allLanguages.Zip(extensions, (lang, ext) => new { Language = lang, Extension = ext }),
              lang => lang,
              langExt => langExt.Language,
              (lang, langExt) => langExt.Extension)
        .ToArray();

    return selectedExtensions.Length > 0 ? selectedExtensions : extensions;
}

var bundleCommand = new Command("bundle", "Bundle code files into a single file based on selected languages.");
var rspCommand = new Command("create-rsp", "Response file");

var languageOption = new Option<string>(new[] { "--language", "-l" }, "Programming languages to include in the bundle, Use 'all' for all languages") { IsRequired = true };
var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "Output file name. If not provided, the bundle will be saved in the current directory with a default name.");
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Include source code origin as a comment");
var sortOption = new Option<string>(new[] { "--sort", "-s" }, "Sort order for code file by letters or type");
var removeEmptyLinesOption = new Option<bool>(new[] { "--remove-empty-lines", "-r" }, "Remove empty lines from code files");
var authorOption = new Option<string>(new[] { "--author", "-a" }, "Registering the name of the author of the file ");

outputOption.SetDefaultValue(new FileInfo($"ZipCode_{DateTime.Now:yyyyMMddHHmmss}.txt") );
noteOption.SetDefaultValue(false);
sortOption.SetDefaultValue("letter");
removeEmptyLinesOption.SetDefaultValue(false);

bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

string currentPath = Directory.GetCurrentDirectory();
List<string> allFolders = Directory.GetFiles(currentPath, "", SearchOption.AllDirectories)
.Where(file => !file.Contains("bin") && !file.Contains("Debug")).ToList();
string[] codeFileLanguages = { "c#", "c", "c++", "java", "html", "css", "javascript", "pyton", "ruby", "swift", "php", "jsx" };
string[] codeFileExtensions = { ".cs", ".c", ".cpp", ".java", ".html", ".css", ".js", ".py", ".rb", ".swift", ".php" ,".jsx"};

bundleCommand.SetHandler((language, output, sort, note, removeEmptyLines, author) =>
{
    try
    {
        #region  Allfiles
        //string[] ignoredFolders = { "bin", "debug" };

        //string[] selectedLanguages;
        //if (language.ToLower() == "all")
        //{
        //    selectedLanguages = Directory.GetDirectories(Directory.GetCurrentDirectory())
        //        .Where(dir => !ignoredFolders.Contains(Path.GetFileName(dir).ToLower()))
        //        .SelectMany(dir => Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
        //            .Select(file => Path.GetExtension(file).TrimStart('.').ToLower()))
        //        .Distinct()
        //        .Where(l => !string.IsNullOrWhiteSpace(l))
        //        .ToArray();
        //    Console.WriteLine(selectedLanguages.Count());
        //    foreach (var i in selectedLanguages)
        //    {
        //        Console.WriteLine(i);
        //    }
        //}
        //else
        //{
        //    selectedLanguages = language.Split(',')
        //        .Select(l => l.Trim().ToLower())
        //        .Where(l => !string.IsNullOrWhiteSpace(l))
        //        .ToArray();
        //    foreach (var i in selectedLanguages)
        //    {
        //        Console.WriteLine(i);
        //    }
        //}


        //Console.WriteLine($"Output File: {output.FullName}");

        //var codeFiles = Directory.GetDirectories(Directory.GetCurrentDirectory())
        //    .Where(dir => !ignoredFolders.Contains(Path.GetFileName(dir).ToLower()))
        //    .SelectMany(dir => Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
        //        .Where(file => selectedLanguages.Contains(Path.GetExtension(file).TrimStart('.').ToLower()))
        //        .Select(file => File.ReadAllText(file)))
        //    .ToList();
        #endregion

        string[] languages = GetExtensionOfLanguages(language, codeFileExtensions, codeFileLanguages);
        string[] codeFiles = allFolders.Where(file => languages
        .Contains(Path.GetExtension(file))).ToArray();

        if (codeFiles.Any())
        {
            using (StreamWriter writer = new StreamWriter(output.Name))
            {
                if (!string.IsNullOrEmpty(author))
                {
                    author = author.Replace(" ", "-");
                    writer.WriteLine($"---------------------Author: {author}---------------------");
                }

                if (note)
                {
                    writer.WriteLine($"// Source code extracted from : {Directory.GetCurrentDirectory()}");
                }

                if (sort.ToLower() == "type")
                {
                    codeFiles = codeFiles.OrderBy(code => Path.GetExtension(code)).ToArray();
                }
                else
                {
                    codeFiles = codeFiles.OrderBy(code => Path.GetFileNameWithoutExtension(code)).ToArray();
                }
                foreach (var file in codeFiles)
                {
                    string content = System.IO.File.ReadAllText(file);

                    if (removeEmptyLines)
                    {
                        content = string.Join(Environment.NewLine, content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));
                    }

                    writer.WriteLine($"---- {Path.GetFileName(file)}---");
                    writer.WriteLine(content);
                    writer.WriteLine();
                }

                Console.WriteLine($"File {output.Name} was created successfully");
                Console.WriteLine(codeFiles.Count());
            }
        }
        else
        {
            Console.WriteLine("Error: No files found for the specified languages.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

}, languageOption, outputOption, sortOption, noteOption, removeEmptyLinesOption, authorOption);
rspCommand.SetHandler(() =>
{
    try
    {
        Console.WriteLine("Creating response file...");
        string language = PromptForInput("Enter programming languages to include (use 'all' for all languages): ");
        string outputFilePath = PromptForInput("Enter output file name (or leave empty for default): ");
        bool note = PromptForYesNo("Include source code origin as a comment (y/n): ");
        string sort = PromptForInput("Enter sort order for code file by letters or type:(letters/type)");
        bool removeEmptyLines = PromptForYesNo("Remove empty lines from code files (y/n): ");
        string author = PromptForInput("Enter the name of the author of the file: ");

        var responseFile = new FileInfo("responseFile.rsp");

        using (StreamWriter rspWriter = new StreamWriter(responseFile.FullName))
        {
            rspWriter.WriteLine($"--language {language}");

            if (!string.IsNullOrEmpty(outputFilePath))
            {
                rspWriter.WriteLine($"--output {outputFilePath}");
            }

            if (note)
            {
                rspWriter.WriteLine("--note");
            }

            if (!string.IsNullOrEmpty(sort))
            {
                rspWriter.WriteLine($"--sort {sort}");
            }

            if (removeEmptyLines)
            {
                rspWriter.WriteLine("--remove-empty-lines");
            }

            if (!string.IsNullOrEmpty(author))
            {
                rspWriter.WriteLine($"--author {author}");
            }
        }

        Console.WriteLine($"Response file '{responseFile.Name}' created successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
});

var RootCommand = new RootCommand("Root command for file bundler for CLI");
RootCommand.AddCommand(bundleCommand);
RootCommand.AddCommand(rspCommand);
RootCommand.InvokeAsync(args);


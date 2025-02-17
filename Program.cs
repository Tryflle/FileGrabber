namespace FileGrabber {
    class Program {
        static List<FileInfo> Files = new List<FileInfo>();
        private static List<FileInfo> SubDirectoryFiles = new List<FileInfo>();
        static string Dir = Directory.GetCurrentDirectory();
        private static string DirToMove;
        private static string SearchQuery;

        static Dictionary<string, long> FileToFileSize = new Dictionary<string, long>();
        // Todo:
        // Some kind of ui probably with Avalonia, multi directory, options to do more things to files etc.

        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine(
                    "Usage:\n Search: \"-search:query\" \n File types: zip, rar, exe, etc. \n All files types: * \n Specify directory: \"-dir:C:\\Example\\Folder\" \n Full path: -full-path \n Move all results to folder \"-move:C:\\destination\\folder\" \n Also search subdirectories: -search-subdir");
                Console.WriteLine("Example: 7z zip rar (optional) \"-dir:C:\\Dir\\Example\"");
                return;
            }

            var fullPath = false;
            var moving = false;
            var searching = false;
            var ignoreSubDirectories = true;

            Console.WriteLine("Looking for files with:");
            foreach (var arg in args) {
                Console.WriteLine(arg);
                if (arg.Contains("-search-subdir")) ignoreSubDirectories = false;
                if (arg.StartsWith("-search:")) {
                    searching = true;
                    SearchQuery = arg.Replace("-search:", "");
                }

                if (arg.StartsWith("-dir:")) Dir = arg.Replace("-dir:", "");
                if (arg.StartsWith("-full-path")) fullPath = true;
                if (arg.StartsWith("-move")) {
                    moving = true;
                    DirToMove = arg.Replace("-move:", "");
                }
            }

            DirectoryInfo di = new DirectoryInfo(Dir);
            FileInfo[] allFiles = di.GetFiles();

            if (!ignoreSubDirectories) {
                GetSubFiles(di.FullName);

                foreach (var file in SubDirectoryFiles) {
                    var fileTypeCorrect = args.Any(s => file.Name.EndsWith(s));
                    if (searching && !file.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    if (fileTypeCorrect && !args.Contains("*")) Files.Add(file);
                    if (args.Contains("*")) Files.Add(file);
                }
            }

            foreach (var file in allFiles) {
                var fileTypeCorrect = args.Any(s => file.Name.EndsWith(s));
                if (searching && !file.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                if (fileTypeCorrect && !args.Contains("*")) Files.Add(file);
                if (args.Contains("*")) Files.Add(file);
            }

            if (Files.Count == 0) {
                Console.WriteLine("No files of the requirements could be found in " + Dir);
                return;
            }

            Console.WriteLine("Found " + Files.Count + " File" + (Files.Count == 1 ? "" : "s"));

            foreach (var file in Files) {
                
                if (FileToFileSize.ContainsKey(file.FullName) || FileToFileSize.ContainsKey(file.Name)) { 
                    Console.WriteLine($"File {file.FullName} already exists in the dictionary. Skipping.");
                    continue;
                }
                if (fullPath) {
                    FileToFileSize.Add(file.FullName, file.Length);
                    Console.WriteLine("fullPath");
                }
                else FileToFileSize.Add(file.Name, file.Length);
                
            }

            var sortedFiles = FileToFileSize.OrderBy(pair => pair.Value);
            foreach (var file in sortedFiles) {
                Console.WriteLine($"{file.Key} - {StorageSize(file.Value)}");
            }

            if (moving) {
                Console.WriteLine($"You are moving files to {DirToMove}");
                Directory.CreateDirectory(DirToMove);
                foreach (var file in Files) {
                    File.Move(file.FullName, Path.Combine(DirToMove, file.Name));
                    Console.WriteLine(Path.Combine(DirToMove, file.Name));
                }
            }
        }

        static string StorageSize(double length) {
            var GB = Math.Pow(1024, 3);
            var MB = Math.Pow(1024, 2);

            if (length >= GB) return $"{Math.Round(length / GB, 1)} GB";
            if (length >= MB) return $"{Math.Round(length / MB, 1)} MB";
            if (length >= 1024) return $"{Math.Round(length / 1024, 1)} KB";
            return $"{(int)length} B";
        }

        static void GetSubFiles(string dir) {
            DirectoryInfo di = new DirectoryInfo(dir);
            FileInfo[] allFiles = di.GetFiles();
            SubDirectoryFiles.AddRange(allFiles);
            foreach (var subdir in di.GetDirectories()) {
                GetSubFiles(subdir.FullName);
            }
        }
    }
}
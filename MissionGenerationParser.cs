namespace Hacknet
{
    public static class MissionGenerationParser
    {
        public static string Path;
        public static string File;
        public static string Comp;
        public static string Client;
        public static string Target;
        public static string Other;

        public static void init()
        {
            string str;
            Target = str = "UNKNOWN";
            Client = str;
            File = str;
            Path = str;
            Comp = "firstGeneratedNode";
        }

        public static string parse(string input)
        {
            return
                input.Replace("#PATH#", Path)
                    .Replace("#FILE#", File)
                    .Replace("#COMP#", Comp)
                    .Replace("#CLIENT#", Client)
                    .Replace("#TARGET#", Target)
                    .Replace("#OTHER#", Other)
                    .Replace("#LC_CLIENT#", Client.Replace(' ', '_').ToLower());
        }
    }
}
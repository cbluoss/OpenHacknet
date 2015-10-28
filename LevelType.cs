namespace Hacknet
{
    public struct LevelType
    {
        public int NumOfPuzzles;
        public int NumOfBackgrounds;
        public string name;

        public LevelType(int puzzles, int bgs, string lvlname)
        {
            NumOfPuzzles = puzzles;
            NumOfBackgrounds = bgs;
            name = lvlname;
        }
    }
}
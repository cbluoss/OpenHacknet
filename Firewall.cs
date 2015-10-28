using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Hacknet
{
    public class Firewall
    {
        private const int MIN_SOLUTION_LENGTH = 6;
        private const int OUTPUT_LINE_WIDTH = 20;
        private const int CHARS_SOLVED_PER_PASS = 3;
        private const string SOLVED_CHAR = "0";
        private readonly float additionalDelay;
        private int analysisPasses;
        private readonly int complexity = 1;
        private string solution;
        private readonly int solutionLength = 6;
        public bool solved;

        public Firewall()
        {
            generateRandomSolution();
        }

        public Firewall(int complexity)
        {
            this.complexity = complexity;
            solutionLength = 6 + complexity;
            generateRandomSolution();
        }

        public Firewall(int complexity, string solution)
        {
            this.complexity = complexity;
            this.solution = solution;
            solutionLength = solution.Length;
        }

        public Firewall(int complexity, string solution, float additionalTime)
        {
            this.complexity = complexity;
            this.solution = solution;
            additionalDelay = additionalTime;
            solutionLength = solution.Length;
        }

        private void generateRandomSolution()
        {
            var stringBuilder = new StringBuilder(solutionLength);
            for (var index = 0; index < solutionLength; ++index)
                stringBuilder.Append(Utils.getRandomChar());
            solution = stringBuilder.ToString().ToUpperInvariant();
        }

        public static Firewall load(XmlReader reader)
        {
            while (reader.Name != "firewall")
                reader.Read();
            var complexity = 0;
            string solution = null;
            var additionalTime = 0.0f;
            if (reader.MoveToAttribute("complexity"))
                complexity = reader.ReadContentAsInt();
            if (reader.MoveToAttribute("solution"))
                solution = reader.ReadContentAsString();
            if (reader.MoveToAttribute("additionalDelay"))
                additionalTime = reader.ReadContentAsFloat();
            return new Firewall(complexity, solution, additionalTime);
        }

        public string getSaveString()
        {
            return "<firewall complexity=\"" + complexity + "\" solution=\"" + solution + "\" additionalDelay=\"" +
                   additionalDelay.ToString(CultureInfo.InvariantCulture) + "\" />";
        }

        public void resetSolutionProgress()
        {
            analysisPasses = 0;
        }

        public bool attemptSolve(string attempt, object os)
        {
            if (attempt.Length != solution.Length)
            {
                var str = attempt.Length < solution.Length ? "Too few charachters" : "Too many charachters";
                ((OS) os).write("Solution Incorrect Length - " + str);
            }
            else if (attempt.ToLower().Equals(solution.ToLower()))
            {
                solved = true;
                return true;
            }
            return false;
        }

        public void writeAnalyzePass(object os_object, object target_object)
        {
            var target = (Computer) target_object;
            var os = (OS) os_object;
            if (target.firewallAnalysisInProgress)
            {
                os.write("-Analysis already in Progress-");
            }
            else
            {
                os.delayer.PostAnimation(generateOutputPass(analysisPasses, os, target));
                ++analysisPasses;
            }
        }

        private IEnumerator<ActionDelayer.Condition> generateOutputPass(int pass, OS os, Computer target)
        {
            target.firewallAnalysisInProgress = true;
            os.write("Firewall Analysis Pass " + analysisPasses + "\n");
            yield return ActionDelayer.Wait(0.03);
            os.write("--------------------");
            yield return ActionDelayer.Wait(0.03);
            var preceedeString = "     ";
            var secondsDelayPerLine = 0.08 + 0.06*pass + additionalDelay;
            for (var i = 0; i < solutionLength; ++i)
            {
                os.write(preceedeString + generateOutputLine(i));
                yield return ActionDelayer.Wait(secondsDelayPerLine);
            }
            os.write("--------------------\n");
            target.firewallAnalysisInProgress = false;
        }

        private string generateOutputLine(int location)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < 20; ++index)
                stringBuilder.Append("0");
            var num = 20 - 3*analysisPasses;
            for (var index = 0; index < num; ++index)
                stringBuilder[index] = string.Concat(Utils.getRandomChar()).ToLower()[0];
            var index1 = Utils.random.Next(stringBuilder.Length);
            if (location < solution.Length)
                stringBuilder[index1] = solution[location];
            var index2 = 0;
            while (index2 < stringBuilder.Length)
            {
                stringBuilder.Insert(index2, " ");
                index2 += 2;
            }
            return stringBuilder.ToString();
        }

        public override bool Equals(object obj)
        {
            var firewall = obj as Firewall;
            if (firewall != null && firewall.additionalDelay == (double) additionalDelay &&
                firewall.complexity == complexity)
                return firewall.solution == solution;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "Firewall: solution\"" + solution + "\" - time:" + additionalDelay + " - complexity:" +
                   complexity;
        }
    }
}
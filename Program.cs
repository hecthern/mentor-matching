using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MentorMatch
{
    /// <summary>
    /// The bits folks are interested in mentoring or being mentored in
    /// </summary>
    public class Interest
    {
        public bool CareerAdvice;
        public bool SkillsGrowth;
        public bool ExpandNetwork;
        public bool LifeAdvice;
        public bool SocialConnection;
        public bool LearnDiscipline;
        public bool LearnTech;
        public bool NewCareer;
        public string Other;

        /// <summary>
        /// ctors
        /// </summary>
        public Interest()
        {
        }

        public Interest(string CareerAdvice,
            string SkillsGrowth,
            string ExpandNetwork,
            string LifeAdvice,
            string SocialConnection,
            string LearnDiscipline,
            string LearnTech,
            string NewCareer,
            string Other)
        {
            this.CareerAdvice = bool.Parse(CareerAdvice);
            this.SkillsGrowth = bool.Parse(SkillsGrowth);
            this.ExpandNetwork = bool.Parse(ExpandNetwork);
            this.LifeAdvice = bool.Parse(LifeAdvice);
            this.SocialConnection = bool.Parse(SocialConnection);
            this.LearnDiscipline = bool.Parse(LearnDiscipline);
            this.LearnTech = bool.Parse(LearnTech);
            this.NewCareer = bool.Parse(NewCareer);
            this.Other = Other;
        }

        /// <summary>
        /// Counts overlapping bits that are set to true
        /// </summary>
        public static int Overlap(Interest i1, Interest i2)
        {
            int overlap = 0;
            if (i1.CareerAdvice && (i1.CareerAdvice == i2.CareerAdvice)) { overlap++; }
            if (i1.SkillsGrowth && (i1.SkillsGrowth == i2.SkillsGrowth)) { overlap++; }
            if (i1.ExpandNetwork && (i1.ExpandNetwork == i2.ExpandNetwork)) { overlap++; }
            if (i1.LifeAdvice && (i1.LifeAdvice == i2.LifeAdvice)) { overlap++; }
            if (i1.SocialConnection && (i1.SocialConnection == i2.SocialConnection)) { overlap++; }
            if (i1.LearnDiscipline && (i1.LearnDiscipline == i2.LearnDiscipline)) { overlap++; }
            if (i1.LearnTech && (i1.LearnTech == i2.LearnTech)) { overlap++; }
            if (i1.NewCareer && (i1.NewCareer == i2.NewCareer)) { overlap++; }

            return overlap;
        }

        /// <summary>
        /// For dumping back into a CSV we can re-import into Excel
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},\"{8}\"",
                this.CareerAdvice ? "1" : "0",
                this.SkillsGrowth ? "1" : "0",
                this.ExpandNetwork ? "1" : "0",
                this.LifeAdvice ? "1" : "0",
                this.SocialConnection ? "1" : "0",
                this.LearnDiscipline ? "1" : "0",
                this.LearnTech ? "1" : "0",
                this.NewCareer ? "1" : "0",
                this.Other);
        }
    }

    /// <summary>
    /// Managing each person's data
    /// </summary>
    public class Person
    {
        // Data read from survey
        public int Id;
        public string Name;
        public string Email;
        public string Role;
        public int Band;
        public bool WantMentor;
        public Interest WantMentorItems;
        public bool CanMentor;
        public Interest CanMentorItems;

        // This person is mentored
        public bool IsMentored;

        /// <summary>
        /// Number of folks this person is mentoring
        /// </summary>
        public int IsMentoring;

        /// <summary>
        /// Parsing the raw csv data exported from Excel
        /// </summary>
        public Person(string data)
        {
            var entries = data.Split(",");

            this.CanMentorItems = new Interest();
            this.WantMentorItems = new Interest();
            // Counting columns
            this.Id = int.Parse(entries[0]);
            this.Role = entries[1].ToLower();

            if (!int.TryParse(entries[2], out this.Band))
            { 
                this.Band = -1;
            }

            this.Email = entries[3];
            this.Name = entries[4];

            this.WantMentor = bool.Parse(entries[5]);
            this.CanMentor = bool.Parse(entries[6]);

            this.WantMentorItems = new Interest(
                entries[7],
                entries[8],
                entries[9],
                entries[10],
                entries[11],
                entries[12],
                entries[13],
                entries[14],
                null);

            // Empty field 15 - no data

            this.CanMentorItems = new Interest(
                entries[16],
                entries[17],
                entries[18],
                entries[19],
                entries[20],
                entries[21],
                entries[22],
                entries[23],
                null);
        }

        /// <summary>
        /// Dump contents as CSV so we can re-import into excel
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\"{0},\"", this.Id);

            sb.AppendFormat("\"{0},\"", this.Name);
            sb.AppendFormat("\"{0},\"", this.Email);

            sb.AppendFormat("\"{0},\"", this.Role);
            sb.AppendFormat("\"{0},\"", this.Band);

            sb.AppendFormat("\"{0},\"", this.WantMentor);
            sb.AppendFormat("\"{0},\"", this.WantMentorItems.ToString());

            sb.AppendFormat("\"{0},\"", this.CanMentor);
            sb.AppendFormat("\"{0},\"", this.CanMentorItems.ToString());

            sb.AppendFormat("\"{0},\"", this.IsMentored);
            sb.AppendFormat("\"{0},\"", this.IsMentoring);

            return sb.ToString();
        }
    }

    class Program
    {
        // Run the model with a maximum of this many mentees per mentor.
        const int MaxMentees = 3;

        static void Main(string[] args)
        {
            // Parse the raw survey data into fields, this was the first step in the process.
            //ParseSurveyFile("cando");
            //ParseSurveyFile("want");

            // Re-exported parts of the excel doc to the minimal set of data needed for matching.
            var personList = ParseMinimalSheet(@"C:\Users\peterzen\OneDrive - Microsoft\Desktop\MentorMinimal.txt");

            // Store off the list of eventual matches.
            List<Tuple<Person, Person>> matches = new List<Tuple<Person, Person>>();

            // Let's figure out how many different disciplines are represented, we'll later match by discipline
            var disciplines = GetDisciplines(personList);

            foreach (var discipline in disciplines)
            {
                var disciplinedPeople = GetPeopleByRole(discipline, personList);

                for (int pass = 1; pass <= MaxMentees; pass++)
                {
                    var match = ProcessListOfUsers(disciplinedPeople, pass);
                    matches.AddRange(match);
                }

                var noMentor = disciplinedPeople.Where(x => x.IsMentored);
                var NotMentoring = disciplinedPeople.Where(x => x.IsMentoring == 0);
            }

            // Sort by survey ID and process the output in that order so we can re-import it into excel and have it line up.
            var orderedMatches = matches.OrderBy(x => x.Item1.Id);
            int id = 1;

            foreach (var match in orderedMatches)
            {
                while (id != match.Item1.Id)
                {
                    Console.WriteLine("{0},,,,,,,,", id);
                    id++;
                }

                Console.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    match.Item1.Id,

                    match.Item1.Name,
                    match.Item1.Email,
                    match.Item1.Band,

                    match.Item2.Name,
                    match.Item2.Email,
                    match.Item2.Band,

                    match.Item2.IsMentoring,
                    Interest.Overlap(match.Item1.WantMentorItems, match.Item2.CanMentorItems));

                id++;
            }

        }

        /// <summary>
        /// Read in the minimal set of excel data to parse for matching, parse each entry.
        /// </summary>
        private static List<Person> ParseMinimalSheet(string filename)
        {
            var fileStream = new StreamReader(filename);
            List<Person> people = new List<Person>();

            while (!fileStream.EndOfStream)
            {
                string line = fileStream.ReadLine();
                Person p = new Person(line);
                people.Add(p);
            }

            return people;
        }

        /// <summary>
        /// Return a unique list of disciplines.  A comparator class may be more elegant so I could call people.Distinct()
        /// It's been a long day.
        /// </summary>
        public static List<string> GetDisciplines(List<Person> people)
        {
            List<string> Roles = new List<string>();
            foreach (var p in people)
            {
                if (!Roles.Contains(p.Role))
                {
                    Roles.Add(p.Role.ToLower());
                }
            }

            return Roles;
        }

        /// <summary>
        /// Return list of users matching a given role.  No other filtering is done.
        /// </summary>
        public static List<Person> GetPeopleByRole(string role, List<Person> people)
        {
            return people.Where(x => x.Role.Contains(role)).ToList();
        }

        /// <summary>
        /// Given a list of users, match them based on requests/willingness
        /// </summary>
        public static List<Tuple<Person, Person>> ProcessListOfUsers(List<Person> people, int maxMentors)
        {
            var MentorList = people.Where(x => x.CanMentor && x.IsMentoring < maxMentors).ToList();
            var MenteeList = people.Where(x => x.WantMentor && !x.IsMentored).ToList();

            List<Tuple<Person, Person>> mentorMatch = new List<Tuple<Person, Person>>();

            foreach(var p in MenteeList)
            {
                var mentor = FindMentor(p, MentorList, maxMentors);
                if (mentor != null)
                {
                    // Record output list, set person as mentoring
                    mentorMatch.Add(new Tuple<Person, Person>(p, mentor));

                    mentor.IsMentoring++;
                    p.IsMentored = true;
                }
            } 

            return mentorMatch;
        }

        /// <summary>
        /// Given a person, find a mentor for that person.
        /// </summary>
        public static Person FindMentor(Person p, List<Person> people, int maxMentors)
        {
            // Filter by band first
            var mentors = people.Where(x => x.Band > p.Band).ToList();
            Person mentorTarget = null;

            // It's late, let's just brute force it.
            int maxOverlap = 0;
            foreach (var mentor in mentors)
            {
                int overlap = Interest.Overlap(p.WantMentorItems, mentor.CanMentorItems); 
                if (overlap > maxOverlap && mentor.IsMentoring < maxMentors)
                {
                    mentorTarget = mentor;
                    maxOverlap = overlap;
                }
            }

            return mentorTarget;
        }

        /// <summary>
        /// Survey parsed out into a text and read into a collection of objects.  This was used to convert the 
        /// strings from the survey into a collection of bits for each item.  Manual process to export the data,
        /// create a text file and parse it out then re-paste it back into excel.
        /// Sample inputs:
        ///     Career advice;Skills, growth, problem solving;Social connection;Learning about other career journeys;
        ///     Career advice; Skills, growth, problem solving; Expanding my network;Learning about other career journeys;
        ///     Social connection; Skills, growth, problem solving; Career advice; Just want to elaborate that I can offer an EIC perspective on some of these topics; Learning about other career journeys;
        /// Note that there is no fixed order for the fields.
        /// </summary>
        private static void ParseSurveyFile(string filename)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var fileStream = new StreamReader("c:\\temp\\" + filename + ".txt");  // You know this is fancy if you're reading files out of temp.

            while (!fileStream.EndOfStream)
            {
                Interest interest = new Interest();
                string line = fileStream.ReadLine();

                // Maintain blank lines so that output rows line up with input rows
                if (String.IsNullOrEmpty(line))
                {
                    stringBuilder.AppendLine(interest.ToString());
                    continue;
                }

                var items = line.Split(";");

                foreach (var item in items)
                {
                    var target = item.Trim();

                    // Convert to individual fields
                    if (target.Contains("Career advice"))
                        interest.CareerAdvice = true;
                    else if (target.Contains("Skills, growth, problem solving"))
                        interest.SkillsGrowth = true;
                    else if (target.Contains("Expanding my network"))
                        interest.ExpandNetwork = true;
                    else if (target.Contains("Life advice"))
                        interest.LifeAdvice = true;
                    else if (target.Contains("Social connection"))
                        interest.SocialConnection = true;
                    else if (target.Contains("Learning a new discipline"))
                        interest.LearnDiscipline = true;
                    else if (target.Contains("Learning a new technology"))
                        interest.LearnTech = true;
                    else if (target.Contains("Exploring a new career journey") || target.Contains("Learning about other career journeys"))
                        interest.NewCareer = true;
                    else if (!String.IsNullOrEmpty(target))  // only one freeform field was present
                        interest.Other = target;
                }

                stringBuilder.AppendLine(interest.ToString());
            }

            var streamWriter = new StreamWriter("c:\\temp\\" + filename + ".csv");
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
            Console.WriteLine(stringBuilder.ToString());
        }
    }
}

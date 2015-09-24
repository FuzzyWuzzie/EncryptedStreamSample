using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ESSClient
{
    // adapted from http://gamedevelopment.tutsplus.com/tutorials/quick-tip-how-to-code-a-simple-character-name-generator--gamedev-14308
    class NameGenerator
    {
        private static Random rand = new Random();

        public static string Generate()
        {
            string[] firstNameSyllables = new string[] { "ken", "ton", "den", "nie", "mar", "tin", "pam", "ela", "dam", "a", "tor", "maur", "een", "lis", "sa", "er", "rin", "matt", "gar" };

            string name = "";
            int syllables = rand.Next(1, 4);
            for(int i = 0; i < syllables; i++)
            {

                name += firstNameSyllables[rand.Next(firstNameSyllables.Length)];
            }

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        }
    }
}

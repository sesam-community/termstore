using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Models
{
    public class TermLabel
    {
        public bool IsDefaultForLanguage { get; set; }
        public int Language { get; set; }
        public string Value { get; set; }
    }
}

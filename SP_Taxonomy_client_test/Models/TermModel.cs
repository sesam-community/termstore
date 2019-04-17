using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Models
{
    public class TermModel
    {
        public string termGroupName { get; set; }
        public Guid termGroupId { get; set;}
        public string termSetName { get; set; }
        public Guid termSetId { get; set;}
        public string termName { get; set; }
        public Guid termId { get; set; }
        //language code
        public int termLcid { get; set; }
    }
}

using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Models
{
    public class TermModel
    {
        public string termGroupName { get; set; }
        public string termGroupId { get; set; }
        public string termSetName { get; set; }
        public string termSetId { get; set; }
        public string termName { get; set; }
        public string termId { get; set; }
        //language code
        public List<childModel> termChildTerms {get; set;}
        public int termLcid { get; set; }
        public string termDescription { get; set; }
        public bool termIsAvailableForTagging { get; set; }
        public IDictionary<string, string> termLocalCustomProperties { get; set; }
        public IDictionary<string, string> termCustomProperties { get; set; }
        public bool termIsDeprecated { get; set; }
        public List<TermLabel> termLabels { get; set; }

    }

    public class childModel {
        public string childName { get; set; }
        public string childId { get; set; }
        public string childDescription { get; set;}
        public int childLcid { get; set;}
        public IDictionary<string, string> childLocalCustomProperties { get; set;}
        public IDictionary<string, string> childCustomProperties { get; set;}
        public List<ChildLabel> childLabels { get; set; }
    }
}

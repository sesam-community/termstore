using Microsoft.SharePoint.Client.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Models
{
    // Below class used for both getting and posting [GET] [POST] //
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

    // Below class used for getting [GET] //
    public class childModel {
        public string childName { get; set; }
        public string childId { get; set; }
        public string childDescription { get; set;}
        public int childLcid { get; set;}
        public List<childInChildModel> childChildTerms {get; set;}
        public IDictionary<string, string> childLocalCustomProperties { get; set;}
        public IDictionary<string, string> childCustomProperties { get; set;}
        public List<ChildLabel> childLabels { get; set; }
    }

    // Below class used for getting [GET] //
    public class childInChildModel {
        public string childChildName { get; set; }
        public string childChildId { get; set; }
        public string childChildDescription { get; set;}
        public int childChildLcid { get; set;}
        public List<childInChildrenModel> childInChildrenTerms {get; set;}
        public IDictionary<string, string> childChildLocalCustomProperties { get; set;}
        public IDictionary<string, string> childChildCustomProperties { get; set;}
        public List<ChildLabel> childChildLabels { get; set; }
    }

    // Below class used for getting [GET] //
    public class childInChildrenModel {
        public string childrenChildName { get; set; }
        public string childrenChildId { get; set; }
        public string childrenChildDescription { get; set;}
        public int childrenChildLcid { get; set;}
        public List<grandchildInChildModel> childrenGrandchildTerms {get; set;}
        public IDictionary<string, string> childrenChildLocalCustomProperties { get; set;}
        public IDictionary<string, string> childrenChildCustomProperties { get; set;}
        public List<ChildLabel> childrenChildLabels { get; set; }
    }
    
    // Below class used for getting [GET] //
    
    public class grandchildInChildModel {
        public string GrandchildName { get; set; }      
        public string GrandchildId { get; set; }        
        public string GrandchildDescription { get; set;}
        public int GrandchildLcid { get; set;}
        public IDictionary<string, string> GrandchildLocalCustomProperties { get; set;}      
        public IDictionary<string, string> GrandchildCustomProperties { get; set;}      
        public List<ChildLabel> GrandchildLabels { get; set; }
    }


    // Below class used for posting [POST] //
    public class childFromParentModel {
        public string cpGroupName { get; set; }
        public string cpGroupId { get; set; }
        public string cpSetName { get; set; }
        public string cpSetId { get; set; }
        public string cpTermName { get; set; }
        public string cpTermId { get; set; }
        public string cpChildName { get; set; }
        public string cpChildId { get; set; }
        public string cpChildDescription { get; set;}
        public int cpChildLcid { get; set;}
        public IDictionary<string, string> cpChildLocalCustomProperties { get; set;}
        public IDictionary<string, string> cpChildCustomProperties { get; set;}
        public List<ChildLabel> cpChildLabels { get; set; }
    }

    // Below class used for posting [POST] //
    public class childFromChildModel {
        public string cpGroupName { get; set; }
        public string cpGroupId { get; set; }
        public string cpSetName { get; set; }
        public string cpSetId { get; set; }
        public string cpTermName { get; set; }
        public string cpTermId { get; set; }
        public string cpChildName { get; set; }
        public string cpChildId { get; set; }
        public string ccpChildName { get; set; }
        public string ccpChildId { get; set; }
        public string ccpChildDescription { get; set;}
        public int ccpChildLcid { get; set;}
        public IDictionary<string, string> ccpChildLocalCustomProperties { get; set;}
        public IDictionary<string, string> ccpChildCustomProperties { get; set;}
        public List<ChildLabel> ccpChildLabels { get; set; }
    }

    public class childFromChildrenModel {
        public string GroupName { get; set; }
        public string GroupId { get; set; }
        public string SetName { get; set; }
        public string SetId { get; set; }
        public string TermName { get; set; }
        public string TermId { get; set; }
        public string ChildName { get; set; }
        public string ChildId { get; set; }
        public string cpChildName { get; set; }
        public string cpChildId { get; set; }
        public string ccpChildName { get; set; }
        public string ccpChildId { get; set; }
        public string ccpChildDescription { get; set;}
        public int ccpChildLcid { get; set;}
        public IDictionary<string, string> ccpChildLocalCustomProperties { get; set;}
        public IDictionary<string, string> ccpChildCustomProperties { get; set;}
        public List<ChildLabel> ccpChildLabels { get; set; }
    }

     public class grandchildFromChildrenModel {
        public string GroupName { get; set; }
        public string GroupId { get; set; }
        public string SetName { get; set; }
        public string SetId { get; set; }
        public string TermName { get; set; }
        public string TermId { get; set; }
        public string ParentchildName { get; set; }
        public string ParentchildId { get; set; }
        public string ChildName { get; set; }
        public string ChildId { get; set; }
        public string cpChildName { get; set; }
        public string cpChildId { get; set; }
        public string ccpChildName { get; set; }
        public string ccpChildId { get; set; }
        public string ccpChildDescription { get; set;}
        public int ccpChildLcid { get; set;}
        public IDictionary<string, string> ccpChildLocalCustomProperties { get; set;}
        public IDictionary<string, string> ccpChildCustomProperties { get; set;}
        public List<ChildLabel> ccpChildLabels { get; set; }
    }
}



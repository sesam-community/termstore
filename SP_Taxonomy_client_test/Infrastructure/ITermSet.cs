using Microsoft.AspNetCore.Mvc;
using Microsoft.SharePoint.Client.Taxonomy;
using SP_Taxonomy_client_test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Infrastructure
{
    public interface ITermSet
    {
        public Task<ActionResult<IEnumerable<TermModel>>> GetAllTerms(string _termset);
        public Task<IActionResult> AllTerms(string _termstore, string _termgroup, string _termset);
        public Task<ActionResult<IEnumerable<TermModel>>> CreateFromList(TermModel[] termList);
        public Task<ActionResult<IEnumerable<childFromParentModel>>> CreateFromParentList(childFromParentModel[] termList);

        public Task<ActionResult<IEnumerable<childFromChildModel>>> CreateFromChildList(childFromChildModel[] termList);

        public Task<ActionResult<IEnumerable<childFromChildrenModel>>> CreateFromChildChildList(childFromChildrenModel[] termList);

        public Task<ActionResult<IEnumerable<grandchildFromChildrenModel>>> CreateFromGrandchildList(grandchildFromChildrenModel[] termList);

        public Task<ActionResult<IEnumerable<grandchildFromChildChildModel>>> CreateFromGrandchildChildList(grandchildFromChildChildModel[] termList);

        public List<TermStoreModel> GetTermStores();
        public List<TermGroupModel> GetTermStoreGroups(string id);
    }
}

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
        public Task<ActionResult<IEnumerable<TermModel>>> GetAllTerms();
        public Task<ActionResult<IEnumerable<TermModel>>> CreateFromList(TermModel[] termList);

        public List<TermStoreModel> GetTermStores();
        public List<TermGroupModel> GetTermStoreGroups(string id);
    }
}

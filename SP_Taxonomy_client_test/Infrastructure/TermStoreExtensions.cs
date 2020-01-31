using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;

namespace SharePoint.Client.Taxonomy.Extensions
{
    public static class TermStoreExtensions
    {
        public static IEnumerable<Term> GetAllTerms(this TermStore termStore)
        {
            var ctx = termStore.Context;
            ctx.Load(termStore,
                       store => store.Groups.Include(
                           group => group.TermSets
                       )
               );
            ctx.ExecuteQuery();
            var result = new Dictionary<TermSet, TermCollection>();
            foreach (var termGroup in termStore.Groups)
            {
                foreach (var termSet in termGroup.TermSets)
                {
                    var allTermsInTermSet = termSet.GetAllTerms();
                    ctx.Load(allTermsInTermSet);
                    result[termSet] = allTermsInTermSet;
                }
            }
            var allTerms = result.SelectMany(x => x.Value);
            return allTerms;
        }

    }
}
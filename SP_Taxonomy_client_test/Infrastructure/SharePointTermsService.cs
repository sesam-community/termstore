using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using SP_Taxonomy_client_test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Infrastructure
{
    public class SharePointTermsService : ITermSet
    {

        private readonly IConfiguration config;
        private string url;
        private string username;
        private string password;
        private ClientContext cc;

        public SharePointTermsService(IConfiguration config)
        {
            this.config = config;

            this.url = this.config["url"];
            this.username = this.config["username"];
            this.password = this.config["password"];
            try
            {
                this.cc = AuthHelper.GetClientContextForUsernameAndPassword(this.config["url"], this.config["username"], this.config["password"]);
            }
            catch (NullReferenceException e) {
                System.Diagnostics.Debug.WriteLine("Exception occuresd whilt obtain client context due to: "+e.Message);
                throw new ArgumentNullException(e.Message);
            }

            
        }

        /// <summary>
        /// Fetch all terms from Sharepoint terms store
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<TermModel>>> GetAllTerms()
        {
            List<TermModel> resultList = new List<TermModel>(32);
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            TermStore termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            Web web = cc.Web;
            cc.Load(web);
            await cc.ExecuteQueryAsync();

            this.cc.Load(termStore,
                    store => store.Name,
                    store => store.Groups.Include(
                        group => group.Name,
                        group => group.LastModifiedDate,
                        group => group.CreatedDate,
                        group => group.Description,
                        group => group.Id,
                        group => group.TermSets.Include(
                            termSet => termSet.Name,
                            termSet => termSet.Contact,
                            termSet => termSet.CreatedDate,
                            termSet => termSet.Description,
                            termSet => termSet.Id,
                            termSet => termSet.IsOpenForTermCreation
                        )
                    )
            );
            await this.cc.ExecuteQueryAsync();

            if (taxonomySession == null || termStore == null) {
                return resultList;
            }

            foreach (TermGroup group in termStore.Groups)
            {
                foreach (TermSet termSet in group.TermSets)
                {
                    var terms = termSet.GetAllTerms();
                    this.cc.Load(terms);
                    await this.cc.ExecuteQueryAsync();

                    foreach (Term term in terms) {
                        var _term = new TermModel();

                        _term.termGroupName = group.Name;
                        _term.termSetName = termSet.Name;
                        _term.termName = term.Name;
                        _term.termGroupId = group.Id;
                        _term.termSetId = termSet.Id;
                        _term.termId = term.Id;

                        resultList.Add(_term);
                    }
                }
            }

            return resultList;
        }
        /// <summary>
        /// Create one or more terms 
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<TermModel>>> CreateFromList(TermModel[] termList)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession);
            await cc.ExecuteQueryAsync();

            TermStore termStore = taxonomySession.GetDefaultSiteCollectionTermStore();

            foreach (var term in termList) {
                var termSet = termStore.GetTermSet(term.termSetId);

                cc.Load(termSet);
                await cc.ExecuteQueryAsync();

                var newTerm = termSet.CreateTerm(term.termName, 1033, Guid.NewGuid());

                cc.Load(newTerm);
                await cc.ExecuteQueryAsync();
                term.termId = newTerm.Id;
            }
            return termList;
        }
    }
}

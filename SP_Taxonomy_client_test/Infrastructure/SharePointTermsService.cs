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
using System.Text;
using System.Threading.Tasks;

namespace SP_Taxonomy_client_test.Infrastructure
{
    public class SharePointTermsService : ITermSet
    {

        private readonly IConfiguration config;
        private readonly string url;
        private readonly string username;
        private readonly string password;
        private readonly ClientContext cc;

        public SharePointTermsService(IConfiguration config)
        {
            this.config = config;

            this.url = this.config["url"];
            this.username = this.config["username"];
            this.password = this.config["password"];
            try
            {
                this.cc = AuthHelper.GetClientContextForUsernameAndPassword(url, username, password);
            }
            catch (NullReferenceException e)
            {
                System.Diagnostics.Debug.WriteLine("Exception occuresd whilt obtain client context due to: " + e.Message);
                throw new ArgumentNullException(e.Message);
            }


        }

        public List<TermStoreModel> GetTermStores()
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(this.cc);
            List<TermStoreModel> resultList = new List<TermStoreModel>(1);

            this.cc.Load(taxonomySession.TermStores);
            this.cc.ExecuteQuery();

            foreach (var termStore in taxonomySession.TermStores)
            {
                TermStoreModel tempStore = new TermStoreModel
                {
                    DefaultLanguage = termStore.DefaultLanguage,
                    Id = termStore.Id.ToString(),
                    Name = termStore.Name,
                    IsOnline = termStore.IsOnline
                };
                resultList.Add(tempStore);
            }

            return resultList;
        }

        public List<TermGroupModel> GetTermStoreGroups(string id)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(this.cc);
            List<TermGroupModel> resultList = new List<TermGroupModel>(32);

            var termStore = taxonomySession.TermStores.GetById(new Guid(id));
            this.cc.Load(termStore.Groups);
            this.cc.ExecuteQuery();

            foreach (var group in termStore.Groups)
            {
                Console.WriteLine(group);
            }

            return resultList;
        }

        /// <summary>
        /// Fetch all terms from Sharepoint terms store
        /// Terms include some info about their TermSet and TermGroup as well as their own info
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
                            set => set.Name,
                            set => set.Description,
                            set => set.Id,
                            set => set.Contact,
                            set => set.CustomProperties,
                            set => set.IsAvailableForTagging,
                            set => set.IsOpenForTermCreation,
                            set => set.CustomProperties,
                            set => set.Terms.Include(
                                term => term.Name,
                                term => term.Description,
                                term => term.Id,
                                term => term.IsAvailableForTagging,
                                term => term.LocalCustomProperties,
                                term => term.CustomProperties,
                                term => term.IsDeprecated,
                                term => term.Labels.Include(
                                    label => label.Value,
                                    label => label.Language,
                                    label => label.IsDefaultForLanguage))
                        )
                    )
            );
            await this.cc.ExecuteQueryAsync();

            if (taxonomySession == null || termStore == null)
            {
                return resultList;
            }

            foreach (TermGroup group in termStore.Groups)
            {
                foreach (TermSet termSet in group.TermSets)
                {
                    var terms = termSet.Terms;
                    //this.cc.Load(terms);
                    //await this.cc.ExecuteQueryAsync();

                    foreach (Term term in terms)
                    {
                        var _term = new TermModel
                        {
                            termGroupName = group.Name,
                            termSetName = termSet.Name,
                            termName = term.Name,
                            termGroupId = group.Id.ToString(),
                            termSetId = termSet.Id.ToString(),
                            termId = term.Id.ToString(),
                            termDescription = term.Description,
                            termIsAvailableForTagging = term.IsAvailableForTagging,
                            termLocalCustomProperties = term.LocalCustomProperties,
                            termCustomProperties = term.CustomProperties,
                            termIsDeprecated = term.IsDeprecated,
                            termLabels = term.Labels.Select(
                                x => new TermLabel {
                                    IsDefaultForLanguage = x.IsDefaultForLanguage,
                                    Language = x.Language,
                                    Value = x.Value }
                                ).ToList()
                        };

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

            foreach (var term in termList)
            {
                var termSet = termStore.GetTermSet(new Guid(term.termSetId));

                cc.Load(termSet, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();

                byte[] bytes = Encoding.Default.GetBytes(term.termName);
                term.termName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02); ;
                

                if (termSet.Terms.Any(x => x.Name == term.termName))
                {
                    if (term.termId == null) {
                        continue;
                    }

                    var termToUpdate = termSet.Terms.GetById(new Guid(term.termId));
                    cc.Load(termToUpdate, t => t.Name, t => t.Labels.Include(lName => lName.Value));
                    await cc.ExecuteQueryAsync();

                    termToUpdate.Name = term.termName;
                    termToUpdate.SetDescription(term.termDescription, term.termLcid);

                    foreach (var customLocalProperty in term.termLocalCustomProperties) {
                        termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                    }

                    foreach (var customProperty in term.termCustomProperties) {
                        termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                    }

                    if (term.termLabels != null)
                    {
                        foreach (var label in term.termLabels)
                        {
                            if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                            {
                                termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                    }
                    cc.Load(termToUpdate);
                    await cc.ExecuteQueryAsync();
                }
                else {
                    var newTerm = termSet.CreateTerm(term.termName, 1033, Guid.NewGuid());

                    cc.Load(newTerm);
                    await cc.ExecuteQueryAsync();
                    term.termId = newTerm.Id.ToString();
                }

                
 
            }
            return termList;
        }
    }
}

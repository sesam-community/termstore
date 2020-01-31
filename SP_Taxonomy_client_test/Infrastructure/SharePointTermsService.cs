using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Client.Taxonomy;
using SP_Taxonomy_client_test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SP_Taxonomy_client_test.Infrastructure
{
    public class SharePointTermsService : ITermSet
    {

        private readonly IConfiguration config;
        private readonly string url;
        private readonly string username;
        private readonly string password;
        private readonly ClientContext cc;

        public SharePointTermsService(IConfiguration config)
        {
            
            this.config = config;

            if (this.config["url"] != null)
            {
                this.url = this.config["url"];
                this.username = this.config["username"];
                this.password = this.config["password"];
            }
            else
            {
               using (var file = System.IO.File.OpenText("helpers.json"))
                {
                    var reader = new JsonTextReader(file);
                    var jObject = JObject.Load(reader);
                    this.url = jObject.GetValue("url").ToString();
                    this.username = jObject.GetValue("username").ToString();
                    this.password = jObject.GetValue("password").ToString();
                }
            }
                  
            try
            {
                this.cc = AuthHelper.GetClientContextForUsernameAndPassword(url, username, password);
            }

            catch (NullReferenceException e)
            {
                System.Diagnostics.Debug.WriteLine("Exception occured whilst obtaining client context due to: " + e.Message);
                throw new ArgumentNullException(e.Message);
            }


        }

        public List<TermStoreModel> GetTermStores()
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(this.cc);
            List<TermStoreModel> resultList = new List<TermStoreModel>(1);

            this.cc.Load(taxonomySession.TermStores);
            this.cc.ExecuteQuery();

            foreach (var termStore in taxonomySession.TermStores)
            {
                TermStoreModel tempStore = new TermStoreModel
                {
                    DefaultLanguage = termStore.DefaultLanguage,
                    Id = termStore.Id.ToString(),
                    Name = termStore.Name,
                    IsOnline = termStore.IsOnline
                };
                resultList.Add(tempStore);
            }

            return resultList;
        }

        public List<TermGroupModel> GetTermStoreGroups(string id)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(this.cc);
            List<TermGroupModel> resultList = new List<TermGroupModel>(32);

            var termStore = taxonomySession.TermStores.GetById(new Guid(id));
            this.cc.Load(termStore.Groups);
            this.cc.ExecuteQuery();

            foreach (var group in termStore.Groups)
            {
                Console.WriteLine(group);
            }

            return resultList;
        }

        public async Task<IActionResult> AllTerms([FromQuery(Name = "termstore")] string _termstore, [FromQuery(Name = "termgroup")] string _termgroup, [FromQuery(Name = "termset")] string _termset)
        {
            try
            {
                List<JObject> listTerms = new List<JObject>();
                TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
                var termStores = taxonomySession.TermStores;
                TermStore termStore = termStores.GetById(new Guid(_termstore));
                var termGroups = termStore.Groups;
                var termGroup = termGroups.GetByName(_termgroup);
                var termSets = termGroup.TermSets;
                cc.Load(termGroup, 
                    group   => group.Id,
                    group   => group.Name,
                    group   => group.TermSets.Include(
                        termset => termset.Id,
                        termset => termset.Name,
                        termset => termset.Terms.Include(
                            term => term.Name,
                            term => term.Description,
                            term => term.Id,
                            term => term.Labels,
                            term => term.Terms.Include(
                                childTerm => childTerm.Id,
                                childTerm => childTerm.Name,
                                childTerm => childTerm.Description,
                                childTerm => childTerm.Labels
                            )
                    )
                    ));
                await cc.ExecuteQueryAsync();
                for (int i = 0; i < termSets.Count; i++)
                {
                    if (termSets[i].Name.Equals(_termset))
                    {
                        TermSet termSet = termSets[i];
                        var terms = termSet.Terms;
                        foreach (var term in terms)
                        {
                            var json = new JObject();
                            json.Add(new JProperty("termSetId", termSet.Id));
                            json.Add(new JProperty("termSetName", termSet.Name));
                            json.Add(new JProperty("termGroup", termGroup.Name));
                            json.Add(new JProperty("termGroupId", termGroup.Id));
                            json.Add(new JProperty("termId", term.Id));
                            json.Add(new JProperty("termName", term.Name));
                            json.Add(new JProperty("termDescription", term.Description));
                            var labels = term.Labels;
                            var jsonLabels = new JArray();
                            foreach (var label in labels)
                            {
                                var jsonChild = new JObject();
                                jsonChild.Add(new JProperty("isDefaultForLanguage", label.IsDefaultForLanguage));
                                jsonChild.Add(new JProperty("language", label.Language));
                                jsonChild.Add(new JProperty("value", label.Value));
                                jsonLabels.Add(new JObject(jsonChild));
                            }
                            json.Add(new JProperty("termLabels", jsonLabels));   
                            var jsonChildTerms = new JArray();
                            var childTerms = term.Terms;
                            foreach (var childTerm in childTerms)
                            {
                                var jsonChildTerm = new JObject();
                                jsonChildTerm.Add(new JProperty("childTermId", childTerm.Id));
                                jsonChildTerm.Add(new JProperty("childTermName", childTerm.Name));
                                jsonChildTerm.Add(new JProperty("childTermDescription", childTerm.Description));
                                var childTermLabels = childTerm.Labels;
                                var childTermLabelArray = new JArray();
                                foreach (var childTermLabel in childTermLabels)
                                {
                                    var jsonChild = new JObject();
                                    jsonChild.Add(new JProperty("isDefaultForLanguage", childTermLabel.IsDefaultForLanguage));
                                    jsonChild.Add(new JProperty("language", childTermLabel.Language));
                                    jsonChild.Add(new JProperty("value", childTermLabel.Value));
                                    childTermLabelArray.Add(new JObject(jsonChild));
                                }
                                jsonChildTerm.Add(new JProperty("childTermLabels", childTermLabelArray));
                                jsonChildTerms.Add(new JObject(jsonChildTerm));
                            }        
                            json.Add(new JProperty("childTerms", jsonChildTerms));             
                            listTerms.Add(json);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                return new OkObjectResult(listTerms);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
        

        /// <summary>
        /// Fetch all terms from Sharepoint terms store
        /// Terms include some info about their TermSet and TermGroup as well as their own info
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<TermModel>>> GetAllTerms([FromQuery(Name = "termset")] string _termset)
        {
            List<TermModel> resultList = new List<TermModel>(32);
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);       
            TermStore termStore = taxonomySession.GetDefaultSiteCollectionTermStore();
            await cc.ExecuteQueryAsync();
            try {
                this.cc.Load(termStore,
                            store => store.Name,
                            store => store.Groups.Include(
                                group => group.Name,
                                group => group.LastModifiedDate,
                                group => group.CreatedDate,
                                group => group.Description,
                                group => group.Id,
                                group => group.TermSets.Include(
                                    set => set.Name,
                                    set => set.Description,
                                    set => set.Id,
                                    set => set.Contact,
                                    set => set.CustomProperties,
                                    set => set.IsAvailableForTagging,
                                    set => set.IsOpenForTermCreation,
                                    set => set.CustomProperties,
                                    set => set.Terms.Include(
                                        term => term.Name,
                                        term => term.Description,
                                        term => term.Id,
                                        term => term.IsAvailableForTagging,
                                        term => term.LocalCustomProperties,
                                        term => term.CustomProperties,
                                        term => term.IsDeprecated,
                                        term => term.Labels,
                                    term  => term.Terms.Include(
                                        term => term.Name,
                                        term => term.Description,
                                        term => term.Id,
                                        term => term.LocalCustomProperties,
                                        term => term.CustomProperties,
                                        term=> term.Labels,
                                        term => term.Terms.Include(
                                            term => term.Name,
                                            term => term.Description,
                                            term=> term.Id,
                                            term=> term.LocalCustomProperties,
                                            term=> term.CustomProperties,
                                            term => term.Labels,
                                            term => term.Terms.Include(
                                                term => term.Name,
                                                term => term.Description,
                                                term=> term.Id,
                                                term=> term.LocalCustomProperties,
                                                term=> term.CustomProperties,
                                                term=> term.Labels,
                                                term => term.Terms.Include(
                                                    term => term.Name,
                                                    term => term.Description,
                                                    term=> term.Id,
                                                    term=> term.LocalCustomProperties,
                                                    term=> term.CustomProperties,
                                                    term=> term.Labels,
                                                    term => term.Terms.Include(
                                                        term => term.Name,
                                                        term => term.Description,
                                                        term=> term.Id,
                                                        term=> term.LocalCustomProperties,
                                                        term=> term.CustomProperties,
                                                        term=> term.Labels
                                                    )
                                                
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );
            await this.cc.ExecuteQueryAsync();
            }
            
            catch (Exception e) {
                Console.WriteLine("Failing in load with error :" + e);
            }
            
            //if (taxonomySession == null || termStore == null)
            //{
            //    return resultList;
            //}

            var termGroups = termStore.Groups;
            var termGroup = termGroups.GetByName(_termset);
            
            try
            {
                foreach (TermGroup group in termStore.Groups)
                {
                    if (group.Name == _termset)
                    {
                        foreach (TermSet termSet in group.TermSets)
                        {
                            var terms = termSet.Terms;

                            foreach (Term term in terms)
                            {
                                var _term = new TermModel
                                {
                                    termGroupName = group.Name,
                                    termSetName = termSet.Name,
                                    termName = term.Name,
                                    termGroupId = group.Id.ToString(),
                                    termSetId = termSet.Id.ToString(),
                                    termId = term.Id.ToString(),
                                    termDescription = term.Description,
                                    termIsAvailableForTagging = term.IsAvailableForTagging,
                                    termLocalCustomProperties = term.LocalCustomProperties,
                                    termCustomProperties = term.CustomProperties,
                                    termChildTerms = term.Terms.Select(dk => new childModel {
                                        childName = dk.Name,
                                        childDescription = dk.Description,
                                        childLocalCustomProperties = dk.LocalCustomProperties,
                                        childCustomProperties = dk.CustomProperties,
                                        childId = dk.Id.ToString(),
                                        childLabels = dk.Labels.Select(
                                            no => new ChildLabel {
                                                IsDefaultForLanguage = no.IsDefaultForLanguage,
                                                Language = no.Language,
                                                Value = no.Value 
                                            }
                                        ).ToList(),
                                        childChildTerms = dk.Terms.Select(se => new childInChildModel {
                                            childChildName = se.Name,
                                            childChildDescription = se.Description,
                                            childChildLocalCustomProperties = se.LocalCustomProperties,
                                            childChildCustomProperties = se.CustomProperties,
                                            childChildId = se.Id.ToString(),
                                            childChildLabels = se.Labels.Select(
                                                i => new ChildLabel {
                                                    IsDefaultForLanguage = i.IsDefaultForLanguage,
                                                    Language = i.Language,
                                                    Value = i.Value 
                                                }
                                            ).ToList(),
                                            childInChildrenTerms = se.Terms.Select(no4life => new childInChildrenModel {
                                                childrenChildName = no4life.Name,
                                                childrenChildDescription = no4life.Description,
                                                childrenChildLocalCustomProperties = no4life.LocalCustomProperties,
                                                childrenChildCustomProperties = no4life.CustomProperties,
                                                childrenChildId = no4life.Id.ToString(),
                                                childrenChildLabels = no4life.Labels.Select(
                                                    i => new ChildLabel {
                                                        IsDefaultForLanguage = i.IsDefaultForLanguage,
                                                        Language = i.Language,
                                                        Value = i.Value 
                                                    }
                                                ).ToList(),
                                                childrenGrandchildTerms = no4life.Terms.Select(dk4life => new grandchildInChildModel {
                                                    GrandchildName = dk4life.Name,
                                                    GrandchildDescription = dk4life.Description,
                                                    GrandchildLocalCustomProperties = dk4life.LocalCustomProperties,
                                                    GrandchildCustomProperties = dk4life.CustomProperties,
                                                    GrandchildId = dk4life.Id.ToString(),
                                                    GrandchildLabels = dk4life.Labels.Select(
                                                        i => new ChildLabel {
                                                            IsDefaultForLanguage = i.IsDefaultForLanguage,
                                                            Language = i.Language,
                                                            Value = i.Value 
                                                        }
                                                    ).ToList(),
                                                    GrandChildChildTerms = dk4life.Terms.Select(se4life => new grandchildInGrandchildModel {
                                                        GrandchildChildName = se4life.Name,
                                                        GrandchildChildDescription = se4life.Description,
                                                        GrandchildChildLocalCustomProperties = se4life.LocalCustomProperties,
                                                        GrandchildChildCustomProperties = se4life.CustomProperties,
                                                        GrandchildChildId = se4life.Id.ToString(),
                                                        GrandchildChildLabels = se4life.Labels.Select(
                                                            i => new ChildLabel {
                                                                IsDefaultForLanguage = i.IsDefaultForLanguage,
                                                                Language = i.Language,
                                                                Value = i.Value 
                                                            }
                                                        ).ToList()
                                                    }).ToList(),
                                                }).ToList(),
                                            }).ToList(),           
                                        }).ToList(),    
                                    }).ToList(),
                                    termIsDeprecated = term.IsDeprecated,
                                    termLabels = term.Labels.Select(
                                        x => new TermLabel {
                                            IsDefaultForLanguage = x.IsDefaultForLanguage,
                                            Language = x.Language,
                                            Value = x.Value }
                                        ).ToList()
                                };

                                resultList.Add(_term);
                            }
                        }
                    }    

                }
   
            }
            catch (Exception e)
            {
                Console.WriteLine("Failing in model mapping with error :" + e);
            }

            return resultList;
        }

        public async Task<ActionResult<IEnumerable<childFromParentModel>>> PostChildTerms()
        {
            List<childFromParentModel> resultList = new List<childFromParentModel>(32);
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            TermStore termStore = taxonomySession.TermStores[0];        
            await cc.ExecuteQueryAsync();

            var termGroup = termStore.Groups[0];
            var termSet = termGroup.TermSets[0];
            var parentTerm = termSet.Terms[0];
            var terms = parentTerm.Terms; 
                        
            foreach (Term term in terms)
            {
                var _term = new childFromParentModel()
                {
                    cpGroupName=termGroup.Name,
                            cpSetName = termSet.Name,
                            cpTermName = term.Name,
                            cpGroupId =termGroup.Id.ToString(),
                            cpSetId = termSet.Id.ToString(),
                            cpTermId = term.Id.ToString(),
                            cpChildName = term.Name,
                    cpChildId = term.Id.ToString(),
                    cpChildDescription = term.Description,
                    cpChildLocalCustomProperties = term.LocalCustomProperties,
                    cpChildCustomProperties = term.CustomProperties,
                    cpChildLabels = term.Labels.Select(
                            no => new ChildLabel {
                                IsDefaultForLanguage = no.IsDefaultForLanguage,
                                Language = no.Language,
                                Value = no.Value 
                            }
                        ).ToList()
                        };

                        resultList.Add(_term);
                        
            }
            return resultList;
        }

        public async Task<ActionResult<IEnumerable<childFromChildModel>>> PostChildInChildTerms()
        {
            List<childFromChildModel> resultList = new List<childFromChildModel>(32);
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            TermStore termStore = taxonomySession.TermStores[0]; 
            await cc.ExecuteQueryAsync();
            
            var termGroup = termStore.Groups[0];
            var termSet = termGroup.TermSets[0];
            var parentTerm = termSet.Terms[0];
            var terms = parentTerm.Terms; 
                        
            foreach (Term term in terms)
            {
                var _term = new childFromChildModel()
                {
                    cpGroupName= termGroup.Name,
                            cpSetName = termSet.Name,
                            cpTermName = term.Name,
                            cpGroupId = termGroup.Id.ToString(),
                            cpSetId = termSet.Id.ToString(),
                            cpTermId = term.Id.ToString(),
                            cpChildName = term.Name,
                    cpChildId = term.Id.ToString(),
                    ccpChildName = term.Name,
                    ccpChildId = term.Id.ToString(),
                    ccpChildDescription = term.Description,
                    ccpChildLocalCustomProperties = term.LocalCustomProperties,
                    ccpChildCustomProperties = term.CustomProperties,
                    ccpChildLabels = term.Labels.Select(
                            no => new ChildLabel {
                                IsDefaultForLanguage = no.IsDefaultForLanguage,
                                Language = no.Language,
                                Value = no.Value 
                            }
                        ).ToList()
                        };

                        resultList.Add(_term);

            }
            return resultList;
        }

        public async Task<ActionResult<IEnumerable<childFromChildrenModel>>> PostChildChildChildrenTerms()
        {
            List<childFromChildrenModel> resultList = new List<childFromChildrenModel>(32);
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            TermStore termStore = taxonomySession.TermStores[0];      
            await cc.ExecuteQueryAsync();

            var termGroup = termStore.Groups[0];
            var termSet = termGroup.TermSets[0];
            var parentTerm = termSet.Terms[0];
            var childTerm = parentTerm.Terms[0];
            var terms = childTerm.Terms;

            foreach (Term term in terms)
            {
                var _term = new childFromChildrenModel()
                {
                
                GroupName= termGroup.Name,
                            SetName = termSet.Name,
                            TermName = parentTerm.Name,
                            GroupId = termGroup.Id.ToString(),
                            SetId = termSet.Id.ToString(),
                            TermId = parentTerm.Id.ToString(),
                ChildName= childTerm.Name,
                            ChildId = childTerm.Id.ToString(),
                            cpChildName = term.Name,
                cpChildId = term.Id.ToString(),
                ccpChildName = term.Name,
                ccpChildId = term.Id.ToString(),
                ccpChildDescription = term.Description,
                ccpChildLocalCustomProperties = term.LocalCustomProperties,
                ccpChildCustomProperties = term.CustomProperties,
                ccpChildLabels = term.Labels.Select(
                        no => new ChildLabel {
                            IsDefaultForLanguage = no.IsDefaultForLanguage,
                            Language = no.Language,
                            Value = no.Value 
                        }
                    ).ToList()
                        };
                        resultList.Add(_term);    
            }
            return resultList;
        }


        /// <summary>
        /// Create one or more terms 
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<TermModel>>> CreateFromList(TermModel[]? termList)
        {
            
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            await cc.ExecuteQueryAsync();

            TermStore termStore = taxonomySession.TermStores[0];
            cc.Load(termStore);

            foreach (var term in termList)
            {
                TermSet termSet = termStore.GetTermSet(new Guid(term.termSetId));
                
                cc.Load(termSet, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();

                byte[] bytes = Encoding.Default.GetBytes(term.termName);
                term.termName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02);
                
                if (termSet.Terms.Any(x => x.Name == term.termName))
                {
                    if (term.termId == null) {
                        continue;
                    }

                    try
                    {
                        var termToUpdate = termSet.Terms.GetById(new Guid(term.termId));
                        cc.Load(termToUpdate, t => t.Name, t => t.Labels.Include(lName => lName.Value));
                        await cc.ExecuteQueryAsync();

                        if (term.termDescription != null)
                        {
                            termToUpdate.SetDescription(term.termDescription, term.termLcid);
                        }

                        if (term.termLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.termLocalCustomProperties) 
                            {
                                termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }
                        if (term.termCustomProperties != null) 
                        {
                            foreach (var customProperty in term.termCustomProperties) 
                            {
                                termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }
                        
                        if (term.termLabels != null)
                        {
                            foreach (var label in term.termLabels)
                            {
                                if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                                {
                                    termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        termToUpdate.Name = label.Value;
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Writing name of parent term : " + term.termName);
                        Console.WriteLine("---------------------------------------------------------");
                        var count = 1;
                        foreach(var child in term.termChildTerms)
                        {
                            var childToUpdate = termToUpdate.CreateTerm(child.childName, child.childLcid, Guid.NewGuid());
                            
                            cc.Load(childToUpdate, t => t.Name, t => t.Labels.Include(lName => lName.Value));
                            await cc.ExecuteQueryAsync();

                            if (child.childLocalCustomProperties != null) 
                            {
                                foreach (var customLocalProperty in child.childLocalCustomProperties) 
                                {
                                    childToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                                }
                            }
                            
                            if (child.childCustomProperties != null) 
                            {
                                foreach (var customProperty in child.childCustomProperties) 
                                {
                                    childToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                                }
                            }

                            if (child.childLabels != null)
                            {
                                foreach (var label in child.childLabels)
                                {
                                    if (!childToUpdate.Labels.Any(no => no.Value == label.Value))
                                    {
                                        childToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                        if (label.IsDefaultForLanguage == true)
                                        {
                                            childToUpdate.Name = label.Value;
                                        }
                                    }
                                }
                            }

                            foreach(var grandchild in child.childChildTerms)
                            {
                                var grandChildToUpdate = childToUpdate.CreateTerm(grandchild.childChildName, grandchild.childChildLcid, Guid.NewGuid());

                                if (grandchild.childChildLocalCustomProperties != null) 
                                {
                                    foreach (var customLocalProperty in  grandchild.childChildLocalCustomProperties) 
                                    {
                                        grandChildToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                                    }
                                }

                                if (grandchild.childChildCustomProperties != null) 
                                {
                                    foreach (var customProperty in grandchild.childChildCustomProperties) 
                                    {
                                        grandChildToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                                    }
                                }

                                if (grandchild.childChildLabels != null)
                                {
                                    foreach (var label in grandchild.childChildLabels) 
                                    {
                                        grandChildToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    }
                                }
                          
                                Console.WriteLine("Writing name of grandchild term : " + grandchild.childChildName);
                            }

                            Console.WriteLine("Writing iteration count to check where number of children crash termstore:" + count);
                            Console.WriteLine("Writing name of child term : " + child.childName);
                            count++;
                        }

                        termStore.CommitAll();
                        cc.ExecuteQuery();
                    
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }
                else {
                    try
                    {
                        var newTerm = termSet.CreateTerm(term.termName,term.termLcid, Guid.NewGuid());
                        
                        if (term.termDescription != null)
                        {
                            newTerm.SetDescription(term.termDescription, term.termLcid);
                        }
                        
                        if (term.termLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.termLocalCustomProperties) 
                            {
                                newTerm.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }

                        if (term.termCustomProperties != null) 
                        {
                            foreach (var customProperty in term.termCustomProperties) 
                            {
                                newTerm.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }

                        if (term.termLabels != null)
                        {
                            foreach (var label in term.termLabels) 
                            {
                                newTerm.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                        

                        Console.WriteLine("---------------------------------------------------------");
                        Console.WriteLine("Writing name of parent term : " + term.termName);
                        Console.WriteLine("---------------------------------------------------------");

                        termStore.CommitAll();
                        cc.ExecuteQuery();
                        term.termId = newTerm.Id.ToString();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);;
                    }
                }             
            }
            return termList;
        }
        
        /// <summary>
        /// Create one or more terms 
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<childFromParentModel>>>CreateFromParentList(childFromParentModel[]? termList)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            await cc.ExecuteQueryAsync();
            
            TermStore termStore = taxonomySession.TermStores[0];
            cc.Load(termStore);

            foreach (var term in termList)
            {
                
                Term parentTerm = termStore.GetTerm(new Guid(term.cpTermId));
                cc.Load(parentTerm, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();

                byte[] bytes = Encoding.Default.GetBytes(term.cpChildName);
                term.cpChildName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02);
                
                if (parentTerm.Terms.Any(x => x.Name == term.cpChildName))
                {
                    if (term.cpChildId == null) {
                        continue;
                    }

                    try
                    {
                        var termToUpdate = parentTerm.Terms.GetById(new Guid(term.cpChildId));
                   
                        if (term.cpChildDescription != null)
                        {
                            termToUpdate.SetDescription(term.cpChildDescription, term.cpChildLcid);
                        }

                        if (term.cpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.cpChildLocalCustomProperties) 
                            {
                                termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }
                        if (term.cpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.cpChildCustomProperties) 
                            {
                                termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }
                        
                        if (term.cpChildLabels != null)
                        {
                            foreach (var label in term.cpChildLabels)
                            {
                                if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                                {
                                    termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        termToUpdate.Name = label.Value;
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.cpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                    
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }
                else {
                    try
                    {
                        var newTerm = parentTerm.CreateTerm(term.cpChildName,term.cpChildLcid, Guid.NewGuid());                        
                        if (term.cpChildDescription != null)
                        {
                            newTerm.SetDescription(term.cpChildDescription, term.cpChildLcid);
                        }
                        
                        if (term.cpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.cpChildLocalCustomProperties) 
                            {
                                newTerm.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }

                        if (term.cpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.cpChildCustomProperties) 
                            {
                                newTerm.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }

                        if (term.cpChildLabels!= null)
                        {
                            foreach (var label in term.cpChildLabels) 
                            {
                                newTerm.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.cpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                        term.cpChildId = newTerm.Id.ToString();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }             
            }
            return termList;
        }

        /// <summary>
        /// Create one or more terms 
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<childFromChildModel>>>CreateFromChildList(childFromChildModel[]? termList)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            await cc.ExecuteQueryAsync();
            
            TermStore termStore = taxonomySession.TermStores[0];
            cc.Load(termStore);

            foreach (var term in termList)
            {

                Term childTerm = termStore.GetTerm(new Guid(term.cpChildId));
                cc.Load(childTerm, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();
                
                byte[] bytes = Encoding.Default.GetBytes(term.ccpChildName);
                term.ccpChildName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02);
                
                if (childTerm.Terms.Any(x => x.Name == term.ccpChildName))
                {
                    if (term.ccpChildId == null) {
                        continue;
                    }

                    try
                    {
                        var termToUpdate = childTerm.Terms.GetById(new Guid(term.ccpChildId));                           
                        if (term.ccpChildDescription != null)
                        {
                            termToUpdate.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }

                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }
                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }
                        
                        if (term.ccpChildLabels != null)
                        {
                            foreach (var label in term.ccpChildLabels)
                            {
                                if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                                {
                                    termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        termToUpdate.Name = label.Value;
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                    
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }
                else {
                    try
                    {
                        var newTerm = childTerm.CreateTerm(term.ccpChildName,term.ccpChildLcid, Guid.NewGuid());                        
                        if (term.ccpChildDescription != null)
                        {
                            newTerm.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }
                        
                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                newTerm.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }

                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                newTerm.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }

                        if (term.ccpChildLabels!= null)
                        {
                            foreach (var label in term.ccpChildLabels) 
                            {
                                newTerm.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                        term.ccpChildId = newTerm.Id.ToString();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }             
            }
            return termList;
        }

        /// <summary>
        /// Create one or more terms 
        /// </summary>
        /// <param name="termList"></param>
        /// <returns></returns>
        public async Task<ActionResult<IEnumerable<childFromChildrenModel>>>CreateFromChildChildList(childFromChildrenModel[]? termList)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            await cc.ExecuteQueryAsync();
            
            TermStore termStore = taxonomySession.TermStores[0];
            cc.Load(termStore);

            foreach (var term in termList)
            {
                Term childTerm = termStore.GetTerm(new Guid(term.cpChildId));
                cc.Load(childTerm, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();
                
                byte[] bytes = Encoding.Default.GetBytes(term.ccpChildName);
                term.ccpChildName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02);
                
                if (childTerm.Terms.Any(x => x.Name == term.ccpChildName))
                {
                    if (term.ccpChildId == null) {
                        continue;
                    }

                    try
                    {
                        var termToUpdate = childTerm.Terms.GetById(new Guid(term.ccpChildId));                           
                        if (term.ccpChildDescription != null)
                        {
                            termToUpdate.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }

                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }
                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }
                        
                        if (term.ccpChildLabels != null)
                        {
                            foreach (var label in term.ccpChildLabels)
                            {
                                if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                                {
                                    termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        termToUpdate.Name = label.Value;
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                    
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }
                else {
                    try
                    {
                        var newTerm = childTerm.CreateTerm(term.ccpChildName,term.ccpChildLcid, Guid.NewGuid());                        
                        if (term.ccpChildDescription != null)
                        {
                            newTerm.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }
                        
                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                newTerm.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }

                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                newTerm.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }

                        if (term.ccpChildLabels!= null)
                        {
                            foreach (var label in term.ccpChildLabels) 
                            {
                                newTerm.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                        term.ccpChildId = newTerm.Id.ToString();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }             
            }
            return termList;
        }
        
        public async Task<ActionResult<IEnumerable<grandchildFromChildrenModel>>>CreateFromGrandchildList(grandchildFromChildrenModel[]? termList)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            await cc.ExecuteQueryAsync();
            
            TermStore termStore = taxonomySession.TermStores[0];
            cc.Load(termStore);

            foreach (var term in termList)
            {
                Term childTerm = termStore.GetTerm(new Guid(term.cpChildId));
                cc.Load(childTerm, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();
                
                byte[] bytes = Encoding.Default.GetBytes(term.ccpChildName);
                term.ccpChildName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02);
                
                if (childTerm.Terms.Any(x => x.Name == term.ccpChildName))
                {
                    if (term.ccpChildId == null) {
                        continue;
                    }

                    try
                    {
                        var termToUpdate = childTerm.Terms.GetById(new Guid(term.ccpChildId));                           
                        if (term.ccpChildDescription != null)
                        {
                            termToUpdate.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }

                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }
                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }
                        
                        if (term.ccpChildLabels != null)
                        {
                            foreach (var label in term.ccpChildLabels)
                            {
                                if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                                {
                                    termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        termToUpdate.Name = label.Value;
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                    
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }
                else {
                    try
                    {
                        var newTerm = childTerm.CreateTerm(term.ccpChildName,term.ccpChildLcid, Guid.NewGuid());                        
                        if (term.ccpChildDescription != null)
                        {
                            newTerm.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }
                        
                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                newTerm.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }

                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                newTerm.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }

                        if (term.ccpChildLabels!= null)
                        {
                            foreach (var label in term.ccpChildLabels) 
                            {
                                newTerm.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                        term.ccpChildId = newTerm.Id.ToString();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }             
            }
            return termList;
        }

        public async Task<ActionResult<IEnumerable<grandchildFromChildChildModel>>>CreateFromGrandchildChildList(grandchildFromChildChildModel[]? termList)
        {
            TaxonomySession taxonomySession = TaxonomySession.GetTaxonomySession(cc);
            cc.Load(taxonomySession.TermStores);
            await cc.ExecuteQueryAsync();
            
            TermStore termStore = taxonomySession.TermStores[0];
            cc.Load(termStore);

            foreach (var term in termList)
            {
                Term childTerm = termStore.GetTerm(new Guid(term.cpChildId));
                cc.Load(childTerm, set => set.Name, set => set.Terms.Include(term => term.Name));
                await cc.ExecuteQueryAsync();
                
                byte[] bytes = Encoding.Default.GetBytes(term.ccpChildName);
                term.ccpChildName = Encoding.UTF8.GetString(bytes).Replace('&', (char)0xff06).Replace('"', (char)0xff02);
                
                if (childTerm.Terms.Any(x => x.Name == term.ccpChildName))
                {
                    if (term.ccpChildId == null) {
                        continue;
                    }

                    try
                    {
                        var termToUpdate = childTerm.Terms.GetById(new Guid(term.ccpChildId));                           
                        if (term.ccpChildDescription != null)
                        {
                            termToUpdate.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }

                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                termToUpdate.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }
                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                termToUpdate.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }
                        
                        if (term.ccpChildLabels != null)
                        {
                            foreach (var label in term.ccpChildLabels)
                            {
                                if (!termToUpdate.Labels.Any(x => x.Value == label.Value))
                                {
                                    termToUpdate.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                                    if (label.IsDefaultForLanguage == true)
                                    {
                                        termToUpdate.Name = label.Value;
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                    
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }
                else {
                    try
                    {
                        var newTerm = childTerm.CreateTerm(term.ccpChildName,term.ccpChildLcid, Guid.NewGuid());                        
                        if (term.ccpChildDescription != null)
                        {
                            newTerm.SetDescription(term.ccpChildDescription, term.ccpChildLcid);
                        }
                        
                        if (term.ccpChildLocalCustomProperties != null) 
                        {
                            foreach (var customLocalProperty in term.ccpChildLocalCustomProperties) 
                            {
                                newTerm.SetLocalCustomProperty(customLocalProperty.Key, customLocalProperty.Value);
                            }
                        }

                        if (term.ccpChildCustomProperties != null) 
                        {
                            foreach (var customProperty in term.ccpChildCustomProperties) 
                            {
                                newTerm.SetCustomProperty(customProperty.Key, customProperty.Value);
                            }
                        }

                        if (term.ccpChildLabels!= null)
                        {
                            foreach (var label in term.ccpChildLabels) 
                            {
                                newTerm.CreateLabel(label.Value, label.Language, label.IsDefaultForLanguage);
                            }
                        }
                        
                        Console.WriteLine("Writing name of child term : " + term.ccpChildName);
                        termStore.CommitAll();
                        cc.ExecuteQuery();
                        term.ccpChildId = newTerm.Id.ToString();
                    }
                    catch (Exception e) {
                        Console.WriteLine("Failing with error : " + e.Message);
                    }
                }             
            }
            return termList;
        }

    }
}


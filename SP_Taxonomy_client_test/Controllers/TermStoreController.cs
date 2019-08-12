using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client.Taxonomy;
using SP_Taxonomy_client_test.Infrastructure;
using SP_Taxonomy_client_test.Models;

namespace SP_Taxonomy_client_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermStoreController : ControllerBase
    {
        private readonly ITermSet _spTermsService;

        private readonly IConfiguration _config;

        public TermStoreController(ITermSet _spTermsService, IConfiguration _config)
        {
            this._spTermsService = _spTermsService;
            this._config = _config;
        }

        // GET api/termstore
        [HttpGet]
        [Produces("application/json")]
        public List<TermStoreModel> GetTermStores()
        {
            this._spTermsService.GetTermStoreGroups("ff2f403b-7ea3-4527-9fc1-1d8757026509");
            var termStores = this._spTermsService.GetTermStores();
            return termStores;
        }
    }
}
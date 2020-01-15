using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Taxonomy;
using SP_Taxonomy_client_test.Infrastructure;
using SP_Taxonomy_client_test.Models;

namespace SP_Taxonomy_client_test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermSetController : ControllerBase
    {
        private readonly ITermSet _spTermsService;

        private readonly IConfiguration _config;

        public TermSetController(ITermSet _spTermsService, IConfiguration _config)
        {
            this._spTermsService = _spTermsService;
            this._config = _config;
        }

        // GET api/termset
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<TermModel>>> GetTerms()
        {
            return await this._spTermsService.GetAllTerms();
        }


        [HttpPost("children")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<childFromParentModel>>> PostChildTerms([FromBody] childFromParentModel[] termList) 
        {
            return await this._spTermsService.CreateFromParentList(termList);
        }


        [HttpPost("child/children")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<childFromChildModel>>> PostChildChildTerms([FromBody] childFromChildModel[] termList) 
        {
            return await this._spTermsService.CreateFromChildList(termList);
        }

        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<TermModel>>> PostTerms([FromBody] TermModel[] termList) 
        {
            return await this._spTermsService.CreateFromList(termList);
        }
    }
}

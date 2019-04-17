using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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

        public TermSetController(ITermSet _spTermsService)
        {
            this._spTermsService = _spTermsService;
        }

        // GET api/termset
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<TermModel>>> GetTerms()
        {
            return await this._spTermsService.GetAllTerms();
        }


        [HttpPost]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<ActionResult<IEnumerable<TermModel>>> PostTerms([FromBody] TermModel[] termList) {
            return await this._spTermsService.CreateFromList(termList);
        }

    }
}

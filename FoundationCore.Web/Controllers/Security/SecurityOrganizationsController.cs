using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Foundation.Security.Controllers.WebAPI
{
    //
    // Changes the get list data function return captions from the linked tables ot improve clarity
    //
    public partial class SecurityOrganizationsController : SecureWebAPIController
    {

        [Route("api/SecurityOrganizations/ListData")]
        [HttpGet]
        public async Task<IActionResult> GetListData()
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Unauthorized();
            }

            List<BasicListItem> output = await (from o in _context.SecurityOrganizations
                                                join t in _context.SecurityTenants on o.securityTenantId equals t.id
                                                where
                                                o.active == true &&
                                                o.deleted == false &&
                                                t.active == true &&
                                                t.deleted == false
                                                orderby t.name, o.name
                                                select new BasicListItem()
                                                {
                                                    id = o.id,
                                                    name = t.name + " - " + o.name,
                                                })
                                          .ToListAsync();

            return Ok(output);
        }
    }
}

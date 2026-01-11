using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Foundation.Security.Controllers.WebAPI
{
    //
    // Changes the get list data function return captions from the linked tables to improve clarity
    //
    public partial class SecurityTeamsController : SecureWebAPIController
    {

        [Route("api/SecurityTeams/ListData")]
        [HttpGet]
        public async Task<IActionResult> GetListData()
        {
            if (await DoesUserHaveReadPrivilegeSecurityCheckAsync(READ_PERMISSION_LEVEL_REQUIRED) == false)
            {
                return Unauthorized();
            }

            List<BasicListItem> output = await (from tm in _context.SecurityTeams
                                                join d in _context.SecurityDepartments on tm.securityDepartmentId equals d.id
                                                join o in _context.SecurityOrganizations on d.securityOrganizationId equals o.id
                                                join t in _context.SecurityTenants on o.securityTenantId equals t.id
                                                where
                                                tm.active == true &&
                                                tm.deleted == false &&
                                                d.active == true &&
                                                d.deleted == false &&
                                                o.active == true &&
                                                o.deleted == false &&
                                                t.active == true &&
                                                t.deleted == false
                                                orderby t.name, o.name, d.name, tm.name
                                                select new BasicListItem()
                                                {
                                                    id = tm.id,
                                                    name = t.name + " - " + o.name + " - " + d.name + " - " + tm.name
                                                })
                                                .ToListAsync();

            return Ok(output);
        }
    }
}

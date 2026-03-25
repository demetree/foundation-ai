using Microsoft.AspNetCore.Mvc;
using Foundation.Security;

namespace Foundation.Scheduler.Controllers.WebAPI
{
    /// <summary>
    /// Partial class extension that disables the code-generated write routes for EventCharge.
    ///
    /// All EventCharge writes MUST go through the FinancialManagementService via
    /// the api/financial/charges endpoints on FinancialTransactionsController.
    ///
    /// The code-generated read routes (GET api/EventCharges, GET api/EventCharge/{id})
    /// remain fully functional.
    ///
    /// AI-Developed — This file was significantly developed with AI assistance.
    /// </summary>
    public partial class EventChargesController
    {
        private const string WRITE_DISABLED_MESSAGE =
            "Direct writes to EventCharge are disabled. " +
            "Use the api/financial/charges endpoints which route through the FinancialManagementService " +
            "for proper transaction handling, fiscal period validation, and audit trail.";

        /// <summary>
        /// Blocks the code-generated POST (create) route.
        /// Callers should use POST api/financial/charges instead.
        /// </summary>
        [HttpPost]
        [Route("api/EventCharge")]
        public new IActionResult PostEventCharge([FromBody] Database.EventCharge.EventChargeDTO eventChargeDTO)
        {
            return BadRequest(new { error = WRITE_DISABLED_MESSAGE });
        }

        /// <summary>
        /// Blocks the code-generated PUT/POST (update) route.
        /// Callers should use PUT api/financial/charges/{id} instead.
        /// </summary>
        [HttpPut]
        [Route("api/EventCharge/{id}")]
        public new IActionResult PutEventCharge(int id, [FromBody] Database.EventCharge.EventChargeDTO eventChargeDTO)
        {
            return BadRequest(new { error = WRITE_DISABLED_MESSAGE });
        }

        /// <summary>
        /// Blocks the code-generated DELETE route.
        /// Callers should use DELETE api/financial/charges/{id} instead.
        /// </summary>
        [HttpDelete]
        [Route("api/EventCharge/{id}")]
        public new IActionResult DeleteEventCharge(int id)
        {
            return BadRequest(new { error = WRITE_DISABLED_MESSAGE });
        }
    }
}

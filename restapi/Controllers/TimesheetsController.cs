using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using restapi.Models;

namespace restapi.Controllers
{
    [Route("[controller]")]
    public class TimesheetsController : Controller
    {
        [HttpGet]
        [Produces(ContentTypes.Timesheets)]
        [ProducesResponseType(typeof(IEnumerable<Timecard>), 200)]
        public IEnumerable<Timecard> GetAll()
        {
            //var timecard1 = new Timecard(2);
            //Database.Add(timecard1);
            
            return Database
                .All
                .OrderBy(t => t.Opened);
        }

        [HttpGet("{id}")]
        [Produces(ContentTypes.Timesheet)]
        [ProducesResponseType(typeof(Timecard), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetOne(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null) 
            {
                return Ok(timecard);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Produces(ContentTypes.Timesheet)]
        [ProducesResponseType(typeof(Timecard), 200)]
        public Timecard Create([FromBody] DocumentResource resource)
        {
            var timecard = new Timecard(resource.Resource);

            var entered = new Entered() { Resource = resource.Resource };

            timecard.Transitions.Add(new Transition(entered));

            Database.Add(timecard);

            return timecard;
        }

        [HttpGet("{id}/lines")]
        [Produces(ContentTypes.TimesheetLines)]
        [ProducesResponseType(typeof(IEnumerable<AnnotatedTimecardLine>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetLines(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                var lines = timecard.Lines
                    .OrderBy(l => l.WorkDate)
                    .ThenBy(l => l.Recorded);

                return Ok(lines);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/lines")]
        [Produces(ContentTypes.TimesheetLine)]
        [ProducesResponseType(typeof(AnnotatedTimecardLine), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        public IActionResult AddLine(string id, [FromBody] TimecardLine timecardLine)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Draft)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }

                var annotatedLine = timecard.AddLine(timecardLine);

                return Ok(annotatedLine);
            }
            else
            {
                return NotFound();
            }
        }
        
        [HttpGet("{id}/transitions")]
        [Produces(ContentTypes.Transitions)]
        [ProducesResponseType(typeof(IEnumerable<Transition>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetTransitions(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                return Ok(timecard.Transitions);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/submittal")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Submit(string id, [FromBody] Submittal submittal)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Draft)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }

                if (timecard.Lines.Count < 1)
                {
                    return StatusCode(409, new EmptyTimecardError() { });
                }
                
                var transition = new Transition(submittal, TimecardStatus.Submitted);
                timecard.Transitions.Add(transition);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/submittal")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetSubmittal(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Submitted)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Submitted)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);                                        
                }
                else 
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/cancellation")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Cancel(string id, [FromBody] Cancellation cancellation)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Draft && timecard.Status != TimecardStatus.Submitted)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }
                
                var transition = new Transition(cancellation, TimecardStatus.Cancelled);
                timecard.Transitions.Add(transition);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/cancellation")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetCancellation(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Cancelled)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Cancelled)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);                                        
                }
                else 
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/rejection")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Close(string id, [FromBody] Rejection rejection)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Submitted)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }
                
                var transition = new Transition(rejection, TimecardStatus.Rejected);
                timecard.Transitions.Add(transition);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/rejection")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetRejection(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Rejected)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Rejected)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);                                        
                }
                else 
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }
        
        [HttpPost("{id}/approval")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Approve(string id, [FromBody] Approval approval)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Submitted)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }
                
                var transition = new Transition(approval, TimecardStatus.Approved);
                timecard.Transitions.Add(transition);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/approval")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetApproval(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Approved)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Approved)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);                                        
                }
                else 
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// function to delete a timecard 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deletion"></param>
        /// <returns></returns>
        [HttpDelete("{id}/deletion")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        //[ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Delete(string id, [FromBody] Deletion deletion)
        {
            Timecard timecard = Database.Find(id);            

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Draft && timecard.Status != TimecardStatus.Submitted)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }

                var transition = new Transition(deletion, TimecardStatus.Deleted);
                timecard.Transitions.Add(transition);
                Database.Remove(timecard);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Insert a line to existing timecard
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timecardLine"></param>
        /// <returns></returns>
        [HttpPost("{id}/TimecardLine")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition),200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(InvalidStateError),409)]
        public IActionResult TimecardLine(string id,[FromBody] TimecardLine timecardLine)
        {
            Timecard timecard = Database.Find(id);
            
            //logic to check for timecard first
            if(timecard != null && timecard.Status == TimecardStatus.Draft)
            { 
                
               var result = timecard.AddLine(timecardLine);
               return Ok(result);

            }else if(timecard.Status == TimecardStatus.Approved || timecard.Status == TimecardStatus.Rejected || timecard.Status == TimecardStatus.Submitted)
            {
                return NotFound("no entry allow");
            }
            else if (timecard.Status == TimecardStatus.Cancelled || timecard.Status == TimecardStatus.Deleted)
            {
                return NotFound("no timecard found");
            }
            else
            {
                return null;
            }
            
            
        }

        /// <summary>
        /// Update existing timecardline
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timecardLine"></param>
        /// <returns></returns>
        [HttpPatch("{id}/{lineNumber}/UpdateTimecardLine")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        public IActionResult UpdateTimecardLine(string id,string lineNumber, [FromBody] TimecardLine timecardLine)
        {
            Timecard timecard = Database.Find(id);

            //logic to check for timecard first
            if (timecard != null && timecard.Status == TimecardStatus.Draft)
            {

                var result = timecard.UpdateLine(Convert.ToInt32(lineNumber),timecardLine);
                return Ok(result);

            }
            else if (timecard.Status == TimecardStatus.Approved || timecard.Status == TimecardStatus.Rejected || timecard.Status == TimecardStatus.Submitted)
            {
                return NotFound("no update allow");
            }
            else if (timecard.Status == TimecardStatus.Cancelled || timecard.Status == TimecardStatus.Deleted)
            {
                return NotFound("no timecard found");
            }
            else
            {
                return null;
            }

        }
    }
}

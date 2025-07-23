using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var departments = db.Departments
                .Select(d => new {
                    name = d.Name,
                    subject = d.Subject
                })
                .ToArray();
            return Json(departments);
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalog = db.Departments
                .Select(dept => new {
                    subject = dept.Subject,
                    dname = dept.Name,
                    courses = db.Courses
                        .Where(c => c.Department == dept.Subject)
                        .Select(c => new {
                            number = c.Number,
                            cname = c.Name
                        }).ToList()
                })
                .ToArray();
            return Json(catalog);
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            uint courseNumber = (uint)number;
            
            var query = from c in db.Classes
                        where c.ListingNavigation.Department == subject &&
                              c.ListingNavigation.Number == courseNumber
                        select new
                        {
                            season = c.Season,
                            year = c.Year,
                            location = c.Location,
                            start = c.StartTime.ToString(),
                            end = c.EndTime.ToString(),
                            fname = c.TaughtByNavigation != null ? c.TaughtByNavigation.FName : "",
                            lname = c.TaughtByNavigation != null ? c.TaughtByNavigation.LName : ""
                        };
            
            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            
            var assignment = (from a in db.Assignments
                            join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                            join c in db.Classes on ac.InClass equals c.ClassId
                            where c.ListingNavigation.Department == subject &&
                                  c.ListingNavigation.Number == courseNumber &&
                                  c.Season == season &&
                                  c.Year == classYear &&
                                  ac.Name == category &&
                                  a.Name == asgname
                            select a).FirstOrDefault();
            
            return Content(assignment?.Contents ?? "");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            uint courseNumber = (uint)num;
            uint classYear = (uint)year;
            
            var submission = (from s in db.Submissions
                            join a in db.Assignments on s.Assignment equals a.AssignmentId
                            join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                            join c in db.Classes on ac.InClass equals c.ClassId
                            where s.Student == uid &&
                                  c.ListingNavigation.Department == subject &&
                                  c.ListingNavigation.Number == courseNumber &&
                                  c.Season == season &&
                                  c.Year == classYear &&
                                  ac.Name == category &&
                                  a.Name == asgname
                            select s).FirstOrDefault();
            
            return Content(submission?.SubmissionContents ?? "");
        }

        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var student = db.Students.FirstOrDefault(s => s.UId == uid);
            if (student != null)
            {
                return Json(new {
                    fname = student.FName,
                    lname = student.LName,
                    uid = student.UId,
                    department = student.MajorNavigation.Name
                });
            }
            var professor = db.Professors.FirstOrDefault(p => p.UId == uid);
            if (professor != null)
            {
                return Json(new {
                    fname = professor.FName,
                    lname = professor.LName,
                    uid = professor.UId,
                    department = professor.WorksInNavigation.Name
                });
            }
            var admin = db.Administrators.FirstOrDefault(a => a.UId == uid);
            if (admin != null)
            {
                return Json(new {
                    fname = admin.FName,
                    lname = admin.LName,
                    uid = admin.UId
                });
            }
            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}


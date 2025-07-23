using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LMSControllerTests
{
    public class UnitTest1
    {
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
           // An example of a simple unit test on the CommonController
           CommonController ctrl = new CommonController(MakeTinyDB());

           var allDepts = ctrl.GetDepartments() as JsonResult;

           // Cast to the actual type instead of using dynamic
           var departments = allDepts.Value as object[];
           
           Assert.NotNull(departments);
           Assert.Equal(1, departments.Length);
           
           // Use reflection to access the anonymous object properties
           var firstDept = departments[0];
           var deptType = firstDept.GetType();
           var subjectProperty = deptType.GetProperty("subject");
           Assert.NotNull(subjectProperty);
           var subjectValue = (string)subjectProperty.GetValue(firstDept);
           Assert.Equal("CS", subjectValue);
        }

        [Fact]
        public void CreateDepartment_NewDepartment_ReturnsSuccess()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act
            var result = ctrl.CreateDepartment("MATH", "Mathematics") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            
            // Use reflection to access the anonymous object properties
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.True(successValue);
        }

        [Fact]
        public void CreateDepartment_DuplicateDepartment_ReturnsFalse()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act - Try to create CS department that already exists
            var result = ctrl.CreateDepartment("CS", "Computer Science") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            
            // Use reflection to access the anonymous object properties  
            var resultType = result.Value.GetType();
            var successProperty = resultType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = (bool)successProperty.GetValue(result.Value);
            Assert.False(successValue);
        }

        [Fact]
        public void GetCourses_ValidDepartment_ReturnsCoursesArray()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act
            var result = ctrl.GetCourses("CS") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var courses = result.Value as Array;
            Assert.NotNull(courses);
            Assert.Equal(2, courses.Length); // Should have 2 CS courses
        }

        [Fact]
        public void GetProfessors_ValidDepartment_ReturnsProfessorsArray()
        {
            // Arrange
            AdministratorController ctrl = new AdministratorController(MakeTinyDB());
            
            // Act
            var result = ctrl.GetProfessors("CS") as JsonResult;
            
            // Assert
            Assert.NotNull(result);
            var professors = result.Value as Array;
            Assert.NotNull(professors);
            Assert.Equal(1, professors.Length); // Should have 1 CS professor
        }


        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
           // Original code - commented out due to MySQL conflict in tests
           // var contextOptions = new DbContextOptionsBuilder<LMSContext>()
           // .UseInMemoryDatabase( "LMSControllerTest" )
           // .ConfigureWarnings( b => b.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
           // .UseApplicationServiceProvider( NewServiceProvider() )
           // .Options;

           // Fixed version for testing - uses unique database name and proper configuration
           var contextOptions = new DbContextOptionsBuilder<LMSContext>()
           .UseInMemoryDatabase( $"LMSControllerTest_{Guid.NewGuid()}" )
           .ConfigureWarnings( b => b.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
           .UseApplicationServiceProvider( NewServiceProvider() )
           .EnableSensitiveDataLogging()
           .Options;

           var db = new LMSContext(contextOptions);

           db.Database.EnsureDeleted();
           db.Database.EnsureCreated();

           db.Departments.Add( new Department { Name = "KSoC", Subject = "CS" } );

           // Add test courses
           db.Courses.Add( new Course { CatalogId = 1, Name = "Database Systems", Number = 5530, Department = "CS" } );
           db.Courses.Add( new Course { CatalogId = 2, Name = "Software Engineering", Number = 3500, Department = "CS" } );

           // Add test professor
           db.Professors.Add( new Professor { UId = "u1234567", FName = "John", LName = "Doe", WorksIn = "CS", Dob = DateTime.Now.AddYears(-40) } );

           db.SaveChanges();

           return db;
        }

        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }
}
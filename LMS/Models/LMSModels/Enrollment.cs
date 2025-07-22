using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels;

public partial class Enrollment
{
    public string StudentUid { get; set; } = null!;

    public string SubjectAbbr { get; set; } = null!;

    public int CourseNumber { get; set; }

    public uint Year { get; set; }

    public string Season { get; set; } = null!;

    public string? Grade { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Student StudentU { get; set; } = null!;
}

# Final project for Programming in C# (`NPRG035`) and other C# courses (`NPRG038`, `NPRG057`, `NPRG064`)
This repository should contain all work related to your final project for all relevant C# courses. This file specifically lists several important notes regarding the final project. While working on your project, we expect you will completely rewrite this file. Don't forget that Git allows you to see the previous version of the file, and the [source project this repository is forked from](https://gitlab.mff.cuni.cz/teaching/nprg035/2025/student-base) also contains the original version of this file.

(Czech note: Pokud by Vám nějaká z informací v tomto repozitáři nebyla jasná a ani Google Translate nepomůže, ozvěte se prosím svému cvičícímu, který Vám to objasní.)

## Website courses (including detailed information about further final project requirements and deadlines)
(Make sure to check the relevant language variant based on the language of your studies. Differences, however unlikely, may occur.)

- NPRG035 (EN: [Programming in C# language](https://d3s.mff.cuni.cz/teaching/nprg035/) | CZ: [Programování v jazyce C#](https://d3s.mff.cuni.cz/cz/teaching/nprg035/))
- NPRG038 (EN: [Advanced C# Programming](https://d3s.mff.cuni.cz/teaching/nprg038/) | CZ: [Pokročilé programování v jazyce C#](https://d3s.mff.cuni.cz/cz/teaching/nprg038/))
- NPRG057 (CZ: [Advanced .NET Programming II | Pokročilé programování pro .NET II](https://is.cuni.cz/studium/predmety/index.php?do=predmet&kod=NPRG057))
- NPRG064 (CZ: [Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET](https://is.cuni.cz/studium/predmety/index.php?do=predmet&kod=NPRG064))

## Working in this repository
In this repository, your role is set to Developer. This means that you are not allowed to directly push changes into the main branch. Instead, for *relevant changes* (see below), you are supposed to create a merge request from a `dev` branch into the `main` branch and assign it to the *relevant teacher* (see below). The relevant changes are these (performed in this order):

### Submitting the specification
This is done by modifying the [`spec/README.md`](./spec/README.md) file. For further details about the specification, check the content of the linked file.

### Rarely: Submitting specification adjustments after the specification deadline
In rare scenarios, it might be necessary to alter the specification. A non-exhaustive list of reasons includes:
- A specific library or external service that was supposed to be used went out of support, is buggy, non-portable, has licensing or legal issues, or is otherwise not suitable for the project.
- Further research shows that the problem the final project is addressing turned out to be much more difficult and likely can't be completed within a reasonable time span.

In these rare cases, make sure to back your statements, cite resources, or otherwise convince the teacher that the change is appropriate.

### Submitting the implementation
When working on the implementation, make sure to make use of the skills you learned throughout the semester. This includes things like proper program decomposition, object design, writing readable code, naming identifiers, etc.

Do not forget that documentation is a crucial part of the final project. When creating the merge request, make sure to point out where the documentation can be found. The documentation typically consists of three parts:

#### User guide, likely in [`README.md`](./README.md)
The main purpose of the user guide is to provide instructions to the relevant target audience on how to work with the application. Based on the target audience, a different level of detail might be required (a library for sound processing and a mobile app for exercise have different target audiences). This document should answer questions like how to start the application, what arguments to pass, what to click on, where files are stored, what the format of the files is (if the user is expected to edit them), what the API is, where to store configuration, etc. Screenshots of relevant parts of the UI can also be included.

Other forms of the user guide are also accepted. For example, a game can have a dedicated in-game tutorial; a GUI-heavy application might have extensive tooltips directly in the application. Utilizing GitLab wiki pages is also possible.

#### Developer guide, likely in [`docs/Developer.md`](./docs/Developer.md)
Imagine a scenario where a different developer of your skill level, who has never seen this project, would like to contribute to your source code. To make the new developer more accustomed to your code, it is useful to have a developer guide that answers questions like how the application is designed, what the layout of the application is, what (C#) projects are in the solution, what algorithms are used, how to extend the application, and which components handle user input or communicate with the database.

Again, other forms of the developer guide (diagrams, slides, ...) can also be accepted.

#### Documentation comments, directly in code
While the developer guide serves as a good introduction to the codebase and basically creates a mental unidirectional mapping *features => project/class/method*, it is also helpful to have the other half of the mapping (*method => functionality*). This is done by providing [documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments), starting with `///` directly in the source code. This typically allows IDEs to display this information when mousing over a method or a type. It is usually sufficient to document publicly available types (`class`, `struct`, `interface`) and members (methods, properties).

The documentation comment is usually rather short and only describes what the method does, what the [parameters](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d39-param) are, what the [return value](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d313-returns) is, and what [exceptions](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d35-exception) are possibly thrown.

Here is an example of a not-so-useful documentation comment:

```csharp
/// <summary>
/// Grades the answer.
/// </summary>
/// <param name="answer">The answer or null.</param>
/// <returns>The grade.</returns>
public abstract double Grade(string? answer);
```

Compare it to this more verbose one:
```csharp
/// <summary>
/// Grades the answer by assigning a score based on the correctness of the user's input.
/// If the user decides to skip this question, the <paramref name="answer"/> is <c>null</c>.
/// </summary>
/// <param name="answer">Whatever the user provided as their answer, with leading and trailing whitespaces removed, or <c>null</c> if the user decided to skip this question.</param>
/// <returns>The answer's score, in the range [0, 1].</returns>
public abstract double Grade(string? answer);
```

### Occasionally: Fixing the implementation after the presentation
In some cases, problems in the implementation may arise during the presentation. This might include a bug in the implementation of a specific feature, a certain part of the documentation not being clear, or some part of the source not being [good code](https://imgs.xkcd.com/comics/good_code.png). In these scenarios, you are expected to revise the implementation and create another merge request that fixes these problems.

---
When creating the merge request, you are supposed to assign the request itself to the *relevant teacher*. Who the relevant teacher is depends on which courses you plan to get credit for using this final project. The logic that selects the *relevant teacher* can be summarized like this:

| List of courses to complete | Teacher |
| ------------- | ------------- |
| includes `NPRG057` | Pavel Ježek |
| includes `NPRG038` but not `NPRG057` | `NPRG038` Teacher |
| otherwise | `NPRG035` Teacher |

Therefore, `NPRG064` doesn't affect the *relevant teacher* (and any teacher can assess whether the final project meets these requirements). In rare occasions, it is possible to choose different teacher. These occasions would usually be: repeating the course, project being part of ISP (`NPRG045`) with different C# teacher as a supervisor, and similar. Make sure to discuss this in advance, before assigning the merge request.

Note that this is to keep the workload balanced across the teachers.

## FAQ

#### What can I do as a final project? Is there a list of projects to choose from?
Any form of a program can be accepted as a final project. This includes a library, web app, phone app, game, console app, ...
The topic is for you to decide. From our experience, most of the final projects are based on *I have a hobby and I could use this kind of tool*.

#### Can I use external libraries for my project?
Yes, if it makes sense. If your topic is *Library for graph algorithms*, then using a library for graph algorithms is likely not sensible. On the other hand, if your topic is *WebApp for monitoring power outages*, then using a plotting library is perfectly sensible.

To be concrete, using Unity, Godot, MonoGame, WPF, Avalonia, etc., is allowed.

#### How to present the final project?
Contact your *relevant teacher*, schedule a date and means (in person or over Zoom) for the presentation, and prepare your presentation.

The form of the presentation depends on the kind of project. Presenting a project with a TUI/GUI is usually done by running the application itself and showcasing the features. For a library, presenting a typical use-case scenario, API, and some parts of the developer guide might be more useful.

Note that scheduling a presentation may take some time, especially during the holiday season. It is not impossible that it will take two weeks for you to present your program after you finish the implementation. There might be some state examinations happening during this period, so make sure to communicate with your teacher in advance if you need to present your program before a specific day if you are finishing your studies.

#### I am retaking a course and I have work in a different repository --- what should I do?
If you did pass the requirements for the final project in a previous year, you don't need to repeat that part of the course again. If you didn't pass this requirement, you are, in principle, starting with no progress on the final project. Therefore, follow the current year's deadlines, the current year's *relevant teacher*, and manually transfer your work (specification, implementation) from the other repository to this one (including merge requests).
